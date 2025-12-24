# Documentation Reorganization Script
# This script moves existing documentation into the new organized structure

$rootPath = "C:\Users\usarm\source\repos\bewl\VillageBuilder\Documentation"

# Create new directory structure
$directories = @(
    "GettingStarted",
    "CoreSystems",
    "WorldAndSimulation",
    "EntitiesAndBuildings",
    "Rendering",
    "SaveLoad",
    "Performance",
    "BugFixes"
)

Write-Host "Creating directory structure..." -ForegroundColor Cyan
foreach ($dir in $directories) {
    $path = Join-Path $rootPath $dir
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
        Write-Host "  Created: $dir" -ForegroundColor Green
    }
}

# Move files to appropriate directories
Write-Host "`nMoving files to organized structure..." -ForegroundColor Cyan

# Performance docs
$performanceDocs = @(
    "PERFORMANCE_OPTIMIZATIONS.md",
    "OPTIMIZATION_CHANGELOG.md",
    "BENCHMARK_GUIDE.md"
)

foreach ($file in $performanceDocs) {
    $source = Join-Path (Join-Path $rootPath "..") $file
    if (Test-Path $source) {
        $dest = Join-Path (Join-Path $rootPath "Performance") $file
        Move-Item -Path $source -Destination $dest -Force
        Write-Host "  Moved: $file -> Performance/" -ForegroundColor Yellow
    }
}

# Bug fix docs
$bugFixDocs = @(
    "PATHFINDING_SELECTION_IMPROVEMENTS.md",
    "BUGFIX_CONSTRUCTION_HUNGER.md",
    "VIEWPORT_RESOLUTION_FIX.md",
    "BORDERLESS_FULLSCREEN_FIX.md",
    "FONT_CONFIGURATION.md",
    "FONT_QUICK_SETUP.md",
    "CONSTRUCTION_DAILY_ROUTINE_FIX.md",
    "CONSTRUCTION_PRESENCE_REQUIREMENT.md",
    "CONSTRUCTION_WORKER_RETURN_FIX.md",
    "MULTI_PERSON_SELECTION_FLICKER_FIX.md",
    "MULTI_PERSON_SELECTION_THREAD_SAFETY_FIX.md",
    "AUTO_CONSTRUCTION_ASSIGNMENT.md"
)

$gamePath = Join-Path (Split-Path $rootPath -Parent) "VillageBuilder.Game\Documentation"
foreach ($file in $bugFixDocs) {
    $source = Join-Path $gamePath $file
    if (Test-Path $source) {
        $dest = Join-Path (Join-Path $rootPath "BugFixes") $file
        Move-Item -Path $source -Destination $dest -Force
        Write-Host "  Moved: $file -> BugFixes/" -ForegroundColor Yellow
    }
}

# System docs
$systemMoves = @{
    "CONSTRUCTION_SYSTEM.md" = "EntitiesAndBuildings"
    "SAVE_LOAD_SYSTEM.md" = "SaveLoad"
    "EVENT_LOG_LOAD_BEHAVIOR.md" = "SaveLoad"
    "VISUAL_ENHANCEMENTS.md" = "Rendering"
    "UI_INTEGRATION_GUIDELINES.md" = "Rendering"
}

foreach ($file in $systemMoves.Keys) {
    $source = Join-Path $gamePath $file
    if (Test-Path $source) {
        $dest = Join-Path (Join-Path $rootPath $systemMoves[$file]) $file
        Move-Item -Path $source -Destination $dest -Force
        Write-Host "  Moved: $file -> $($systemMoves[$file])/" -ForegroundColor Yellow
    }
}

Write-Host "`nReorganization complete!" -ForegroundColor Green
Write-Host "`nNew structure:" -ForegroundColor Cyan
Get-ChildItem -Path $rootPath -Directory | ForEach-Object {
    Write-Host "  $($_.Name)/" -ForegroundColor White
    Get-ChildItem -Path $_.FullName -File | ForEach-Object {
        Write-Host "    - $($_.Name)" -ForegroundColor Gray
    }
}
