using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillageBuilder.Engine.Networking.Serialization
{
    /// <summary>
    /// Data Transfer Object for commands over the network
    /// </summary>
    public class CommandDto
    {
        public string CommandType { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public int TargetTick { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
