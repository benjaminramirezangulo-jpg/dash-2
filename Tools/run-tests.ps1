param([string]$Compiler='D:\DashWork\roslyn\tasks\net472\csc.exe')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
New-Item -ItemType Directory -Force (Join-Path $root 'Build'),(Join-Path $root 'TestResults') | Out-Null
& $Compiler /nologo /target:exe /optimize+ /langversion:latest ('/out:'+(Join-Path $root 'Build\VelocityTests.exe')) (Join-Path $root 'Assets\scripts\Velocity\VelocityModel.cs') (Join-Path $root 'Assets\scripts\Velocity\VelocityRecording.cs') (Join-Path $root 'Tests\VelocityTests.cs')
if($LASTEXITCODE -ne 0){throw 'Regression compilation failed'}
& (Join-Path $root 'Build\VelocityTests.exe') (Join-Path $root 'TestResults\model-tests.xml')
if($LASTEXITCODE -ne 0){throw 'Regression tests failed'}
