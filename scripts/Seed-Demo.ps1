$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
Add-Type -AssemblyName System.Data
$c=[System.Data.SqlClient.SqlConnection]::new('Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=SchoolDeskDemo;Integrated Security=True');$c.Open()
if($c.Database -ne 'SchoolDeskDemo'){$c.Dispose();throw 'Demo seeding is restricted to SchoolDeskDemo.'}
$cmd=$c.CreateCommand();$cmd.CommandText='SELECT COUNT(*) FROM Users';$count=[int]$cmd.ExecuteScalar();$cmd.Dispose()
if($count -gt 0){$c.Dispose();Write-Host 'Users already exist. Demo seeding skipped; existing records and passwords were not changed.';return}
function New-Password { $b=New-Object byte[] 18;$r=[Security.Cryptography.RandomNumberGenerator]::Create();try{$r.GetBytes($b)}finally{$r.Dispose()};return ([Convert]::ToBase64String($b)+'aA7!') }
function Password-Hash([string]$password){$salt=New-Object byte[] 16;$r=[Security.Cryptography.RandomNumberGenerator]::Create();try{$r.GetBytes($salt)}finally{$r.Dispose()};$k=[Security.Cryptography.Rfc2898DeriveBytes]::new($password,$salt,210000,[Security.Cryptography.HashAlgorithmName]::SHA256);try{return 'pbkdf2-sha256$210000$'+[Convert]::ToBase64String($salt)+'$'+[Convert]::ToBase64String($k.GetBytes(32))}finally{$k.Dispose()}}
$accounts=@(@{Name='Alex Morgan';Email='admin@schooldesk.example';Role='Admin'},@{Name='Maya Reed';Email='teacher@schooldesk.example';Role='Teacher'},@{Name='Oliver Chen';Email='teacher2@schooldesk.example';Role='Teacher'},@{Name='Sam Harper';Email='accountant@schooldesk.example';Role='Accountant'},@{Name='Jamie Parker';Email='parent@schooldesk.example';Role='Parent'},@{Name='Taylor Parker';Email='student@schooldesk.example';Role='Student'})
$credentials=@('SchoolDesk local fictional demo accounts','Keep this file private. It is excluded from Git.','')
$tx=$c.BeginTransaction()
try{
 foreach($a in $accounts){$password=New-Password;$cmd=$c.CreateCommand();$cmd.Transaction=$tx;$cmd.CommandText='INSERT Users(FullName,Email,PasswordHash,Role) VALUES(@n,@e,@p,@r)';[void]$cmd.Parameters.AddWithValue('@n',$a.Name);[void]$cmd.Parameters.AddWithValue('@e',$a.Email);[void]$cmd.Parameters.AddWithValue('@p',(Password-Hash $password));[void]$cmd.Parameters.AddWithValue('@r',$a.Role);[void]$cmd.ExecuteNonQuery();$cmd.Dispose();$credentials+= "$($a.Role): $($a.Email) | $password"}
 $sql=@'
 DECLARE @admin int=(SELECT Id FROM Users WHERE Email='admin@schooldesk.example');
 DECLARE @teacher int=(SELECT Id FROM Users WHERE Email='teacher@schooldesk.example');
 DECLARE @teacher2 int=(SELECT Id FROM Users WHERE Email='teacher2@schooldesk.example');
 DECLARE @parent int=(SELECT Id FROM Users WHERE Email='parent@schooldesk.example');
 DECLARE @student int=(SELECT Id FROM Users WHERE Email='student@schooldesk.example');
 INSERT Classes(Name,Section,TeacherId) VALUES('Grade 7','A',@teacher),('Grade 8','B',@teacher2);
 DECLARE @classA int=(SELECT Id FROM Classes WHERE Name='Grade 7');
 DECLARE @classB int=(SELECT Id FROM Classes WHERE Name='Grade 8');
 INSERT Students(AdmissionNumber,FullName,BirthDate,ClassId,GuardianName,ParentUserId,UserId) VALUES
 ('SD-001','Taylor Parker','2013-03-15',@classA,'Jamie Parker',@parent,@student),
 ('SD-002','Riley Parker','2013-09-02',@classA,'Jamie Parker',@parent,NULL),
 ('SD-003','Casey Lane','2013-05-19',@classA,'Jordan Lane',NULL,NULL),
 ('SD-004','Avery Brooks','2013-01-22',@classA,'Morgan Brooks',NULL,NULL),
 ('SD-005','Quinn Ellis','2013-08-09',@classA,'Alex Ellis',NULL,NULL),
 ('SD-006','Jordan Cole','2012-02-14',@classB,'Sam Cole',NULL,NULL),
 ('SD-007','Robin Hayes','2012-06-23',@classB,'Jamie Hayes',NULL,NULL),
 ('SD-008','Cameron Blake','2012-11-07',@classB,'Chris Blake',NULL,NULL);
 INSERT Invoices(StudentId,Description,Amount,DueDate,CreatedBy) SELECT Id,'Monthly tuition',8500,DATEADD(day,14,CONVERT(date,GETDATE())),@admin FROM Students;
 INSERT Payments(InvoiceId,Amount,Reference,RecordedBy) SELECT Id,3000,'DEMO-RCPT-'+CAST(Id AS varchar(12)),@admin FROM Invoices WHERE Id IN(SELECT TOP 3 Id FROM Invoices ORDER BY Id);
 INSERT Attendance(StudentId,Day,Status,RecordedBy) SELECT s.Id,CONVERT(date,GETDATE()),CASE WHEN s.AdmissionNumber='SD-004' THEN 'Absent' ELSE 'Present' END,c.TeacherId FROM Students s JOIN Classes c ON c.Id=s.ClassId;
 INSERT Exams(ClassId,Title,Subject,MaxMarks,ExamDate,CreatedBy) VALUES(@classA,'Term assessment','Mathematics',100,CONVERT(date,GETDATE()),@teacher),(@classB,'Term assessment','Science',100,CONVERT(date,GETDATE()),@teacher2);
 INSERT Results(ExamId,StudentId,Marks,RecordedBy) SELECT e.Id,s.Id,70+(s.Id%25),e.CreatedBy FROM Exams e JOIN Students s ON s.ClassId=e.ClassId;
 INSERT AuditLogs(ActorId,Action,Entity) VALUES(@admin,'Seed fictional demo','School');
'@
 $cmd=$c.CreateCommand();$cmd.Transaction=$tx;$cmd.CommandText=$sql;[void]$cmd.ExecuteNonQuery();$cmd.Dispose();$tx.Commit()
}catch{$tx.Rollback();throw}finally{$tx.Dispose();$c.Dispose()}
$path=Join-Path $root 'SchoolDesk/App_Data/demo-credentials.txt'
[IO.File]::WriteAllLines($path,$credentials)
Write-Host "Demo accounts created. Passwords are saved privately at: $path"
