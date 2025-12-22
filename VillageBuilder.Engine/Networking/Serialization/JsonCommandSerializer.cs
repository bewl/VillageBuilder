using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Commands;
using VillageBuilder.Engine.Commands.BuildingCommands;
using VillageBuilder.Engine.Commands.ResourceCommands;
using VillageBuilder.Engine.Commands.WorkerCommands;
using VillageBuilder.Engine.Resources;

namespace VillageBuilder.Engine.Networking.Serialization
{
    public class JsonCommandSerializer : ICommandSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonCommandSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public string Serialize(ICommand command)
        {
            var dto = CommandToDto(command);
            return JsonSerializer.Serialize(dto, _options);
        }

        public ICommand? Deserialize(string data)
        {
            var dto = JsonSerializer.Deserialize<CommandDto>(data, _options);
            return dto == null ? null : DtoToCommand(dto);
        }

        public byte[] SerializeToBytes(ICommand command)
        {
            var json = Serialize(command);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public ICommand? DeserializeFromBytes(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            return Deserialize(json);
        }

        private CommandDto CommandToDto(ICommand command)
        {
            var dto = new CommandDto
            {
                CommandType = command.GetType().Name,
                CommandId = command.CommandId.ToString(),
                PlayerId = command.PlayerId,
                TargetTick = command.TargetTick,
                Parameters = new Dictionary<string, object>()
            };

            // Serialize specific command parameters
            switch (command)
            {
                case ConstructBuildingCommand buildCmd:
                    dto.Parameters["BuildingType"] = buildCmd.BuildingType.ToString();
                    dto.Parameters["X"] = buildCmd.X;
                    dto.Parameters["Y"] = buildCmd.Y;
                    break;

                case AssignWorkerCommand workerCmd:
                    dto.Parameters["PersonFirstName"] = workerCmd.PersonFirstName;
                    dto.Parameters["PersonLastName"] = workerCmd.PersonLastName;
                    dto.Parameters["BuildingX"] = workerCmd.BuildingX;
                    dto.Parameters["BuildingY"] = workerCmd.BuildingY;
                    break;

                case TransferResourceCommand transferCmd:
                    dto.Parameters["ResourceType"] = transferCmd.ResourceType.ToString();
                    dto.Parameters["Amount"] = transferCmd.Amount;
                    dto.Parameters["FromBuildingX"] = transferCmd.FromBuildingX;
                    dto.Parameters["FromBuildingY"] = transferCmd.FromBuildingY;
                    dto.Parameters["ToBuildingX"] = transferCmd.ToBuildingX;
                    dto.Parameters["ToBuildingY"] = transferCmd.ToBuildingY;
                    break;
            }

            return dto;
        }

        private ICommand? DtoToCommand(CommandDto dto)
        {
            return dto.CommandType switch
            {
                nameof(ConstructBuildingCommand) => new ConstructBuildingCommand(
                    dto.PlayerId,
                    dto.TargetTick,
                    Enum.Parse<BuildingType>(dto.Parameters["BuildingType"].ToString()!),
                    ((JsonElement)dto.Parameters["X"]).GetInt32(),
                    ((JsonElement)dto.Parameters["Y"]).GetInt32()
                ),

                nameof(AssignWorkerCommand) => new AssignWorkerCommand(
                    dto.PlayerId,
                    dto.TargetTick,
                    dto.Parameters["PersonFirstName"].ToString()!,
                    dto.Parameters["PersonLastName"].ToString()!,   
                    ((JsonElement)dto.Parameters["BuildingX"]).GetInt32(),
                    ((JsonElement)dto.Parameters["BuildingY"]).GetInt32()
                ),

                nameof(TransferResourceCommand) => new TransferResourceCommand(
                    dto.PlayerId,
                    dto.TargetTick,
                    Enum.Parse<ResourceType>(dto.Parameters["ResourceType"].ToString()!),
                    ((JsonElement)dto.Parameters["Amount"]).GetInt32(),
                    ((JsonElement)dto.Parameters["FromBuildingX"]).GetInt32(),
                    ((JsonElement)dto.Parameters["FromBuildingY"]).GetInt32(),
                    ((JsonElement)dto.Parameters["ToBuildingX"]).GetInt32(),
                    ((JsonElement)dto.Parameters["ToBuildingY"]).GetInt32()
                ),

                _ => null
            };
        }
    }
}
