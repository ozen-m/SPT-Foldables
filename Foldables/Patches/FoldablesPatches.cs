using System.Linq;
using BepInEx.Bootstrap;
using Foldables.Patches.Mappings;
using Foldables.Patches.Operations;
using Foldables.Patches.Operations.InRaid;
using Foldables.Patches.Sizes;

namespace Foldables.Patches;

public class FoldablesPatches
{
    public void Enable()
    {
        // Mappings
        new MappingsPatches().Enable();

        // Operations
        // Searchable View
        new OnAddToSlotPatch().Enable();
        new OnRemoveFromSlotPatch().Enable();
        new SearchableItemViewShowPatch().Enable();
        //new SoundPatches().Enable();
        new UnfoldOnOpenInteractionPatch().Enable();
        new InteractionSwitcherPatch().Enable();
        new OnDragPatch().Enable();
        new ItemNamePatch().Enable();
        new FindLocationPatch().Enable();
        // In-raid
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
    }
}
