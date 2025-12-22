using System;
using System.Linq;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.Commands.PersonCommands
{
    public class AssignJobCommand : GameCommand
    {
        public int PersonId { get; }
        public int BuildingId { get; }

        public AssignJobCommand(int playerId, int targetTick, int personId, int buildingId)
            : base(playerId, targetTick)
        {
            PersonId = personId;
            BuildingId = buildingId;
        }

        public override bool CanExecute(GameEngine engine)
        {
            if (!ValidateGameState(engine))
                return false;

            var person = GetPerson(engine);
            var building = GetBuilding(engine);

            if (person == null || !person.IsAlive)
                return false;

            if (building == null || !building.IsConstructed)
                return false;

            return true;
        }

        public override CommandResult Execute(GameEngine engine)
        {
            if (!CanExecute(engine))
                return CommandResult.Failed($"Cannot assign job");

            var person = GetPerson(engine);
            var building = GetBuilding(engine);

            if (person == null || building == null)
                return CommandResult.Failed($"Person or building not found");

            // Find nearest door to the building
            var doorPositions = building.GetDoorPositions();
            var targetDoor = doorPositions.OrderBy(d => 
                Math.Abs(d.X - person.Position.X) + Math.Abs(d.Y - person.Position.Y)
            ).FirstOrDefault();

            if (targetDoor.X == 0 && targetDoor.Y == 0 && doorPositions.Count > 0)
            {
                targetDoor = doorPositions[0];
            }

            // Calculate path to building door
            var path = Pathfinding.FindPath(person.Position, targetDoor, engine.Grid);

            if (path == null || path.Count == 0)
            {
                return CommandResult.Failed($"{person.FirstName} cannot reach the {building.Type}");
            }

            // Set path and assign job
            person.SetPath(path);
            person.AssignToBuilding(building);

            EventLog.Instance.AddMessage(
                $"{person.FirstName} {person.LastName} is traveling to work at the {building.Type}",
                LogLevel.Info);

            return CommandResult.Success(
                $"{person.FullName} assigned to {building.Name}",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { "PersonId", PersonId },
                    { "BuildingId", BuildingId }
                }
            );
        }

        private Person? GetPerson(GameEngine engine)
        {
            return engine.Families
                .SelectMany(f => f.Members)
                .FirstOrDefault(p => p.Id == PersonId);
        }

        private Building? GetBuilding(GameEngine engine)
        {
            return engine.Buildings.FirstOrDefault(b => b.Id == BuildingId);
        }
    }
}
