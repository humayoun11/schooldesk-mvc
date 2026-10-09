[CmdletBinding()]
param([switch]$Demo)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$app=Join-Path $root 'SchoolDesk'
if($env:OS -ne 'Windows_NT'){throw 'SchoolDesk MVC 5 requires Windows, .NET Framework 4.8, SQL Server LocalDB and IIS Express.'}
if(-not (Get-Command dotnet -ErrorAction SilentlyContinue)){throw 'Install a supported .NET SDK (8 or later), restart PowerShell, and rerun setup.'}
if(-not (Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue)){throw 'Install SQL Server Express LocalDB using Visual Studio Installer > Individual components > SQL Server Express LocalDB.'}
& SqlLocalDB.exe start MSSQLLocalDB
if($LASTEXITCODE -ne 0){throw 'LocalDB could not start. Check that the MSSQLLocalDB instance exists.'}
Add-Type -AssemblyName System.Data
function Open-Db([string]$database){$c=[System.Data.SqlClient.SqlConnection]::new("Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=$database;Integrated Security=True");$c.Open();return $c}
function Execute-Sql($connection,[string]$sql,$transaction=$null){$cmd=$connection.CreateCommand();$cmd.CommandText=$sql;$cmd.CommandTimeout=60;if($transaction){$cmd.Transaction=$transaction};try{[void]$cmd.ExecuteNonQuery()}finally{$cmd.Dispose()}}
$master=Open-Db 'master'
try{Execute-Sql $master "IF DB_ID(N'SchoolDeskDemo') IS NULL CREATE DATABASE SchoolDeskDemo"}finally{$master.Dispose()}
$c=Open-Db 'SchoolDeskDemo'
try{
 Execute-Sql $c 'IF OBJECT_ID(N''SchemaMigrations'') IS NULL CREATE TABLE SchemaMigrations(Version nvarchar(100) PRIMARY KEY,Checksum nvarchar(64) NOT NULL,AppliedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME())'
 foreach($file in Get-ChildItem (Join-Path $root 'database/migrations') -Filter '*.sql' | Sort-Object Name){
  $checksum=(Get-FileHash $file.FullName -Algorithm SHA256).Hash
  $cmd=$c.CreateCommand();$cmd.CommandText='SELECT Checksum FROM SchemaMigrations WHERE Version=@v';[void]$cmd.Parameters.AddWithValue('@v',$file.Name);$existing=$cmd.ExecuteScalar();$cmd.Dispose()
  if($existing){if($existing -ne $checksum){throw "Applied migration was changed: $($file.Name). Restore its original contents."};continue}
  $tx=$c.BeginTransaction()
  try{Execute-Sql $c ([IO.File]::ReadAllText($file.FullName)) $tx;$cmd=$c.CreateCommand();$cmd.Transaction=$tx;$cmd.CommandText='INSERT SchemaMigrations(Version,Checksum) VALUES(@v,@h)';[void]$cmd.Parameters.AddWithValue('@v',$file.Name);[void]$cmd.Parameters.AddWithValue('@h',$checksum);[void]$cmd.ExecuteNonQuery();$cmd.Dispose();$tx.Commit();Write-Host "Applied $($file.Name)"}catch{$tx.Rollback();throw}finally{$tx.Dispose()}
 }
}finally{$c.Dispose()}
function Random-Secret(){ $bytes=New-Object byte[] 32;$rng=[Security.Cryptography.RandomNumberGenerator]::Create();try{$rng.GetBytes($bytes)}finally{$rng.Dispose()};return [Convert]::ToBase64String($bytes)}
$secretPath=Join-Path $app 'App_Data/local.settings.config'
if(-not(Test-Path $secretPath)){[IO.File]::WriteAllText($secretPath,"<appSettings><add key=`"JwtSigningKey`" value=`"$(Random-Secret)`" /></appSettings>")}
if($Demo){ & (Join-Path $PSScriptRoot 'Seed-Demo.ps1') }
Push-Location $app
try{& dotnet restore SchoolDesk.csproj;if($LASTEXITCODE -ne 0){throw 'NuGet restore failed. Check the network and NuGet output.'};& dotnet build SchoolDesk.csproj --no-restore -c Release;if($LASTEXITCODE -ne 0){throw 'Build failed. Share the compiler error without any secrets.'}}finally{Pop-Location}
$binding=Join-Path $app 'bin/SchoolDesk.dll.config'
if(Test-Path $binding){[xml]$generated=Get-Content $binding;[xml]$web=Get-Content (Join-Path $app 'Web.config');if($generated.configuration.runtime){$old=$web.configuration.SelectSingleNode('runtime');if($old){[void]$web.configuration.RemoveChild($old)};[void]$web.configuration.AppendChild($web.ImportNode($generated.configuration.runtime,$true));$web.Save((Join-Path $app 'Web.config'))}}
Write-Host 'Setup complete. Run: powershell -ExecutionPolicy Bypass -File .\scripts\Run.ps1'
