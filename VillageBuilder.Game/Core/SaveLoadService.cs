using System;
using System.IO;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Core
{
    /// <summary>
    /// Handles save/load operations with UI-friendly methods
    /// </summary>
    public class SaveLoadService
    {
        private const string SaveDirectory = "Saves";
        private const string SaveExtension = ".vbsave";

        // Cache for save files to avoid filesystem calls every frame
        private static string[]? _cachedSaveFiles = null;
        private static DateTime _cacheTimestamp = DateTime.MinValue;
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Ensure save directory exists
        /// </summary>
        public static void InitializeSaveDirectory()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        /// <summary>
        /// Invalidate the save file cache (call after save/load/delete operations)
        /// </summary>
        public static void InvalidateCache()
        {
            _cachedSaveFiles = null;
            _cacheTimestamp = DateTime.MinValue;
        }

        /// <summary>
        /// Get all available save files (cached for 1 second to avoid filesystem overhead)
        /// </summary>
        public static string[] GetSaveFiles()
        {
            // Check if cache is valid
            if (_cachedSaveFiles != null && (DateTime.Now - _cacheTimestamp) < CacheExpiration)
            {
                return _cachedSaveFiles;
            }

            // Refresh cache
            InitializeSaveDirectory();
            var files = Directory.GetFiles(SaveDirectory, $"*{SaveExtension}");
            _cachedSaveFiles = Array.ConvertAll(files, Path.GetFileNameWithoutExtension);
            _cacheTimestamp = DateTime.Now;

            return _cachedSaveFiles;
        }
        
        /// <summary>
        /// Save game with a given name
        /// </summary>
        public static SaveResult SaveGame(GameEngine engine, string saveName)
        {
            try
            {
                InitializeSaveDirectory();

                // Sanitize filename
                var sanitized = SanitizeFileName(saveName);
                if (string.IsNullOrWhiteSpace(sanitized))
                {
                    return new SaveResult { Success = false, Message = "Invalid save name" };
                }

                var filePath = GetSaveFilePath(sanitized);
                SaveLoadManager.SaveGame(engine, filePath);

                // Invalidate cache after saving
                InvalidateCache();

                return new SaveResult 
                { 
                    Success = true, 
                    Message = $"Game saved to: {sanitized}",
                    FilePath = filePath
                };
            }
            catch (Exception ex)
            {
                return new SaveResult 
                { 
                    Success = false, 
                    Message = $"Save failed: {ex.Message}" 
                };
            }
        }
        
        /// <summary>
        /// Quick save to default slot
        /// </summary>
        public static SaveResult QuickSave(GameEngine engine)
        {
            var saveName = $"quicksave_{DateTime.Now:yyyyMMdd_HHmmss}";
            return SaveGame(engine, saveName);
        }

        /// <summary>
        /// Quick load from most recent quicksave
        /// </summary>
        public static LoadResult QuickLoad()
        {
            try
            {
                InitializeSaveDirectory();

                // Find all quicksave files
                var quicksaveFiles = Directory.GetFiles(SaveDirectory, "quicksave_*" + SaveExtension);

                if (quicksaveFiles.Length == 0)
                {
                    return new LoadResult
                    {
                        Success = false,
                        Message = "No quicksaves found. Press F5 to create one."
                    };
                }

                // Get the most recent quicksave by file modification time
                var latestSave = quicksaveFiles
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .First();

                var saveName = Path.GetFileNameWithoutExtension(latestSave.Name);
                return LoadGame(saveName);
            }
            catch (Exception ex)
            {
                return new LoadResult
                {
                    Success = false,
                    Message = $"Quick load failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Auto save
        /// </summary>
        public static SaveResult AutoSave(GameEngine engine)
        {
            // Keep only last 3 autosaves
            CleanupOldAutosaves();
            
            var saveName = $"autosave_{DateTime.Now:yyyyMMdd_HHmmss}";
            return SaveGame(engine, saveName);
        }
        
        /// <summary>
        /// Load game from save name
        /// </summary>
        public static LoadResult LoadGame(string saveName)
        {
            try
            {
                var filePath = GetSaveFilePath(saveName);

                if (!File.Exists(filePath))
                {
                    return new LoadResult 
                    { 
                        Success = false, 
                        Message = $"Save file not found: {saveName}" 
                    };
                }

                var engine = SaveLoadManager.LoadGame(filePath);

                // Clear the event log on successful load to provide a clean slate
                // Events from the previous game session are not relevant to the loaded game
                EventLog.Instance.Clear();

                // Add a load notification to the now-clean event log
                EventLog.Instance.AddMessage($"Game loaded from save: {saveName}", LogLevel.Success);

                return new LoadResult 
                { 
                    Success = true, 
                    Message = $"Game loaded from: {saveName}",
                    Engine = engine
                };
            }
            catch (Exception ex)
            {
                return new LoadResult 
                { 
                    Success = false, 
                    Message = $"Load failed: {ex.Message}" 
                };
            }
        }
        
        /// <summary>
        /// Delete a save file
        /// </summary>
        public static bool DeleteSave(string saveName)
        {
            try
            {
                var filePath = GetSaveFilePath(saveName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    // Invalidate cache after deleting
                    InvalidateCache();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get save file info
        /// </summary>
        public static SaveFileInfo? GetSaveInfo(string saveName)
        {
            try
            {
                var filePath = GetSaveFilePath(saveName);
                if (!File.Exists(filePath))
                    return null;
                
                var fileInfo = new FileInfo(filePath);
                return new SaveFileInfo
                {
                    Name = saveName,
                    FilePath = filePath,
                    SavedAt = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length
                };
            }
            catch
            {
                return null;
            }
        }
        
        private static string GetSaveFilePath(string saveName)
        {
            return Path.Combine(SaveDirectory, saveName + SaveExtension);
        }
        
        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }
        
        private static void CleanupOldAutosaves()
        {
            try
            {
                var autosaves = Directory.GetFiles(SaveDirectory, "autosave_*" + SaveExtension);
                if (autosaves.Length <= 3)
                    return;
                
                // Sort by creation time and delete oldest
                Array.Sort(autosaves, (a, b) => 
                    File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));
                
                for (int i = 0; i < autosaves.Length - 3; i++)
                {
                    File.Delete(autosaves[i]);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
    
    public class SaveResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? FilePath { get; set; }
    }
    
    public class LoadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public GameEngine? Engine { get; set; }
    }
    
    public class SaveFileInfo
    {
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime SavedAt { get; set; }
        public long FileSize { get; set; }
    }
}
