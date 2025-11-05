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

    public static void FoldItem(this Item item, Callback callback = null) => ItemUiContext.Instance.FoldItem(item, callback);

    /// <summary>
    /// Fold item with delay. Delays only in raid
    /// </summary>
    public static void FoldItemWithDelay(this Item item, ItemContextAbstractClass itemContextAbstractClass = null, Callback callback = null) => ItemUiContext.Instance.FoldItemWithDelay(item, itemContextAbstractClass, callback);

    /// <summary>
    /// Move contained items to item's parent
    /// </summary>
    /// <param name="rootItem">The item whose contents will be spilled</param>
    /// <returns>True if possible/succeded</returns>
    public static bool TryMoveContainedItemsToParent(this Item rootItem, InventoryController inventoryController, bool simulate = true)
    {
        if (rootItem.Parent.Container.ParentItem is InventoryEquipment || rootItem is not CompoundItem compoundItem)
        {
            return false;
        }
        Stack<GStruct153> operations = new();

        // Fold IFoldable item when simulating - when not simulating, it is assumed item is already folded THEN move items to parent (call to this happens after folding)
        if (simulate && rootItem is IFoldable)
        {
            var foldableComponent = rootItem.GetItemComponent<FoldableComponent>();
            if (foldableComponent != null)
            {
                var foldOp = InteractionsHandlerClass.Fold(foldableComponent, !foldableComponent.Folded, false);
                if (foldOp.Failed)
                {
                    return false;
                }
                operations.Push(foldOp);
            }
        }

        var succeeded = ProcessContainerItems(rootItem, compoundItem.Grids, operations, inventoryController);
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

    private static bool ProcessContainerItems(Item rootItem, StashGridClass[] containers, Stack<GStruct153> operations, InventoryController inventoryController)
    {
        Stack<Item> containedItems = new();
        foreach (var container in containers)
        {
            // Stackable items merge to existing stacks
            while (container.ItemCollection.Count > 0)
            {
                foreach (var item in container.ItemCollection.Keys) // container.Items
                {
                    containedItems.Push(item);
                }
                while (containedItems.TryPop(out Item containedItem))
                {
                    var moveResult = InteractionsHandlerClass.QuickFindAppropriatePlace(
                        containedItem,
                        inventoryController,
                        [rootItem.Parent.Container.ParentItem as CompoundItem],
                        InteractionsHandlerClass.EMoveItemOrder.MoveToAnotherSide,
                        false // Do not simulate since the next result depends on the last
                        );
                    if (moveResult.Succeeded)
                    {
                        operations.Push(moveResult);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public static bool IsEmptyNonLinq(this Item item)
    {
        if (item is CompoundItem compoundItem)
        {
            foreach (StashGridClass grid in compoundItem.Grids)
            {
                if (grid.ItemCollection.Count > 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
