using SPTarkov.Server.Core.Models.Spt.Mod;

namespace Foldables;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.ozen.foldables";
    public override string Name { get; init; } = "Foldables";
    public override string Author { get; init; } = "ozen";
    public override List<string> Contributors { get; init; } = [];
    public override SemanticVersioning.Version Version { get; init; } = new("0.0.3");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.2");
    public override List<string> Incompatibilities { get; init; } = [];
    public override Dictionary<string, SemanticVersioning.Range> ModDependencies { get; init; }
    public override string Url { get; init; } = "https://github.com/ozen-m/SPT-Foldables";
    public override bool? IsBundleMod { get; init; } = false;
    public override string License { get; init; } = "MIT";
}
