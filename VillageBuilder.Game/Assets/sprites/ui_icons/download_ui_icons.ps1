# Download UI Icon Emoji Sprites from Twemoji
# This script downloads emoji PNG files for UI icons (resources, buildings, people)

$outputDir = "emojis"
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

Write-Host "=== VillageBuilder UI Icon Emoji Downloader ===" -ForegroundColor Cyan
Write-Host ""

# Twemoji CDN base URL (72x72 resolution)
$baseUrl = "https://cdnjs.cloudflare.com/ajax/libs/twemoji/14.0.2/72x72"

# UI Icon Emojis to download
$emojis = @{
    # Resources
    "1fab5" = "?? Wood/Log"
    "1faa8" = "?? Rock/Stone"
    "1f33e" = "?? Grain/Wheat"
    "1f528" = "?? Tools/Hammer"
    "1f525" = "?? Fire/Firewood"

    # People & Buildings
    "1f465" = "?? People/Families"
    "1f3d8" = "??? Buildings/Houses"
    "1f3e0" = "?? House"
    "1f3ed" = "?? Factory/Workshop"
    "26cf"  = "?? Mine/Pickaxe"

    # Status Icons
    "1f4be" = "?? Save/Disk"
    "1f3d7" = "??? Construction"
    "2699"  = "?? Settings/Commands"
    "1f4ca" = "?? Stats/Chart"

    # Activities
    "1f4a4" = "?? Sleeping"
    "1f6b6" = "?? Walking"
    "1f60c" = "?? Resting"
    "1f9cd" = "?? Standing/Idle"

    # Log Levels
    "2139"  = "?? Info"
    "26a0"  = "?? Warning"
    "274c"  = "? Error"
    "2705"  = "? Success"

    # UI Decorations (Arrows)
    "25b6"  = "?? Play/Arrow Right"
    "25c0"  = "?? Arrow Left"
    "23f5"  = "?? Small Triangle Right"

    # UI Decorations (Lines/Separators)
    "2796"  = "? Heavy Minus/Line"
    "2014"  = "— Em Dash"
    "2015"  = "? Horizontal Bar"
}

$successCount = 0
$failCount = 0

foreach ($codepoint in $emojis.Keys) {
    $description = $emojis[$codepoint]
    $filename = "$codepoint.png"
    $url = "$baseUrl/$filename"
    $outputPath = Join-Path $outputDir $filename
    
    Write-Host "Downloading $description..." -NoNewline
    
    try {
        Invoke-WebRequest -Uri $url -OutFile $outputPath -ErrorAction Stop
        Write-Host "  ?" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "  ? Failed" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "=== Download Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Downloaded emoji sprites:" -ForegroundColor Green
Get-ChildItem $outputDir -Filter "*.png" | ForEach-Object {
    $size = [math]::Round($_.Length / 1KB, 2)
    Write-Host "  • $($_.Name) ($size KB)"
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  ? Success: $successCount" -ForegroundColor Green
Write-Host "  ? Failed: $failCount" -ForegroundColor Red
