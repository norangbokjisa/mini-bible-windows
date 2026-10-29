$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$zip = Join-Path $root 'outputs\mini_bible_windows_v2.9.zip'
if (-not (Test-Path -LiteralPath $zip)) { throw 'ZIP 파일이 없습니다.' }
$extract = Join-Path $root 'work\package-smoke'
if (Test-Path -LiteralPath $extract) { Remove-Item -LiteralPath $extract -Recurse -Force }
Expand-Archive -LiteralPath $zip -DestinationPath $extract
$required = @('MiniBible.App.exe','MiniBible.App.dll','dotnet\dotnet.exe','README.txt','CHANGELOG.txt','sample_bible.csv','sample_bible.json')
foreach ($name in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $extract $name))) { throw "필수 파일 누락: $name" }
}
$launchers = @(Get-ChildItem -LiteralPath $extract -Filter '*.cmd')
if ($launchers.Count -ne 1) { throw "실행 스크립트 수가 올바르지 않습니다: $($launchers.Count)" }
$process = Start-Process -FilePath (Join-Path $extract 'dotnet\dotnet.exe') -ArgumentList @((Join-Path $extract 'MiniBible.App.dll'),'--smoke-test') -Wait -PassThru -WindowStyle Hidden
if ($process.ExitCode -ne 0) { throw "실행 점검 실패: $($process.ExitCode)" }
$sample = Get-Content -Raw -LiteralPath (Join-Path $extract 'sample_bible.csv')
if (($sample -split [Environment]::NewLine).Count -gt 7) { throw '예제 CSV가 예상보다 큽니다.' }
Write-Output 'PACKAGE_SMOKE_OK'
