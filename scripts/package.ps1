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
dotnet restore $project --ignore-failed-sources -p:NuGetAudit=false
if ($LASTEXITCODE -ne 0) { throw '오프라인 복원이 실패했습니다.' }
dotnet publish $project -c Release --self-contained false --no-restore -p:DebugType=None -p:DebugSymbols=false -o $publish
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

$dotnetRoot = Split-Path (Get-Command dotnet).Source
$coreVersion = (Get-ChildItem (Join-Path $dotnetRoot 'shared\Microsoft.NETCore.App') -Directory | Sort-Object {[version]$_.Name} -Descending | Select-Object -First 1).Name
$desktopVersion = (Get-ChildItem (Join-Path $dotnetRoot 'shared\Microsoft.WindowsDesktop.App') -Directory | Where-Object Name -eq $coreVersion | Select-Object -First 1).Name
if (-not $desktopVersion) { throw "일치하는 Windows Desktop Runtime $coreVersion을 찾을 수 없습니다." }
$portable = Join-Path $stage 'dotnet'
New-Item -ItemType Directory -Force -Path (Join-Path $portable 'host\fxr'),(Join-Path $portable 'shared\Microsoft.NETCore.App'),(Join-Path $portable 'shared\Microsoft.WindowsDesktop.App') | Out-Null
Copy-Item -LiteralPath (Join-Path $dotnetRoot 'dotnet.exe') -Destination $portable
Copy-Item -LiteralPath (Join-Path $dotnetRoot "host\fxr\$coreVersion") -Destination (Join-Path $portable 'host\fxr') -Recurse
Copy-Item -LiteralPath (Join-Path $dotnetRoot "shared\Microsoft.NETCore.App\$coreVersion") -Destination (Join-Path $portable 'shared\Microsoft.NETCore.App') -Recurse
Copy-Item -LiteralPath (Join-Path $dotnetRoot "shared\Microsoft.WindowsDesktop.App\$desktopVersion") -Destination (Join-Path $portable 'shared\Microsoft.WindowsDesktop.App') -Recurse
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $output -CompressionLevel Optimal
Write-Output $output
