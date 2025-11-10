using System.Reflection;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Prevent headwear/helmets from folding.
/// Needed because Fold/Unfold option is registered when FoldableComponent is seen on item's children components.
/// </summary>
public class CanFoldPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(InteractionsHandlerClass).GetMethod(nameof(InteractionsHandlerClass.CanFold));
    }

    [PatchPrefix]
    protected static bool Prefix(InteractionsHandlerClass __instance, Item item, ref bool __result)
    {
        if (item is not HeadwearItemClass) return true;

        __result = false;
        return false;
    }
}
