using System.Reflection;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using Foldables.Models.Items;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Additional checks for folding an item
/// </summary>
public class InteractionSwitcherPatch : ModulePatch
{
    private static readonly FailedResult _foldItemFail = new("Cannot fold the item now");
    private static readonly FailedResult _openContainerFail = new("Item is folded and can't unfold in place");
    private static readonly FailedResult _foldItemEquippedFail = new("Cannot fold while the item is equipped");
    private static readonly FailedResult _itemsInsideFail = new("Cannot fold the container with items inside");
    private static readonly FailedResult _unknownItemsInsideFail = new("Cannot fold the container with unsearched items inside");

    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemContextInteractionsSwitcher).GetMethod(nameof(ItemContextInteractionsSwitcher.IsInteractive));
    }

    [PatchPostfix]
    protected static void Postfix(ItemContextInteractionsSwitcher __instance, EItemInfoButton button, ref IResult __result)
    {
        if (__instance._item is not IFoldable)
        {
            return;
        }

        switch (button)
        {
            case EItemInfoButton.Fold or EItemInfoButton.Unfold when __result.Failed:
            {
                // Change failed result error from `stock` to `item`
                __result = _foldItemFail;
                return;
            }
            case EItemInfoButton.Open when __instance._item.IsFoldableFolded():
            {
                // If item can't unfold, then fail opening
                if (__instance.IsInteractive(EItemInfoButton.Unfold).Failed)
                {
                    __result = _openContainerFail;
                }
                return;
            }
            case EItemInfoButton.Fold when __instance._item is FoldableHeadphones { Parent.Container: Slot }:
            case EItemInfoButton.Fold when !Foldables.FoldWhileEquipped.Value && __instance._item is { Parent.Container: Slot }:
            {
                // Headphones cannot be folded while in a slot
                // Config does not allow folding while item is equipped
                __result = _foldItemEquippedFail;
                return;
            }
            case EItemInfoButton.Fold when !__instance._item.IsEmptyNonLinq():
            {
                // Found unknown items inside the container
                if (__instance._itemController.SearchController.ContainsUnknownItems(__instance._item as SearchableItem))
                {
                    __result = _unknownItemsInsideFail;
                    return;
                }

                // If container is not empty and can spill contents, do not fail
                var inventoryController = __instance._itemController as InventoryController;
                __result = __instance._item.TryMoveContainedItemsToParent(inventoryController) ? SuccessfulResult.New : _itemsInsideFail;
                return;
            }
        }
    }
}
