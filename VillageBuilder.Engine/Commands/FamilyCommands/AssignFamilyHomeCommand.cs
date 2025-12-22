using System;
using System.Linq;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.Commands.FamilyCommands
{
    /// <summary>
    /// Command to assign a family to a house as their home
    /// </summary>
    public class AssignFamilyHomeCommand : GameCommand
    {
        public int FamilyId { get; }
        public int BuildingId { get; }

        public AssignFamilyHomeCommand(int playerId, int targetTick, int familyId, int buildingId)
            : base(playerId, targetTick)
        {
            FamilyId = familyId;
            BuildingId = buildingId;
        }

        public override bool CanExecute(GameEngine engine)
        {
            if (!ValidateGameState(engine))
                return false;

            var family = engine.Families.FirstOrDefault(f => f.Id == FamilyId);
            var building = engine.Buildings.FirstOrDefault(b => b.Id == BuildingId);

            return family != null && 
                   building != null && 
                   building.Type == BuildingType.House && 
                   building.IsConstructed;
        }

        public override CommandResult Execute(GameEngine engine)
        {
            if (!CanExecute(engine))
                return CommandResult.Failed("Cannot assign family to home");

            var family = engine.Families.FirstOrDefault(f => f.Id == FamilyId);
            if (family == null)
                return CommandResult.Failed($"Family with ID {FamilyId} not found");

            var building = engine.Buildings.FirstOrDefault(b => b.Id == BuildingId);
            if (building == null)
                return CommandResult.Failed($"Building with ID {BuildingId} not found");

            if (building.Type != BuildingType.House)
                return CommandResult.Failed("Building must be a house to assign as family home");

            if (!building.IsConstructed)
                return CommandResult.Failed("House must be constructed before families can move in");

            // Assign all family members to this house
            foreach (var member in family.Members)
            {
                member.HomeBuilding = building;
                
                // Add to building residents if not already there
                if (!building.Residents.Contains(member))
                {
                    building.Residents.Add(member);
                }
            }

            // Update family home position to an interior floor tile (not doorway)
            var interiorPositions = building.GetInteriorPositions();
            if (interiorPositions.Count > 0)
            {
                // Use first interior position
                family.HomePosition = interiorPositions[0];
            }
            else
            {
                // Fallback to door if no interior
                var doorPositions = building.GetDoorPositions();
                if (doorPositions.Count > 0)
                {
                    family.HomePosition = doorPositions[0];
                }
                else
                {
                    family.HomePosition = new Vector2Int(building.X, building.Y);
                }
            }

            EventLog.Instance.AddMessage(
                $"The {family.FamilyName} family has moved into their new home",
                LogLevel.Success);

            return CommandResult.Success($"Family {family.FamilyName} assigned to house at ({building.X}, {building.Y})");
        }
    }
}
