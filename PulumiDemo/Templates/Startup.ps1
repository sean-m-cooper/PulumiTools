if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))  
{
    $arguments = " & '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$folder = "C:\SWFiles"
$mountScript = "[MountScript]"
if (-NOT (Test-Path -Path $folder)){
    New-Item -Path "c:\" -Name "SWFiles" -ItemType "directory" 
}

Invoke-WebRequest -Uri "https://[StorageAccount].blob.core.windows.net/[Container]/$mountScript" -OutFile "C:\SWFiles\$mountScript"

Set-Content -Path C:\SWFiles\CreateMountJob.ps1 -Value "`$trigger = New-JobTrigger -AtStartup -RandomDelay 00:00:30"
Add-Content -Path  C:\SWFiles\CreateMountJob.ps1 -Value "Register-ScheduledJob -Trigger $trigger -FilePath C:\SWFiles\$mountScript -Name MountDrive"