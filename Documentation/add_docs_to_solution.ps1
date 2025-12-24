# Add Documentation to Solution
# This script adds the Documentation folder structure to VillageBuilder.sln

$solutionPath = "C:\Users\usarm\source\repos\bewl\VillageBuilder\VillageBuilder.sln"
$docsPath = "C:\Users\usarm\source\repos\bewl\VillageBuilder\Documentation"

Write-Host "Adding Documentation folder structure to solution..." -ForegroundColor Cyan

# Read the solution file
$solutionContent = Get-Content $solutionPath -Raw

# Generate a new GUID for the Documentation solution folder
$docsFolderGuid = [guid]::NewGuid().ToString().ToUpper()

# Create the main Documentation solution folder
$docsSolutionFolder = @"

Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Documentation", "Documentation", "{$docsFolderGuid}"
	ProjectSection(SolutionItems) = preProject
		Documentation\README.md = Documentation\README.md
		Documentation\DOCUMENTATION_CHECKLIST.md = Documentation\DOCUMENTATION_CHECKLIST.md
		Documentation\REORGANIZATION_SUMMARY.md = Documentation\REORGANIZATION_SUMMARY.md
		Documentation\reorganize_docs.ps1 = Documentation\reorganize_docs.ps1
	EndProjectSection
EndProject
"@

# Function to create nested solution folders
function Add-SolutionFolder {
    param (
        [string]$FolderName,
        [string]$RelativePath,
        [string]$ParentGuid,
        [hashtable]$Files
    )
    
    $folderGuid = [guid]::NewGuid().ToString().ToUpper()
    
    $filesList = ""
    foreach ($file in $Files.Keys) {
        $filesList += "`t`t$RelativePath\$file = $RelativePath\$file`r`n"
    }
    
    $folder = @"

Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "$FolderName", "$FolderName", "{$folderGuid}"
	ProjectSection(SolutionItems) = preProject
$filesList	EndProjectSection
EndProject
"@
    
    return @{
        Content = $folder
        Guid = $folderGuid
        ParentGuid = $ParentGuid
        Name = $FolderName
    }
}

# Get all subdirectories and their files
$folders = @{
    "GettingStarted" = @{
        "QUICKSTART.md" = $true
    }
    "CoreSystems" = @{
        "GAME_ENGINE.md" = $true
        "GAME_CONFIGURATION.md" = $true
        "TIME_AND_SEASONS.md" = $true
        "COMMAND_SYSTEM.md" = $true
    }
    "WorldAndSimulation" = @{
        "PATHFINDING.md" = $true
    }
    "EntitiesAndBuildings" = @{
        "PEOPLE_AND_FAMILIES.md" = $true
        "BUILDING_SYSTEM.md" = $true
        "CONSTRUCTION_SYSTEM.md" = $true
    }
    "Rendering" = @{
        "VISUAL_ENHANCEMENTS.md" = $true
        "UI_INTEGRATION_GUIDELINES.md" = $true
    }
    "SaveLoad" = @{
        "SAVE_LOAD_SYSTEM.md" = $true
        "EVENT_LOG_LOAD_BEHAVIOR.md" = $true
    }
    "Performance" = @{
        "PERFORMANCE_OPTIMIZATIONS.md" = $true
        "OPTIMIZATION_CHANGELOG.md" = $true
        "BENCHMARK_GUIDE.md" = $true
    }
    "BugFixes" = @{
        "README.md" = $true
        "AUTO_CONSTRUCTION_ASSIGNMENT.md" = $true
        "BORDERLESS_FULLSCREEN_FIX.md" = $true
        "BUGFIX_CONSTRUCTION_HUNGER.md" = $true
        "CONSTRUCTION_DAILY_ROUTINE_FIX.md" = $true
        "CONSTRUCTION_PRESENCE_REQUIREMENT.md" = $true
        "CONSTRUCTION_WORKER_RETURN_FIX.md" = $true
        "FONT_CONFIGURATION.md" = $true
        "FONT_QUICK_SETUP.md" = $true
        "MULTI_PERSON_SELECTION_FLICKER_FIX.md" = $true
        "MULTI_PERSON_SELECTION_THREAD_SAFETY_FIX.md" = $true
        "PATHFINDING_SELECTION_IMPROVEMENTS.md" = $true
        "VIEWPORT_RESOLUTION_FIX.md" = $true
    }
}

# Build the solution folder structure
$allFolders = @()
$nestingSection = ""

foreach ($folderName in $folders.Keys) {
    $files = $folders[$folderName]
    $folder = Add-SolutionFolder -FolderName $folderName -RelativePath "Documentation\$folderName" -ParentGuid $docsFolderGuid -Files $files
    $allFolders += $folder
    
    # Add to GlobalSection for nesting
    $nestingSection += "`t`t{$($folder.Guid)} = {$docsFolderGuid}`r`n"
}

# Combine all folder definitions
$allFolderContent = $docsSolutionFolder
foreach ($folder in $allFolders) {
    $allFolderContent += $folder.Content
}

# Find the position to insert (before GlobalSection)
$insertPosition = $solutionContent.IndexOf("Global")
if ($insertPosition -eq -1) {
    Write-Host "Error: Could not find Global section in solution file" -ForegroundColor Red
    exit 1
}

# Insert the documentation folders
$newSolutionContent = $solutionContent.Insert($insertPosition, $allFolderContent)

# Find the NestedProjects section or create it
if ($newSolutionContent -match "GlobalSection\(NestedProjects\) = preSolution") {
    # Add to existing NestedProjects section
    $nestingPosition = $newSolutionContent.IndexOf("GlobalSection(NestedProjects) = preSolution") + "GlobalSection(NestedProjects) = preSolution".Length + 2
    $newSolutionContent = $newSolutionContent.Insert($nestingPosition, $nestingSection)
} else {
    # Create new NestedProjects section
    $nestingGlobalSection = @"
	GlobalSection(NestedProjects) = preSolution
$nestingSection	EndGlobalSection

"@
    $endGlobalPosition = $newSolutionContent.LastIndexOf("EndGlobal")
    $newSolutionContent = $newSolutionContent.Insert($endGlobalPosition, $nestingGlobalSection)
}

# Backup the original solution file
$backupPath = "$solutionPath.backup"
Copy-Item $solutionPath $backupPath -Force
Write-Host "Created backup: $backupPath" -ForegroundColor Yellow

# Write the new solution file
$newSolutionContent | Set-Content $solutionPath -Encoding UTF8

Write-Host "`n? Documentation folder structure added to solution!" -ForegroundColor Green
Write-Host "`nAdded folders:" -ForegroundColor Cyan
foreach ($folderName in $folders.Keys) {
    $fileCount = $folders[$folderName].Count
    Write-Host "  - $folderName ($fileCount files)" -ForegroundColor White
}

Write-Host "`nTotal: $($allFolders.Count) subfolders with $($folders.Values | ForEach-Object { $_.Count } | Measure-Object -Sum | Select-Object -ExpandProperty Sum) files" -ForegroundColor Cyan

Write-Host "`nPlease reload the solution in Visual Studio to see the changes." -ForegroundColor Yellow
Write-Host "If you need to revert: Copy $backupPath back to $solutionPath" -ForegroundColor Gray
