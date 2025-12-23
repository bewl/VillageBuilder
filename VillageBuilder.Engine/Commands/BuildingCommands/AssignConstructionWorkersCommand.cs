using System;
using System.Linq;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.Commands.BuildingCommands
{
    /// <summary>
    /// Assigns a family to construction work on an unfinished building
    /// </summary>
    public class AssignConstructionWorkersCommand : GameCommand
    {
        public int FamilyId { get; }
        public int BuildingId { get; }

        public AssignConstructionWorkersCommand(int playerId, int targetTick, int familyId, int buildingId)
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

            // Building must exist and NOT be constructed yet
            if (building == null || building.IsConstructed)
                return false;

            // Check if at least one adult family member is available
            if (!family.GetAdults().Any(p => p.IsAlive))
                return false;

            return true;
        }

        public override CommandResult Execute(GameEngine engine)
        {
            if (!CanExecute(engine))
                return CommandResult.Failed($"Cannot assign family to construction");

            var family = GetFamily(engine);
            var building = GetBuilding(engine);

            if (family == null || building == null)
                return CommandResult.Failed($"Family or building not found");

            // Get all working-age adults (18+) who are idle
            var idleAdults = family.GetAdults()
                .Where(p => p.IsAlive && p.CurrentTask == PersonTask.Idle)
                .ToList();

            if (idleAdults.Count == 0)
                return CommandResult.Failed($"The {family.FamilyName} family has no idle adults available");

            // Get door positions or building perimeter for construction work
            var workPositions = building.GetDoorPositions();
            if (workPositions.Count == 0)
            {
                // Fallback to building position
                workPositions.Add(new Vector2Int(building.X, building.Y));
            }

            // Assign each idle adult to construction
            int assignedCount = 0;
            foreach (var person in idleAdults)
            {
                // Find least crowded work position (prefer empty tiles)
                var targetPosition = workPositions
                    .OrderBy(p => engine.Grid.GetTile(p.X, p.Y)?.PeopleOnTile.Count ?? 0)
                    .ThenBy(p => Math.Abs(p.X - person.Position.X) + Math.Abs(p.Y - person.Position.Y))
                    .First();

                // Calculate path to work site
                var path = Pathfinding.FindPath(person.Position, targetPosition, engine.Grid);

                if (path != null && path.Count > 0)
                {
                    person.SetPath(path);
                    person.CurrentTask = PersonTask.Constructing;

                    // Add to building's construction workers
                    if (!building.ConstructionWorkers.Contains(person))
                    {
                        building.ConstructionWorkers.Add(person);
                    }

                    assignedCount++;
                }
            }

            if (assignedCount == 0)
                return CommandResult.Failed($"The {family.FamilyName} family cannot reach the construction site");

            EventLog.Instance.AddMessage(
                $"The {family.FamilyName} family ({assignedCount} workers) is heading to construct the {building.Type}",
                LogLevel.Info);

            return CommandResult.Success(
                $"{family.FamilyName} family assigned to construction",
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
