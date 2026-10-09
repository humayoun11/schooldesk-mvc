[CmdletBinding()]
param([ValidateRange(1024,65535)][int]$Port=5080)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$app=Join-Path $root 'SchoolDesk'
if(-not(Test-Path (Join-Path $app 'bin/SchoolDesk.dll'))){throw 'Run scripts/Setup.ps1 -Demo first.'}
$paths=@("$env:ProgramFiles\IIS Express\iisexpress.exe","${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe")
$iis=$paths | Where-Object {Test-Path $_} | Select-Object -First 1
if(-not $iis){throw 'Install IIS Express using Visual Studio Installer > ASP.NET and web development.'}
$url="http://localhost:$Port"
Write-Host "SchoolDesk: $url . Keep this terminal open. Press Ctrl+C to stop."
Start-Process $url
& $iis "/path:$app" "/port:$Port" '/clr:v4.0' '/systray:false'
