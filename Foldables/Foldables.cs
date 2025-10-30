using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Foldables.Models.Items;
using Foldables.Models.Templates;
using Foldables.Patches.Debug;
using Foldables.Patches.Mappings;
using Foldables.Patches.Operations;
using Foldables.Patches.Operations.InRaid;
using Foldables.Patches.Sizes;

namespace Foldables;

[BepInPlugin("com.ozen.foldables", "Foldables", "1.0.0")]
public class Foldables : BaseUnityPlugin
{
    public static ManualLogSource LogSource;
    public static ConfigEntry<bool> Enabled;

    protected void Awake()
    {
        LogSource = Logger;
        Enabled = Config.Bind("General", "Enabled", true, new ConfigDescription("Enabled", null, new ConfigurationManagerAttributes() { Order = 0 }));

        // Mappings
        new MappingsPatches().Enable();

        // Operations
        new ContextInteractionsPatches().Enable();
        new SearchableViewPatches().Enable();
        new SoundPatches().Enable();
        new InteractionSwitcherPatch().Enable();
        new OnDragPatch().Enable();
        new ItemNamePatch().Enable();
        new FindLocationPatch().Enable();
        // In raid
        new GetActionsPatch().Enable();
        new ActionsNamePatch().Enable();
        //new LootItemScalePatch().Enable();

        // Sizes
        new CalculateExtraSizePatch().Enable();
        new ResizeHelperPatch().Enable();
        new ImageRaycastPatch().Enable();
        new UpdateScalePatch().Enable();

        // Debug
        new DebugPatches().Enable();
    }
}
