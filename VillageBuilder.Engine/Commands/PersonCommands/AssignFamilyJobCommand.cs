using System;
using System.Linq;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.Commands.PersonCommands
{
    public class AssignFamilyJobCommand : GameCommand
    {
        public int FamilyId { get; }
        public int BuildingId { get; }

        public AssignFamilyJobCommand(int playerId, int targetTick, int familyId, int buildingId)
            : base(playerId, targetTick)
        {
            FamilyId = familyId;
            BuildingId = buildingId;
        }

        public override bool CanExecute(GameEngine engine)
        {
            if (!ValidateGameState(engine))
                return false;

            var family = GetFamily(engine);
            var building = GetBuilding(engine);

            if (family == null || family.Members.Count == 0)
                return false;

            if (building == null || !building.IsConstructed)
                return false;

            // Check if at least one adult family member is available
            if (!family.GetAdults().Any(p => p.IsAlive))
                return false;

            return true;
        }

        public override CommandResult Execute(GameEngine engine)
        {
            Console.WriteLine($"[JOB] Executing AssignFamilyJobCommand for Family {FamilyId} to Building {BuildingId}");
            
            if (!CanExecute(engine))
            {
                Console.WriteLine($"[JOB] Assignment failed - CanExecute returned false");
                return CommandResult.Failed($"Cannot assign family to job");
            }

            var family = GetFamily(engine);
            var building = GetBuilding(engine);

            if (family == null || building == null)
            {
                Console.WriteLine($"[JOB] Assignment failed - Family: {family != null}, Building: {building != null}");
                return CommandResult.Failed($"Family or building not found");
            }

            // Get all working-age adults (18+)
            var workingMembers = family.GetAdults().Where(p => p.IsAlive).ToList();
            
            Console.WriteLine($"[JOB] {family.FamilyName} has {workingMembers.Count} working adults");

            if (workingMembers.Count == 0)
            {
                return CommandResult.Failed($"The {family.FamilyName} family has no adults available to work");
            }

            // Get interior positions (floor tiles inside the building)
            var interiorPositions = building.GetInteriorPositions();
            
            // Fallback to door positions if no interior
            if (interiorPositions.Count == 0)
            {
                interiorPositions = building.GetDoorPositions();
            }
            
            // Assign each working adult
            int assignedCount = 0;
            var usedPositions = new HashSet<Vector2Int>(); // Track which positions we've assigned

            foreach (var person in workingMembers)
            {
                Vector2Int? targetPosition = null;

                // Try to find an unoccupied position that hasn't been assigned yet
                var allPositions = building.GetInteriorPositions();
                if (allPositions.Count == 0)
                {
                    allPositions = building.GetDoorPositions();
                }

                // Filter out already-used positions
                var availablePositions = allPositions.Where(p => !usedPositions.Contains(p)).ToList();

                if (availablePositions.Count > 0)
                {
                    // From available positions, prefer empty tiles
                    targetPosition = availablePositions
                        .OrderBy(p => engine.Grid.GetTile(p.X, p.Y)?.PeopleOnTile.Count ?? 0)
                        .ThenBy(p => Math.Abs(p.X - person.Position.X) + Math.Abs(p.Y - person.Position.Y))
                        .FirstOrDefault();
                }
                else if (allPositions.Count > 0)
                {
                    // All positions used, find least crowded
                    targetPosition = allPositions
                        .OrderBy(p => engine.Grid.GetTile(p.X, p.Y)?.PeopleOnTile.Count ?? 0)
                        .ThenBy(p => Math.Abs(p.X - person.Position.X) + Math.Abs(p.Y - person.Position.Y))
                        .FirstOrDefault();
                }

                if (targetPosition == null)
                {
                    Console.WriteLine($"[JOB] No valid position found for {person.FirstName}");
                    continue;
                }

                // Mark this position as used
                usedPositions.Add(targetPosition.Value);

                // Calculate path to unoccupied position
                var path = Pathfinding.FindPath(person.Position, targetPosition.Value, engine.Grid);

                if (path != null && path.Count > 0)
                {
                    Console.WriteLine($"[JOB] {person.FirstName} pathfinding: {path.Count} steps to position at ({targetPosition.Value.X},{targetPosition.Value.Y})");

                    // Set path and assign job (without logging for each person)
                    person.SetPath(path);
                    person.AssignToBuilding(building, logEvent: false);
                    assignedCount++;
                }
                else
                {
                    Console.WriteLine($"[JOB] No path found for {person.FirstName} to position at ({targetPosition.Value.X},{targetPosition.Value.Y})");
                }
            }

            if (assignedCount == 0)
            {
                Console.WriteLine($"[JOB] Assignment failed - No workers could reach the building");
                return CommandResult.Failed($"The {family.FamilyName} family cannot reach the {building.Type}");
            }

            Console.WriteLine($"[JOB] Successfully assigned {assignedCount} workers from {family.FamilyName}");

            // Log family assignment as one event
            EventLog.Instance.AddMessage(
                $"The {family.FamilyName} family ({assignedCount} workers) is traveling to the {building.Type}",
                LogLevel.Info);

            return CommandResult.Success(
                $"{family.FamilyName} family assigned to {building.Name}",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { "FamilyId", FamilyId },
                    { "BuildingId", BuildingId },
                    { "WorkersAssigned", assignedCount }
                }
            );
        }

        private Family? GetFamily(GameEngine engine)
        {
            return engine.Families.FirstOrDefault(f => f.Id == FamilyId);
        }

        private Building? GetBuilding(GameEngine engine)
        {
            return engine.Buildings.FirstOrDefault(b => b.Id == BuildingId);
        }
    }
}
