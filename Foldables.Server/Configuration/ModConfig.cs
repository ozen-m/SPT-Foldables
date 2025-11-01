using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Inventory;
using System.Text.Json.Serialization;

namespace Foldables.Configuration;

public class ModConfig
{
    [JsonPropertyName("MinFoldingTime")]
    public double MinFoldingTime { get; set; } = 1;

    [JsonPropertyName("MaxFoldingTime")]
    public double MaxFoldingTime { get; set; } = 5;

    [JsonPropertyName("DebugLogs")]
    public bool DebugLogs { get; set; } = false;

    [JsonPropertyName("Overrides")]
    public Dictionary<MongoId, OverrideProperties> Overrides { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; } = [];
}

public class OverrideProperties
{
    [JsonPropertyName("FoldedSize")]
    public ItemSize ItemSize { get; set; }

    [JsonPropertyName("FoldingTime")]
    public double? FoldingTime { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; } = [];
}
