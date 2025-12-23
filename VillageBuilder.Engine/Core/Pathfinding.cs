using System;
using System.Collections.Generic;
using System.Linq;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Core
{
    public class Pathfinding
    {
        private class PathNode
        {
            public Vector2Int Position { get; set; }
            public float GCost { get; set; } // Distance from start
            public float HCost { get; set; } // Distance to end (heuristic)
            public float FCost => GCost + HCost;
            public PathNode? Parent { get; set; }
        }

        public static List<Vector2Int>? FindPath(Vector2Int start, Vector2Int end, VillageGrid grid)
        {
            var openSet = new List<PathNode>();
            var closedSet = new HashSet<Vector2Int>();

            var startNode = new PathNode
            {
                Position = start,
                GCost = 0,
                HCost = GetDistance(start, end)
            };

            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                // Get node with lowest F cost
                var currentNode = openSet.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();
                
                if (currentNode.Position.X == end.X && currentNode.Position.Y == end.Y)
                {
                    // Path found, reconstruct it
                    return ReconstructPath(currentNode);
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode.Position);
                
                // Check all neighbors
                foreach (var neighbor in GetNeighbors(currentNode.Position, grid))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    var tile = grid.GetTile(neighbor.X, neighbor.Y);
                    if (tile == null || !tile.IsWalkable)
                        continue;

                    float tentativeGCost = currentNode.GCost + GetDistance(currentNode.Position, neighbor);
                    
                    var neighborNode = openSet.FirstOrDefault(n => n.Position.X == neighbor.X && n.Position.Y == neighbor.Y);
                    
                    if (neighborNode == null)
                    {
                        neighborNode = new PathNode
                        {
                            Position = neighbor,
                            GCost = tentativeGCost,
                            HCost = GetDistance(neighbor, end),
                            Parent = currentNode
                        };
                        openSet.Add(neighborNode);
                    }
                    else if (tentativeGCost < neighborNode.GCost)
                    {
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.Parent = currentNode;
                    }
                }
            }
            
            // No path found
            return null;
        }
        
        private static List<Vector2Int> ReconstructPath(PathNode endNode)
        {
            var path = new List<Vector2Int>();
            var current = endNode;
            
            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }
            
            path.Reverse();
            return path;
        }
        
        private static float GetDistance(Vector2Int a, Vector2Int b)
        {
            // Manhattan distance for grid-based movement
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
        
        private static List<Vector2Int> GetNeighbors(Vector2Int pos, VillageGrid grid)
        {
            var neighbors = new List<Vector2Int>
            {
                new Vector2Int(pos.X + 1, pos.Y),     // Right
                new Vector2Int(pos.X - 1, pos.Y),     // Left
                new Vector2Int(pos.X, pos.Y + 1),     // Down
                new Vector2Int(pos.X, pos.Y - 1),     // Up
            };
            
            // Only return neighbors that are within grid bounds
            return neighbors.Where(n => 
                n.X >= 0 && n.X < grid.Width && 
                n.Y >= 0 && n.Y < grid.Height
            ).ToList();
        }
    }
}
