<#
.SYNOPSIS
    Builds and packages the ImageProcessingEngine External Library for upload to the ODC Portal.

.DESCRIPTION
    1. Publishes the library for linux-x64 (the runtime used by ODC containers).
    2. Zips the contents of the publish output directory to the repo root.
    3. The resulting ImageProcessingEngine.zip is ready for upload via:
       ODC Portal > Extend your apps > External Libraries > Upload

.NOTES
    Prerequisites: .NET 8 SDK installed and on PATH.
    Run from the /scripts directory or from the solution root.
#>

$ErrorActionPreference = "Stop"

$repoRoot    = Resolve-Path "$PSScriptRoot\.."
$projectPath = "$repoRoot\src\ImageProcessingEngine\ImageProcessingEngine.csproj"
$publishDir  = "$repoRoot\src\ImageProcessingEngine\bin\Release\net8.0\linux-x64\publish"
$zipPath     = "$repoRoot\ImageProcessingEngine.zip"

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "  ImageProcessingEngine — ODC Package Build" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

# Step 1: Run tests first — fail fast before packaging.
Write-Host "`n[1/3] Running unit tests..." -ForegroundColor Yellow
dotnet test "$repoRoot\tests\ImageProcessingEngine.Tests\ImageProcessingEngine.Tests.csproj" `
    --configuration Release `
    --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "Tests failed. Fix all tests before packaging." }
Write-Host "      All tests passed." -ForegroundColor Green

# Step 2: Publish for ODC's linux-x64 runtime.
Write-Host "`n[2/3] Publishing for linux-x64..." -ForegroundColor Yellow
dotnet publish $projectPath `
    --configuration Release `
    --runtime linux-x64 `
    --no-self-contained `
    --output $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }
Write-Host "      Published to: $publishDir" -ForegroundColor Green

# Step 3: Zip the publish output (contents at root of zip, not in a subfolder).
Write-Host "`n[3/3] Creating upload package..." -ForegroundColor Yellow
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
    Write-Host "      Removed existing $zipPath"
}
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath

Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "  Package ready: $zipPath" -ForegroundColor Green
Write-Host "`n  Upload this file to:" -ForegroundColor White
Write-Host "  ODC Portal > Extend your apps > External Libraries > Upload" -ForegroundColor White
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
