using System.Text.Json.Serialization;
using JetBrains.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace Foldables.Models;

public record ModConfig
{
    [JsonPropertyName("MinFoldingTime")]
    public double MinFoldingTime { get; set; } = 1;

    [JsonPropertyName("MaxFoldingTime")]
    public double MaxFoldingTime { get; set; } = 5;

    [JsonPropertyName("DebugLogs")]
    public bool DebugLogs { get; set; } = false;

    [JsonPropertyName("BackpackFoldedCellSizes")]
    public CellSizeRange[] BackpackFoldedCellSizes { get; set; }

    [JsonPropertyName("VestFoldedCellSizes")]
    public CellSizeRange[] VestFoldedCellSizes { get; set; }
    
    [JsonPropertyName("HeadphonesFoldedCellSizes")]
    public CellSizeRange[] HeadphonesFoldedCellSizes { get; set; }

    [JsonPropertyName("Overrides")]
    [UsedImplicitly]
    public Dictionary<MongoId, OverrideProperties> Overrides { get; set; } = [];

    [JsonExtensionData]
    [UsedImplicitly]
    public Dictionary<string, object> ExtensionData { get; set; } = [];
}
