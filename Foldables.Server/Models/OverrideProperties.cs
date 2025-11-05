using SPTarkov.Server.Core.Models.Spt.Inventory;
using System.Text.Json.Serialization;

namespace Foldables.Models;

public record OverrideProperties
{
    [JsonPropertyName("Foldable")]
    public bool Foldable { get; set; } = true;

    [JsonPropertyName("FoldedSize")]
    public ItemSize ItemSize { get; set; }

    [JsonPropertyName("FoldingTime")]
    public double? FoldingTime { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; } = [];
}
