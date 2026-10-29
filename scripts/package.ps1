$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$runId = [Guid]::NewGuid().ToString('N')
$publish = Join-Path $root "work\publish\$runId"
$stage = Join-Path $root "work\package\$runId\MiniBibleV2.9"
$output = Join-Path $root 'outputs\mini_bible_windows_v2.9.zip'
$env:DOTNET_CLI_HOME = Join-Path $root 'work'
$env:TEMP = Join-Path $root 'work'
$env:TMP = $env:TEMP

if (Test-Path -LiteralPath $output) { Remove-Item -LiteralPath $output -Force }
New-Item -ItemType Directory -Force -Path $publish,$stage,(Split-Path $output) | Out-Null

$project = Join-Path $root 'src\MiniBible.App\MiniBible.App.csproj'
dotnet restore $project -r win-x64 --ignore-failed-sources -p:NuGetAudit=false
if ($LASTEXITCODE -ne 0) { throw '오프라인 복원이 실패했습니다.' }
dotnet publish $project -c Release -r win-x64 --self-contained true --no-restore -p:DebugType=None -p:DebugSymbols=false -o $publish
if ($LASTEXITCODE -ne 0) { throw '앱 게시가 실패했습니다.' }

Copy-Item -Path (Join-Path $publish '*') -Destination $stage -Recurse
Copy-Item -LiteralPath (Join-Path $root 'distribution\README.txt') -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'distribution\CHANGELOG.txt') -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'distribution\sample_bible.csv') -Destination $stage
Copy-Item -LiteralPath (Join-Path $root 'distribution\sample_bible.json') -Destination $stage
$launchers = @(Get-ChildItem -LiteralPath (Join-Path $root 'distribution') -Filter '*.cmd')
if ($launchers.Count -ne 1) { throw "실행 스크립트 수가 올바르지 않습니다: $($launchers.Count)" }
$launcher = $launchers[0]
Copy-Item -LiteralPath $launcher.FullName -Destination $stage

Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $output -CompressionLevel Optimal
Write-Output $output

