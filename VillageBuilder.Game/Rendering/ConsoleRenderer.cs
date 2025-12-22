using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Rendering
{
    public static class ConsoleRenderer
    {
        public static void DisplayGameStatus(GameEngine game)
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║      VILLAGE BUILDER ENGINE v0.1       ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");
            
            Console.WriteLine($"🕐 {game.Time}");
            Console.WriteLine($"🌤️  Weather: {game.Weather.Condition}, Temp: {game.Weather.Temperature}°C\n");
            
            Console.WriteLine("📦 Resources:");
            foreach (var resource in game.VillageResources.GetAll().Where(r => r.Value > 0))
            {
                Console.WriteLine($"   {resource.Key}: {resource.Value}");
            }
            
            Console.WriteLine($"\n👨‍👩‍👧‍👦 Families: {game.Families.Count}");
            Console.WriteLine($"🏠 Buildings: {game.Buildings.Count(b => b.IsConstructed)}/{game.Buildings.Count}");
            
            Console.WriteLine("\n" + new string('─', 40));
        }

        public static void DisplayGrid(VillageGrid grid, int centerX, int centerY, int viewRadius = 10)
        {
            Console.WriteLine($"╔{new string('═', viewRadius * 2 + 1)}╗");
            
            for (int y = centerY - viewRadius; y <= centerY + viewRadius; y++)
            {
                Console.Write("║");
                for (int x = centerX - viewRadius; x <= centerX + viewRadius; x++)
                {
                    var tile = grid.GetTile(x, y);
                    if (tile == null)
                    {
                        Console.Write(" ");
                        continue;
                    }

                    char symbol = tile.Type switch
                    {
                        TileType.Grass => '.',
                        TileType.Forest => '♣',
                        TileType.Water => '≈',
                        TileType.Mountain => '▲',
                        TileType.Field => '~',
                        TileType.Road => '─',
                        TileType.BuildingFoundation => '□',
                        _ => '?'
                    };
                    
                    if (tile.Building != null)
                        symbol = '■';

                    Console.Write(symbol);
                }
                Console.WriteLine("║");
            }
            
            Console.WriteLine($"╚{new string('═', viewRadius * 2 + 1)}╝");
        }
    }
}
