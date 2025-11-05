using Comfort.Common;
using EFT.InventoryLogic;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Foldables.Patches.Operations;

/// <summary>
/// Additional checks for folding an item
/// </summary>
public class InteractionSwitcherPatch : ModulePatch
{
    protected static readonly FailedResult foldItemFail = new("Cannot fold the item now");
    protected static readonly FailedResult openContainerFail = new("Item is folded and can't unfold in place");
    protected static readonly FailedResult foldItemEquippedFail = new("Cannot fold while the item is equipped");
    protected static readonly FailedResult itemsInsideFail = new("Cannot fold the container with items inside");

    protected override MethodBase GetTargetMethod()
    {
        return typeof(ContextInteractionSwitcherClass).GetMethod(nameof(ContextInteractionSwitcherClass.IsInteractive));
    }

    [PatchPostfix]
    protected static void Postfix(ContextInteractionSwitcherClass __instance, EItemInfoButton button, ref IResult __result)
    {
        if (__instance.Item_0_1 is not IFoldable)
        {
            return;
        }

        if (__result.Failed && (button == EItemInfoButton.Fold || button == EItemInfoButton.Unfold))
        {
            // Change failed result from `stock` to `item`
            __result = foldItemFail;
        }
        else if (button == EItemInfoButton.Open && __instance.Item_0_1.IsFoldableFolded())
        {
            // If item can't unfold, then fail opening
            if (__instance.IsInteractive(EItemInfoButton.Unfold).Failed)
            {
                __result = openContainerFail;
            }
        }
        else if (button == EItemInfoButton.Fold)
        {
            if (!Foldables.FoldWhileEquipped.Value && __instance.Item_0_1.Parent.Container.ParentItem is InventoryEquipment)
            {
                __result = foldItemEquippedFail;
            }
            else if (!__instance.Item_0_1.IsEmptyNonLinq())
            {
                if (__instance.Item_0_1.TryMoveContainedItemsToParent(__instance.TraderControllerClass as InventoryController))
                {
                    // If can spill contents, do not fail
                    __result = SuccessfulResult.New;
                }
                else
                {
                    __result = itemsInsideFail;
                }
            }
        }
    }
}
// rip nesting
