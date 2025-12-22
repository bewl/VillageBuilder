using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.Commands.WorkerCommands
{
    public class AssignWorkerCommand : GameCommand
    {
        public string PersonFirstName { get; }
        public string PersonLastName { get; }
        public int BuildingX { get; }
        public int BuildingY { get; }

        public AssignWorkerCommand(int playerId, int targetTick, string firstName, string lastName, int buildingX, int buildingY)
            : base(playerId, targetTick)
        {
            PersonFirstName = firstName;
            PersonLastName = lastName;
            BuildingX = buildingX;
            BuildingY = buildingY;
        }

        public override bool CanExecute(GameEngine engine)
        {
            if (!ValidateGameState(engine))
                return false;

            var person = FindPerson(engine);
            var building = FindBuilding(engine);

            return person != null && building != null && building.IsConstructed;
        }

        public override CommandResult Execute(GameEngine engine)
        {
            var person = FindPerson(engine);
            var building = FindBuilding(engine);

            if (person == null)
                return CommandResult.Failed($"Person {PersonFirstName} {PersonLastName} not found");

            if (building == null)
                return CommandResult.Failed($"Building at ({BuildingX}, {BuildingY}) not found");

            if (!building.IsConstructed)
                return CommandResult.InvalidState($"Building {building.Name} is not yet constructed");

            // Remove from previous building
            foreach (var b in engine.Buildings)
            {
                b.Workers.Remove(person);
            }

            // Assign to new building
            building.Workers.Add(person);

            return CommandResult.Success(
                $"{person.FirstName} {person.LastName} assigned to {building.Name}",
                new Dictionary<string, object>
                {
                    { "Worker", $"{person.FirstName} {person.LastName}" },
                    { "Building", building.Name }
                }
            );
        }

        private Person? FindPerson(GameEngine engine)
        {
            return engine.Families
                .SelectMany(f => f.Members)
                .FirstOrDefault(p => p.FirstName == PersonFirstName && p.LastName == PersonLastName);
        }

        private Building? FindBuilding(GameEngine engine)
        {
            return engine.Buildings.FirstOrDefault(b => b.X == BuildingX && b.Y == BuildingY);
        }
    }
}
