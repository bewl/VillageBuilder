# Building Sprite Auto-Downloader
# Downloads all required emoji sprites from Twemoji for VillageBuilder

param(
    [string]$OutputDir = "Assets/sprites/emojis",
    [switch]$Force = $false,
    [switch]$Quiet = $false
)

$baseUrl = "https://raw.githubusercontent.com/twitter/twemoji/master/assets/72x72"
$ErrorActionPreference = "Continue"

# Sprite list with descriptions
$sprites = @{
    # Full building icons (9)
    "1f3e0.png" = "?? House"
    "1f33e.png" = "?? Farm"
    "1f3e6.png" = "?? Warehouse"
    "1f3ed.png" = "?? Workshop"
    "26cf.png"  = "?? Mine"
    "1fab5.png" = "?? Lumberyard"
    "1f3ea.png" = "?? Market"
    "1f6b0.png" = "?? Well"
    "1f3db.png" = "??? Town Hall"
    
    # Component sprites (26)
    "1f9f1.png" = "?? Brick"
    "1faa8.png" = "?? Stone"
    "1f7eb.png" = "?? Brown square"
    "2b1c.png"  = "? White square"
    "1f6aa.png" = "?? Door"
    "1f6b6.png" = "?? Person"
    "1f512.png" = "?? Lock"
    "1fa9f.png" = "?? Window"
    "1f319.png" = "?? Moon"
    "1f525.png" = "?? Fire"
    "1f53a.png" = "?? Triangle"
    "1f7e9.png" = "?? Green square"
    "1f6a7.png" = "?? Construction"
    "1f4cb.png" = "?? Clipboard"
}

# Create directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
    if (-not $Quiet) {
        Write-Host "? Created directory: $OutputDir" -ForegroundColor Green
    }
}

$downloadedCount = 0
$skippedCount = 0
$failedCount = 0
$totalCount = $sprites.Count

if (-not $Quiet) {
    Write-Host "`n?? Building Sprite Auto-Downloader" -ForegroundColor Cyan
    Write-Host "=================================" -ForegroundColor Cyan
    Write-Host "Downloading $totalCount sprites...`n" -ForegroundColor White
}

foreach ($file in $sprites.Keys | Sort-Object) {
    $url = "$baseUrl/$file"
    $output = Join-Path $OutputDir $file
    $description = $sprites[$file]
    
    # Skip if file exists and not forcing
    if ((Test-Path $output) -and -not $Force) {
        if (-not $Quiet) {
            Write-Host "  ? Skipped: $description (already exists)" -ForegroundColor DarkGray
        }
        $skippedCount++
        continue
    }
    
    if (-not $Quiet) {
        Write-Host "  ? Downloading: $description..." -ForegroundColor Cyan -NoNewline
    }
    
    try {
        Invoke-WebRequest -Uri $url -OutFile $output -ErrorAction Stop -UseBasicParsing | Out-Null
        
        if (Test-Path $output) {
            if (-not $Quiet) {
                Write-Host " ?" -ForegroundColor Green
            }
            $downloadedCount++
        } else {
            throw "File not created"
        }
    }
    catch {
        if (-not $Quiet) {
            Write-Host " ?" -ForegroundColor Red
            Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
        }
        $failedCount++
    }
}

# Summary
if (-not $Quiet) {
    Write-Host "`n=================================" -ForegroundColor Cyan
    Write-Host "Summary:" -ForegroundColor White
    Write-Host "  Downloaded: $downloadedCount" -ForegroundColor Green
    Write-Host "  Skipped:    $skippedCount" -ForegroundColor DarkGray
    Write-Host "  Failed:     $failedCount" -ForegroundColor $(if ($failedCount -gt 0) { "Red" } else { "DarkGray" })
    Write-Host "  Total:      $totalCount" -ForegroundColor White
    
    if ($downloadedCount + $skippedCount -eq $totalCount) {
        Write-Host "`n? All sprites ready! Sprites saved to: $OutputDir" -ForegroundColor Green
    } elseif ($failedCount -gt 0) {
        Write-Host "`n? Some sprites failed to download. Game will use ASCII fallback." -ForegroundColor Yellow
    }
}

# Return result object for C# caller
$result = @{
    Downloaded = $downloadedCount
    Skipped = $skippedCount
    Failed = $failedCount
    Total = $totalCount
    Success = ($downloadedCount + $skippedCount -eq $totalCount)
}

return $result
