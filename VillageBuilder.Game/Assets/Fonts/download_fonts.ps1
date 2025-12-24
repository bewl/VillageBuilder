# Font Download Script for VillageBuilder
# Downloads JetBrains Mono and Noto Color Emoji for optimal rendering

Write-Host "=== VillageBuilder Font Downloader ===" -ForegroundColor Cyan
Write-Host ""

$fontsDir = $PSScriptRoot

# JetBrains Mono - Primary font for text and symbols
$jetbrainsUrl = "https://github.com/JetBrains/JetBrainsMono/releases/download/v2.304/JetBrainsMono-2.304.zip"
$jetbrainsZip = Join-Path $fontsDir "JetBrainsMono.zip"
$jetbrainsExtract = Join-Path $fontsDir "JetBrainsMono_temp"

Write-Host "Downloading JetBrains Mono..." -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri $jetbrainsUrl -OutFile $jetbrainsZip -UseBasicParsing
    Write-Host "  ? Downloaded JetBrains Mono" -ForegroundColor Green
    
    # Extract
    Expand-Archive -Path $jetbrainsZip -DestinationPath $jetbrainsExtract -Force
    
    # Copy the Regular TTF file we need
    $regularFont = Get-ChildItem -Path $jetbrainsExtract -Recurse -Filter "JetBrainsMono-Regular.ttf" | Select-Object -First 1
    if ($regularFont) {
        Copy-Item $regularFont.FullName -Destination (Join-Path $fontsDir "JetBrainsMono-Regular.ttf") -Force
        Write-Host "  ? Extracted JetBrainsMono-Regular.ttf" -ForegroundColor Green
    }
    
    # Cleanup
    Remove-Item $jetbrainsZip -Force
    Remove-Item $jetbrainsExtract -Recurse -Force
}
catch {
    Write-Host "  ? Failed to download JetBrains Mono: $_" -ForegroundColor Red
}

Write-Host ""

# Noto Color Emoji - Emoji font for terrain decorations
$notoUrl = "https://github.com/googlefonts/noto-emoji/raw/main/fonts/NotoColorEmoji.ttf"
$notoFile = Join-Path $fontsDir "NotoColorEmoji.ttf"

Write-Host "Downloading Noto Color Emoji..." -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri $notoUrl -OutFile $notoFile -UseBasicParsing
    Write-Host "  ? Downloaded NotoColorEmoji.ttf" -ForegroundColor Green
}
catch {
    Write-Host "  ? Failed to download Noto Color Emoji: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Font Download Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Downloaded fonts:" -ForegroundColor White
Get-ChildItem -Path $fontsDir -Filter "*.ttf" | ForEach-Object {
    $sizeKB = [math]::Round($_.Length / 1KB, 2)
    Write-Host "  • $($_.Name) ($sizeKB KB)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Build the project (Ctrl+Shift+B)" -ForegroundColor Gray
Write-Host "  2. Run the game - fonts will load automatically!" -ForegroundColor Gray
