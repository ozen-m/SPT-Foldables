using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Foldables.Patches.Debug;
using Foldables.Patches.Mappings;
using Foldables.Patches.Operations;
using Foldables.Patches.Operations.InRaid;
using Foldables.Patches.Sizes;
using System.Linq;

namespace Foldables;

[BepInPlugin("com.ozen.foldables", "Foldables", "0.0.3")]
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
        new CallToFoldItemPatch().Enable();
        new StopProcessesPatch().Enable();
        //new LootItemScalePatch().Enable();

        // Sizes
        new CalculateExtraSizePatch().Enable();
        new ResizeHelperPatch().Enable();
        new ImageRaycastPatch().Enable();
        new UpdateScalePatch().Enable();

        // Mod compatibilities
        if (Chainloader.PluginInfos.Keys.Contains("com.ozen.continuousloadammo"))
        {
            new InventoryScreenClosePatch().Enable();
        }

        // Debug
        new DebugPatches().Enable();

    }
}
