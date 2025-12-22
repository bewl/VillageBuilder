using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillageBuilder.Engine.Commands
{
    public enum CommandStatus
    {
        Success,
        Failed,
        InvalidState,
        InsufficientResources,
        InvalidTarget,
        Unauthorized
    }

    public class CommandResult
    {
        public CommandStatus Status { get; }
        public string Message { get; }
        public Dictionary<string, object>? Data { get; }

        public CommandResult(CommandStatus status, string message, Dictionary<string, object>? data = null)
        {
            Status = status;
            Message = message;
            Data = data;
        }

        public bool IsSuccess => Status == CommandStatus.Success;

        public static CommandResult Success(string message = "Command executed successfully", Dictionary<string, object>? data = null)
            => new(CommandStatus.Success, message, data);

        public static CommandResult Failed(string message, Dictionary<string, object>? data = null)
            => new(CommandStatus.Failed, message, data);

        public static CommandResult InvalidState(string message)
            => new(CommandStatus.InvalidState, message);

        public static CommandResult InsufficientResources(string message)
            => new(CommandStatus.InsufficientResources, message);
    }
}
