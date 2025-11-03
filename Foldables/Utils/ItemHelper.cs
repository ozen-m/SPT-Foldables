using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using System.Collections.Generic;

namespace Foldables.Utils;

public static class ItemHelper
{
    public static bool IsFoldableFolded(this Item item)
    {
        return item is IFoldable foldableItem && foldableItem.Folded;
    }

    public static void FoldItem(this Item item) => ItemUiContext.Instance.FoldItem(item);

    /// <summary>
    /// Fold item with delay. Delays only in raid
    /// </summary>
    public static void FoldItemWithDelay(this Item item, ItemContextAbstractClass itemContextAbstractClass = null, Callback callback = null) => ItemUiContext.Instance.FoldItemWithDelay(item, itemContextAbstractClass, callback);

    /// <summary>
    /// Move contained items to item's parent
    /// </summary>
    /// <param name="item">The item whose contents will be spilled</param>
    /// <returns>True if possible/succeded</returns>
    public static bool TryMoveContainedItemsToParent(this Item item, InventoryController inventoryController, bool simulate = true)
    {
        if (item.Parent.Container.ParentItem is InventoryEquipment || item is not CompoundItem compoundItem)
        {
            return false;
        }

        var succeeded = true;
        Stack<GStruct153> operations = new();

        // Fold item when simulating - when not simulating, it is assumed item is already folded THEN move items to parent (call to this happens after folding)
        if (simulate)
        {
            var foldableComponent = item.GetItemComponent<FoldableComponent>();
            operations.Push(InteractionsHandlerClass.Fold(foldableComponent, !foldableComponent.Folded, false));
        }

        foreach (var container in compoundItem.Grids)
        {
            List<Item> containedItems = [.. container.Items];
            foreach (var containedItem in containedItems)
            {
                var moveResult = InteractionsHandlerClass.QuickFindAppropriatePlace(
                    containedItem,
                    inventoryController,
                    [item.Parent.Container.ParentItem as CompoundItem],
                    InteractionsHandlerClass.EMoveItemOrder.MoveToAnotherSide,
                    false // Do not simulate since the next result depends on the last
                    );
                if (moveResult.Succeeded)
                {
                    operations.Push(moveResult);
                }
                else
                {
                    succeeded = false;
                    break;
                }
            }
        }

        if (!simulate && succeeded)
        {
            while (operations.TryPop(out var moveOp))
            {
                // Need to undo since we didn't simulate
                moveOp.Value.RollBack();
                inventoryController.TryRunNetworkTransaction(moveOp);
            }
        }
        else
        {
            while (operations.TryPop(out var moveOp))
            {
                moveOp.Value.RollBack();
            }
        }

        return succeeded;
    }

    public static bool IsEmptyNonLinq(this Item item)
    {
        if (item is CompoundItem compoundItem)
        {
            foreach (StashGridClass grid in compoundItem.Grids)
            {
                if (grid.Gclass3120_0.Count > 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
