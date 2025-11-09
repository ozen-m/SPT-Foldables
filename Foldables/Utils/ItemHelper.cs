using System.Collections.Generic;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;

namespace Foldables.Utils;

public static class ItemHelper
{
    public static bool IsFoldableFolded(this Item item) => item is IFoldable { Folded: true };

    public static void FoldItem(this Item item, Callback callback = null)
        => ItemUiContext.Instance.FoldItem(item, callback);

    /// <summary>
    /// Fold item with delay. Delays only in raid
    /// </summary>
    public static void FoldItemWithDelay(
        this Item item,
        ItemContextAbstractClass itemContextAbstractClass = null,
        Callback callback = null)
        => ItemUiContext.Instance.FoldItemWithDelay(item, itemContextAbstractClass, callback);

    /// <summary>
    /// Check if item is not empty before folding
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool RequiresEmptyingBeforeFold(this Item item)
        => item is IFoldable { Folded: false } && !item.IsEmptyNonLinq();

    /// <summary>
    /// Move contained items to item's parent
    /// </summary>
    /// <param name="rootItem">The item whose contents will be spilled</param>
    /// <returns>True if possible/succeeded</returns>
    public static bool TryMoveContainedItemsToParent(
        this Item rootItem,
        InventoryController inventoryController,
        bool simulate = true)
    {
        if (rootItem.Parent.Container.ParentItem is InventoryEquipment || rootItem is not CompoundItem compoundItem)
        {
            // Do not move contents if item is equipped
            return false;
        }
        Stack<GStruct153> operations = new();

        /*Fold IFoldable item when simulating
        When not simulating, it is assumed item is already folded
        THEN move items to parent (call to this happens after folding)*/
        if (simulate && rootItem is IFoldable)
        {
            var foldableComponent = rootItem.GetItemComponent<FoldableComponent>();
            if (foldableComponent != null)
            {
                var foldOp = InteractionsHandlerClass.Fold(
                    foldableComponent,
                    !foldableComponent.Folded,
                    false
                );
                if (foldOp.Failed)
                {
                    return false;
                }
                operations.Push(foldOp);
            }
        }

        bool succeeded = ProcessContainerItems(rootItem, compoundItem.Grids, operations, inventoryController);
        if (!simulate && succeeded)
        {
            while (operations.TryPop(out var moveOp))
            {
                // Need to undo since we didn't simulate
                moveOp.Value.RollBack();
                _ = inventoryController.TryRunNetworkTransaction(moveOp);
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
        if (item is not CompoundItem compoundItem) return true;

        foreach (var grid in compoundItem.Grids)
        {
            if (grid.ItemCollection.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

    private static bool ProcessContainerItems(
        Item rootItem,
        StashGridClass[] containers,
        Stack<GStruct153> operations,
        InventoryController inventoryController)
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
                while (containedItems.TryPop(out var containedItem))
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
}
