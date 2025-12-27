using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace Foldables;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.ozen.foldables";
    public override string Name { get; init; } = "Foldables";
    public override string Author { get; init; } = "ozen";
    public override List<string> Contributors { get; init; } = [];
    public override Version Version { get; init; } = new("1.0.3");
    public override Range SptVersion { get; init; } = new("~4.0.4");
    public override List<string> Incompatibilities { get; init; } = [];
    public override Dictionary<string, Range> ModDependencies { get; init; }
    public override string Url { get; init; } = "https://github.com/ozen-m/SPT-Foldables";
    public override bool? IsBundleMod { get; init; } = false;
    public override string License { get; init; } = "MIT";
}
