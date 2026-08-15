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
        return typeof(ItemManipulator).GetMethod(nameof(ItemManipulator.CanFold));
    }

    [PatchPrefix]
    protected static bool Prefix(Item item, ref bool __result)
    {
        if (item is not Headwear)
        {
            return true;
        }

        __result = false;
        return false;
    }
}
