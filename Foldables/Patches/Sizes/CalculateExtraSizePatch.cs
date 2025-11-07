using System.Reflection;
using EFT.InventoryLogic;
using Foldables.Models;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Sizes;

/// <summary>
/// Add custom property `SizeReduceDown` to calculation
/// </summary>
public class CalculateExtraSizePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CompoundItem).GetMethod(nameof(CompoundItem.CalculateExtraSize));
    }

    [PatchPostfix]
    protected static void Postfix(CompoundItem __instance, FoldableComponent overrideFoldable, bool overrideValue, Slot overrideSlot, Item overrideSlotContent, ref ExtraSize __result)
    {
        if (__instance is not IFoldable foldableItem) return;

        ExtraSize newSize = default;

        // Get size after folding
        if (overrideFoldable != null)
        {
            // If to fold, set size after folding
            if (overrideValue)
            {
                newSize.ForcedDown -= foldableItem.SizeReduceDown;
            }
        }
        // Get current size
        else
        {
            if (foldableItem.Folded)
            {
                newSize.ForcedDown -= foldableItem.SizeReduceDown;
            }
        }

        __result = ExtraSize.Merge(__result, newSize);
    }
}
