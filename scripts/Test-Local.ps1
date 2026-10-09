[CmdletBinding()]
param([string]$BaseUrl='http://localhost:5080')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$credentialPath=Join-Path $root 'SchoolDesk/App_Data/demo-credentials.txt'
if(-not(Test-Path $credentialPath)){throw 'Run Setup.ps1 -Demo first. The generated credentials file is required.'}
$accounts=@{}
foreach($line in Get-Content $credentialPath){if($line -match '^\w+: ([^ ]+) \| (.+)$'){$accounts[$Matches[1]]=$Matches[2]}}
$passed=0
function Assert-True([bool]$condition,[string]$description){if(-not $condition){throw "FAILED: $description"};$script:passed++;Write-Host "PASS: $description"}
function Status($uri,$session,$method='GET',$body=$null,$headers=@{}){
 try{$requestParams=@{Uri=$uri;Method=$method;UseBasicParsing=$true;Headers=$headers};if($session){$requestParams.WebSession=$session};if($body){$requestParams.Body=$body};$response=Invoke-WebRequest @requestParams;return [int]$response.StatusCode}catch{if($_.Exception.Response){return [int]$_.Exception.Response.StatusCode};throw}
}
function Login([string]$email){
 $session=New-Object Microsoft.PowerShell.Commands.WebRequestSession
 $page=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/Account/Login" -WebSession $session
 if($page.Content -notmatch 'name="__RequestVerificationToken"[^>]*value="([^"]+)"'){throw 'Login form token missing.'}
 $token=[System.Net.WebUtility]::HtmlDecode($Matches[1])
 $response=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/Account/Login" -Method POST -WebSession $session -Body @{Email=$email;Password=$accounts[$email];__RequestVerificationToken=$token}
 Assert-True ($response.Content -match 'Good things start with clarity') "Login for $email"
 return $session
}
function Get-Token([string]$email){$r=Invoke-RestMethod -Uri "$BaseUrl/api/token" -Method POST -Body @{Email=$email;Password=$accounts[$email]};return $r.access_token}
$admin=Login 'admin@schooldesk.example'
$teacher=Login 'teacher@schooldesk.example'
$parent=Login 'parent@schooldesk.example'
$student=Login 'student@schooldesk.example'
$accountant=Login 'accountant@schooldesk.example'
Assert-True ((Status "$BaseUrl/School/Users" $teacher) -eq 403) 'Teacher cannot manage users'
Assert-True ((Status "$BaseUrl/School/Student" $parent) -eq 403) 'Parent cannot create/edit admissions'
Assert-True ((Status "$BaseUrl/School/Fees" $teacher) -eq 403) 'Teacher cannot view finance'
Assert-True ((Status "$BaseUrl/School/Reports" $accountant) -eq 403) 'Accountant cannot view exam results'
$parentStudents=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/School/Students" -WebSession $parent
Assert-True ($parentStudents.Content -match 'Taylor Parker' -and $parentStudents.Content -notmatch 'Casey Lane') 'Parent sees own children only'
$studentStudents=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/School/Students" -WebSession $student
Assert-True ($studentStudents.Content -match 'Taylor Parker' -and $studentStudents.Content -notmatch 'Riley Parker') 'Student sees own record only'
$teacherStudents=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/School/Students" -WebSession $teacher
Assert-True ($teacherStudents.Content -match 'Casey Lane' -and $teacherStudents.Content -notmatch 'Jordan Cole') 'Teacher sees assigned class only'
$teacherExams=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/School/Classes" -WebSession $teacher
Assert-True ($teacherExams.Content -notmatch 'Grade 8') 'Other teacher class absent from class list'
Assert-True ((Status "$BaseUrl/api/students" $null) -eq 401) 'API rejects missing bearer token'
Assert-True ((Status "$BaseUrl/api/students" $admin) -eq 401) 'Browser cookie does not authorise API'
Assert-True ((Status "$BaseUrl/api/students" $null 'GET' $null @{Authorization='Bearer invalid.token.value'}) -eq 401) 'API rejects invalid token'
$teacherToken=Get-Token 'teacher@schooldesk.example'
$teacherApi=Invoke-RestMethod -Uri "$BaseUrl/api/students" -Headers @{Authorization="Bearer $teacherToken"}
Assert-True (@($teacherApi.data).Count -eq 5) 'Teacher API scope returns assigned demo class'
$parentToken=Get-Token 'parent@schooldesk.example'
$parentApi=Invoke-RestMethod -Uri "$BaseUrl/api/students" -Headers @{Authorization="Bearer $parentToken"}
Assert-True (@($parentApi.data).Count -eq 2) 'Parent API scope returns two linked children'
Assert-True ((Status "$BaseUrl/School/CreateUser" $admin 'POST' @{FullName='Blocked';Email='blocked@example.com';Password='ExamplePassword123!';Role='Admin'}) -eq 400) 'State-changing form rejects missing CSRF token'
# Look up the other class and foreign invoice dynamically to avoid relying on identity values.
$classHtml=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/School/Attendance" -WebSession $admin
if($classHtml.Content -match 'value="(\d+)"[^>]*>Grade 8 / B'){
 $otherClass=$Matches[1]
 Assert-True ((Status "$BaseUrl/School/Attendance?classId=$otherClass" $teacher) -eq 403) 'Teacher cannot open another class by changing ID'
}else{throw 'Expected demo class not found; run on an unchanged fictional demo database.'}
$fees=Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/School/Fees?q=Casey" -WebSession $admin
if($fees.Content -match 'href="/School/Receipt/(\d+)"|href="/School/Receipt\?id=(\d+)"'){
 $foreignInvoice=if($Matches[1]){$Matches[1]}else{$Matches[2]}
 Assert-True ((Status "$BaseUrl/School/Receipt/$foreignInvoice" $parent) -eq 404) 'Parent cannot open another student invoice by changing ID'
}else{throw 'Expected demo invoice not found; run on an unchanged fictional demo database.'}
[void](Invoke-RestMethod -Uri "$BaseUrl/Api/Revoke" -Method POST -Headers @{Authorization="Bearer $parentToken"})
Assert-True ((Status "$BaseUrl/api/students" $null 'GET' $null @{Authorization="Bearer $parentToken"}) -eq 401) 'Revoked JWT rejected immediately'
Write-Host "Completed: $passed checks passed. No business records created."
