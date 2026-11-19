param([string]$Compiler='D:\DashWork\roslyn\tasks\net472\csc.exe')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$managed=Join-Path $root 'Executable\DashAndFight_Data\Managed'
$build=Join-Path $root 'Build'
New-Item -ItemType Directory -Force $build | Out-Null
$references=Get-ChildItem $managed -Filter '*.dll' | Where-Object { $_.Name -ne 'Assembly-CSharp.dll' } | ForEach-Object { '/reference:'+$_.FullName }
$sources=Get-ChildItem (Join-Path $root 'Assets\scripts') -Filter '*.cs' -Recurse | ForEach-Object {$_.FullName}
& $Compiler /nologo /target:library /optimize+ /langversion:latest /nostdlib+ ('/out:'+(Join-Path $build 'Assembly-CSharp.dll')) $references $sources
if($LASTEXITCODE -ne 0){throw 'Gameplay compilation failed'}
Copy-Item -LiteralPath (Join-Path $build 'Assembly-CSharp.dll') -Destination (Join-Path $managed 'Assembly-CSharp.dll') -Force
$streaming=Join-Path $root 'Executable\DashAndFight_Data\StreamingAssets\VelocityAudio'
New-Item -ItemType Directory -Force $streaming | Out-Null
Copy-Item (Join-Path $root 'Assets\StreamingAssets\VelocityAudio\*.wav') $streaming -Force
Write-Output 'Compiled gameplay against the exact Unity 6000.6.7f1 player libraries.'
