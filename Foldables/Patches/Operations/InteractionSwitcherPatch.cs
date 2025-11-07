using System.Reflection;
using Comfort.Common;
using EFT.InventoryLogic;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;

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
    protected static readonly FailedResult unknownItemsInsideFail = new("Cannot fold the container with unsearched items inside");

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

        switch (button)
        {
            case EItemInfoButton.Fold or EItemInfoButton.Unfold when __result.Failed:
            {
                // Change failed result error from `stock` to `item`
                __result = foldItemFail;
                return;
            }
            case EItemInfoButton.Open when __instance.Item_0_1.IsFoldableFolded():
            {
                // If item can't unfold, then fail opening
                if (__instance.IsInteractive(EItemInfoButton.Unfold).Failed)
                {
                    __result = openContainerFail;
                }
                return;
            }
            case EItemInfoButton.Fold when !Foldables.FoldWhileEquipped.Value && __instance.Item_0_1.Parent.Container.ParentItem is InventoryEquipment:
            {
                // Config does not allow folding while item is equipped
                __result = foldItemEquippedFail;
                return;
            }
            case EItemInfoButton.Fold when !__instance.Item_0_1.IsEmptyNonLinq():
            {
                // Found unknown items inside the container
                if (__instance.TraderControllerClass.SearchController.ContainsUnknownItems(__instance.Item_0_1 as SearchableItemItemClass))
                {
                    __result = unknownItemsInsideFail;
                    return;
                }

                // If container is not empty and can spill contents, do not fail
                var inventoryController = __instance.TraderControllerClass as InventoryController;
                __result = __instance.Item_0_1.TryMoveContainedItemsToParent(inventoryController)
                    ? SuccessfulResult.New
                    : itemsInsideFail;
                return;
            }
        }
    }
}
