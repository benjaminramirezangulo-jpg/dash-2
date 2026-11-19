param([string]$Compiler='D:\DashWork\roslyn\tasks\net472\csc.exe')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'run-tests.ps1') -Compiler $Compiler
& $Compiler /nologo /target:exe /optimize+ /langversion:latest ('/out:'+(Join-Path $root 'Build\FlowWitness.exe')) (Join-Path $root 'Assets\scripts\Velocity\VelocityModel.cs') (Join-Path $root 'Assets\scripts\Velocity\VelocityRecording.cs') (Join-Path $root 'Tests\FlowWitness.cs')
if($LASTEXITCODE -ne 0){throw 'Flow controller compilation failed'}
& (Join-Path $root 'Build\FlowWitness.exe') (Join-Path $root 'TestResults\flow-witness.csv')
if($LASTEXITCODE -ne 0){throw 'At least one stage has no input-only completion witness'}
& (Join-Path $PSScriptRoot 'build-player.ps1') -Compiler $Compiler
$results=Join-Path $root 'TestResults\runtime'
$log=Join-Path $root 'TestResults\player.log'
$player=Start-Process -FilePath (Join-Path $root 'Executable\DashAndFight.exe') -ArgumentList @('-screen-fullscreen','0','-screen-width','1600','-screen-height','1000','-force-d3d11','-logFile',('"'+$log+'"'),'--dash-tests',('"'+$results+'"')) -WindowStyle Hidden -PassThru
$deadline=(Get-Date).AddMinutes(3)
while(-not $player.WaitForExit(1000)){if((Get-Date) -gt $deadline){$player.Kill();throw 'Unity runtime tests exceeded three minutes'}}
if($player.ExitCode -ne 0){throw "Unity runtime tests failed ($($player.ExitCode)). See $log"}
[xml]$runtime=Get-Content (Join-Path $results 'runtime-tests.xml') -Raw
if([int]$runtime.testsuite.failures -ne 0){throw 'Runtime test report contains failures'}
Write-Output "All suites passed, including $($runtime.testsuite.tests) real Unity player checks and six completion witnesses."
