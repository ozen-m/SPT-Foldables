using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace Foldables;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.ozen.foldables";
    public string Name { get; init; } = "Foldables";
    public string Author { get; init; } = "ozen";
    public List<string> Contributors { get; init; } = [];
    public Version Version { get; init; } = new("1.1.1");
    public Range SptVersion { get; init; } = new("~4.1.2");
    public bool HasPrepatcher { get; init; } = false;
    public List<string> Incompatibilities { get; init; } = [];
    public Dictionary<string, Range> ModDependencies { get; init; }
    public string Url { get; init; } = "https://github.com/ozen-m/SPT-Foldables";
    public string License { get; init; } = "MIT";
}
