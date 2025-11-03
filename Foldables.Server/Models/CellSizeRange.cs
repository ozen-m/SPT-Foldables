using SPTarkov.Server.Core.Models.Spt.Inventory;
using System.Text.Json.Serialization;

namespace Foldables.Models;

public record CellSizeRange
{
    [JsonPropertyName("MaxGridCount")]
    public int MaxGridCount { get; init; }

    [JsonPropertyName("CellSize")]
    public ItemSize CellSize { get; init; }
}
