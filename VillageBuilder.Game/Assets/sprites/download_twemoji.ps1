# Twemoji Sprite Downloader for VillageBuilder
# Downloads open-source Twitter emoji (Twemoji) PNG sprites for terrain decorations

Write-Host "=== Twemoji Emoji Sprite Downloader ===" -ForegroundColor Cyan
Write-Host ""

$spriteDir = Join-Path $PSScriptRoot "emojis"

# Create sprites directory if it doesn't exist
if (-not (Test-Path $spriteDir)) {
    New-Item -ItemType Directory -Path $spriteDir -Force | Out-Null
    Write-Host "? Created sprites directory: $spriteDir" -ForegroundColor Green
}

# Twemoji CDN base URL (72x72 PNG sprites)
$twemojiBase = "https://cdn.jsdelivr.net/gh/twitter/twemoji@14.0.2/assets/72x72/"

# Emoji sprites needed for terrain decorations
$emojiList = @{
    "1f333.png" = "?? Deciduous Tree"
    "1f332.png" = "?? Evergreen Tree"
    "1fab5.png" = "?? Wood/Log"
    "1f33f.png" = "?? Herb/Fern"
    "1fad0.png" = "?? Blueberries"
    "1f33a.png" = "?? Hibiscus"
    "1f33c.png" = "?? Blossom"
    "1f338.png" = "?? Cherry Blossom"
    "1f33e.png" = "?? Sheaf of Rice"
    "1f344.png" = "?? Mushroom"
    "1faa8.png" = "?? Rock"
    "1f426.png" = "?? Bird"
    "1f99c.png" = "?? Parrot"
    "1f985.png" = "?? Eagle"
    "1f98b.png" = "?? Butterfly"
    "1f430.png" = "?? Rabbit"
    "1f98c.png" = "?? Deer"
    "1f41f.png" = "?? Fish"
    # NEW: Predator and wildlife sprites
    "1f98a.png" = "?? Fox"
    "1f43a.png" = "?? Wolf"
    "1f43b.png" = "?? Bear"
    "1f417.png" = "?? Boar"
}

Write-Host "Downloading $($emojiList.Count) emoji sprites from Twemoji CDN..." -ForegroundColor Yellow
Write-Host ""

$successCount = 0
$failCount = 0

foreach ($entry in $emojiList.GetEnumerator()) {
    $filename = $entry.Key
    $description = $entry.Value
    $url = $twemojiBase + $filename
    $outputPath = Join-Path $spriteDir $filename
    
    Write-Host "  Downloading: $description ($filename)" -ForegroundColor Gray
    
    try {
        Invoke-WebRequest -Uri $url -OutFile $outputPath -UseBasicParsing -ErrorAction Stop
        
        # Verify file was downloaded and has content
        $fileInfo = Get-Item $outputPath
        if ($fileInfo.Length -gt 0) {
            Write-Host "    ? Success ($(([math]::Round($fileInfo.Length/1KB, 1))) KB)" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "    ? Failed: Empty file" -ForegroundColor Red
            Remove-Item $outputPath -Force
            $failCount++
        }
    }
    catch {
        Write-Host "    ? Failed: $_" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "=== Download Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  ? Downloaded: $successCount sprites" -ForegroundColor Green

if ($failCount -gt 0) {
    Write-Host "  ? Failed: $failCount sprites" -ForegroundColor Red
} else {
    Write-Host "  ? All sprites downloaded successfully!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Sprites saved to: $spriteDir" -ForegroundColor Gray
Write-Host ""

if ($successCount -gt 0) {
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Build the project (Ctrl+Shift+B)" -ForegroundColor Gray
    Write-Host "  2. Run the game - sprites will load automatically!" -ForegroundColor Gray
    Write-Host "  3. Enjoy colorful emoji terrain decorations! ??????" -ForegroundColor Gray
} else {
    Write-Host "? No sprites downloaded. Check your internet connection." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "License: Twemoji graphics are licensed under CC-BY 4.0" -ForegroundColor Gray
Write-Host "         https://creativecommons.org/licenses/by/4.0/" -ForegroundColor Gray
