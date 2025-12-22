using System;
using System.Collections.Generic;
using System.Linq;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Core
{
    public static class PathfindingHelper
    {
        /// <summary>
        /// Find the nearest walkable tile adjacent to a target position
        /// (useful for finding where to stand when entering a building door)
        /// </summary>
        public static Vector2Int? FindNearestWalkableTile(Vector2Int target, VillageGrid grid, int maxDistance = 3)
        {
            // Check if target itself is walkable
            var targetTile = grid.GetTile(target.X, target.Y);
            if (targetTile != null && targetTile.IsWalkable)
            {
                return target;
            }

            // Check adjacent tiles in order of proximity
            var offsets = new[]
            {
                // Immediately adjacent (distance 1)
                new Vector2Int(0, -1),  // North
                new Vector2Int(1, 0),   // East
                new Vector2Int(0, 1),   // South
                new Vector2Int(-1, 0),  // West
                
                // Diagonal (distance ~1.4)
                new Vector2Int(-1, -1), // NW
                new Vector2Int(1, -1),  // NE
                new Vector2Int(1, 1),   // SE
                new Vector2Int(-1, 1),  // SW
            };

            foreach (var offset in offsets)
            {
                var checkPos = new Vector2Int(target.X + offset.X, target.Y + offset.Y);
                
                if (checkPos.X < 0 || checkPos.X >= grid.Width || checkPos.Y < 0 || checkPos.Y >= grid.Height)
                    continue;
                
                var tile = grid.GetTile(checkPos.X, checkPos.Y);
                if (tile != null && tile.IsWalkable)
                {
                    return checkPos;
                }
            }

            // No adjacent walkable tile found
            return null;
        }
    }
}
