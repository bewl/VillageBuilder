using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VillageBuilder.Game.Graphics
{
    /// <summary>
    /// Utility for automatically downloading building sprites if missing.
    /// Uses PowerShell script to download from Twemoji repository.
    /// </summary>
    public static class SpriteDownloader
    {
        private const string ScriptPath = "Assets/sprites/download_building_sprites.ps1";
        private const string OutputDir = "Assets/sprites/emojis";
        
        /// <summary>
        /// Check if sprites directory exists and has files
        /// </summary>
        public static bool HasSprites()
        {
            if (!Directory.Exists(OutputDir))
                return false;
            
            var files = Directory.GetFiles(OutputDir, "*.png");
            return files.Length > 0;
        }
        
        /// <summary>
        /// Get count of existing sprite files
        /// </summary>
        public static int GetSpriteCount()
        {
            if (!Directory.Exists(OutputDir))
                return 0;
            
            return Directory.GetFiles(OutputDir, "*.png").Length;
        }
        
        /// <summary>
        /// Download sprites synchronously using PowerShell script
        /// </summary>
        public static bool DownloadSprites(bool quiet = false)
        {
            try
            {
                if (!File.Exists(ScriptPath))
                {
                    Console.WriteLine($"? Sprite download script not found: {ScriptPath}");
                    return false;
                }
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{ScriptPath}\" -OutputDir \"{OutputDir}\" {(quiet ? "-Quiet" : "")}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = quiet,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };
                
                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Console.WriteLine("? Failed to start PowerShell process");
                    return false;
                }
                
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();
                
                if (!quiet && !string.IsNullOrEmpty(output))
                {
                    Console.WriteLine(output);
                }
                
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"PowerShell Error: {error}");
                }
                
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Sprite download failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Download sprites asynchronously
        /// </summary>
        public static Task<bool> DownloadSpritesAsync(bool quiet = false)
        {
            return Task.Run(() => DownloadSprites(quiet));
        }
        
        /// <summary>
        /// Auto-download sprites if missing on startup
        /// </summary>
        public static void AutoDownloadIfMissing()
        {
            var existingCount = GetSpriteCount();
            var requiredCount = 23; // Minimum sprites needed (9 buildings + 14 essential components)
            
            if (existingCount >= requiredCount)
            {
                Console.WriteLine($"? Found {existingCount} sprites - skipping download");
                return;
            }
            
            Console.WriteLine($"? Only {existingCount}/{requiredCount} sprites found");
            Console.WriteLine("?? Auto-downloading building sprites...");
            Console.WriteLine("   (This happens once - sprites will be cached)");
            Console.WriteLine();
            
            var success = DownloadSprites(quiet: false);
            
            if (success)
            {
                var newCount = GetSpriteCount();
                Console.WriteLine($"? Download complete! {newCount} sprites ready.");
            }
            else
            {
                Console.WriteLine("? Download failed or incomplete.");
                Console.WriteLine("   Game will use ASCII mode as fallback.");
                Console.WriteLine($"   Manual download: Run {ScriptPath}");
            }
            
            Console.WriteLine();
        }
    }
}
