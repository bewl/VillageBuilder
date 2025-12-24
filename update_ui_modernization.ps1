# UI Modernization Script - Replace ASCII with Emoji-cohesive symbols

$sidebarPath = "VillageBuilder.Game\Graphics\UI\SidebarRenderer.cs"
$content = Get-Content $sidebarPath -Raw

Write-Host "?? Starting UI Modernization..." -ForegroundColor Cyan

# 1. Section headers: ?? ? ?
$content = $content -replace '\$"\+- \{title\} "', '$"? {title} "'
$content = $content -replace 'new string\(''-''', "new string('?'"
Write-Host "? Updated section headers" -ForegroundColor Green

# 2. Remove vertical pipes (?/¦) - replace with clean indent
$content = $content -replace '¦ ', '  '
$content = $content -replace '\+"', '?'
Write-Host "? Removed ASCII borders" -ForegroundColor Green

# 3. Building emoji icons
$content = $content -replace 'BuildingType\.House => "¦"', 'BuildingType.House => "??"'
$content = $content -replace 'BuildingType\.Farm => "\?"', 'BuildingType.Farm => "??"'
$content = $content -replace 'BuildingType\.Warehouse => "¦"', 'BuildingType.Warehouse => "??"'
$content = $content -replace 'BuildingType\.Mine => "\+"', 'BuildingType.Mine => "??"'
$content = $content -replace 'BuildingType\.Lumberyard => "\+"', 'BuildingType.Lumberyard => "??"'
$content = $content -replace 'BuildingType\.Workshop => "\+"', 'BuildingType.Workshop => "??"'
$content = $content -replace 'BuildingType\.Market => "\+"', 'BuildingType.Market => "??"'
$content = $content -replace 'BuildingType\.Well => "\?"', 'BuildingType.Well => "??"'
$content = $content -replace 'BuildingType\.TownHall => "\?"', 'BuildingType.TownHall => "???"'
$content = $content -replace '_ => "\?"', '_ => "?"'
Write-Host "? Updated building icons to emoji" -ForegroundColor Green

# 4. Status/UI emoji icons
$content = $content -replace '\?\? Families:', '?? Families:'
$content = $content -replace '\?\? No families', '?? No families'
$content = $content -replace '¦¦ Buildings:', '??? Buildings:'
$content = $content -replace '\?\? Last Save:', '?? Last Save:'
$content = $content -replace '\?\? No saves', '?? No saves'
Write-Host "? Updated status icons to emoji" -ForegroundColor Green

# 5. Task/activity emoji icons
$content = $content -replace 'PersonTask\.Sleeping => "\?\?"', 'PersonTask.Sleeping => "??"'
$content = $content -replace 'PersonTask\.GoingHome => "\?\?"', 'PersonTask.GoingHome => "??"'
$content = $content -replace 'PersonTask\.GoingToWork => "\?"', 'PersonTask.GoingToWork => "??"'
$content = $content -replace 'PersonTask\.WorkingAtBuilding => "\?"', 'PersonTask.WorkingAtBuilding => "??"'
$content = $content -replace 'PersonTask\.Constructing => "\?\?"', 'PersonTask.Constructing => "???"'
$content = $content -replace 'PersonTask\.Resting => "\?"', 'PersonTask.Resting => "??"'
$content = $content -replace 'PersonTask\.MovingToLocation => "\?"', 'PersonTask.MovingToLocation => "??"'
$content = $content -replace 'PersonTask\.Idle => "\?"', 'PersonTask.Idle => "??"'
$content = $content -replace '_ => "·"', '_ => "?"'
Write-Host "? Updated task icons to emoji" -ForegroundColor Green

# Save changes
Set-Content $sidebarPath -Value $content -NoNewline
Write-Host "" 
Write-Host "? UI Modernization Complete!" -ForegroundColor Green
Write-Host "   ASCII ? Emoji transition successful" -ForegroundColor Gray
