using System.Reflection;
using EFT.InventoryLogic;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Prevent from being able to transfer an item to a folded container
/// </summary>
public class FindLocationPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Grid).GetMethod(nameof(Grid.TryFindLocationForItem));
    }

    [PatchPrefix]
    protected static bool Prefix(Grid __instance, ref ItemAddress location, ref bool __result)
    {
        if (__instance.ParentItem.IsFoldableFolded())
        {
            location = null;
            __result = false;
            return false;
        }
        return true;
    }
}
