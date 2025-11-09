using System.Text.Json.Serialization;
using JetBrains.Annotations;
using SPTarkov.Server.Core.Models.Spt.Inventory;

namespace Foldables.Models;

public record OverrideProperties
{
    [JsonPropertyName("Foldable")]
    public bool Foldable { get; set; } = true;

    [JsonPropertyName("FoldedSize")]
    [UsedImplicitly]
    public ItemSize ItemSize { get; set; }

    [JsonPropertyName("FoldingTime")]
    [UsedImplicitly]
    public double? FoldingTime { get; set; }

    [JsonExtensionData]
    [UsedImplicitly]
    public Dictionary<string, object> ExtensionData { get; set; } = [];
}
