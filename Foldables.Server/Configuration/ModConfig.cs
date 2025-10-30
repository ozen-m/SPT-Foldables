using SPTarkov.Server.Core.Models.Spt.Inventory;
using System.Text.Json.Serialization;

namespace Foldables.Configuration;

public class ModConfig
{
    [JsonPropertyName("DebugLogs")]
    public bool DebugLogs { get; set; } = true;

    [JsonPropertyName("Overrides")]
    public Dictionary<string, ItemSize> Overrides { get; set; } = [];
}
