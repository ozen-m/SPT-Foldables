using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using HarmonyLib;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Foldables.Utils;

public static class ItemUiContextExtensions
{
    private static readonly AccessTools.FieldRef<ItemUiContext, InventoryController> _inventoryControllerField =
        AccessTools.FieldRefAccess<ItemUiContext, InventoryController>("inventoryController_0");

    private static CancellationTokenSource _foldingCts;

    public static bool IsFolding => _foldingCts != null;

    /// <summary>
    /// <see cref="ItemUiContext.FoldItem"/> with callback
    /// </summary>
    public static void FoldItem(
        this ItemUiContext itemUiContext,
        Item item,
        Callback callback,
        InventoryController inventoryController = null /*Is this really necessary*/
    )
    {
        if (!InteractionsHandlerClass.CanFold(item, out var foldableComponent))
        {
            return;
        }
        item.PlayFoldSound();
        var foldEvent = InteractionsHandlerClass.Fold(foldableComponent, !foldableComponent.Folded, true);
        inventoryController ??= _inventoryControllerField(itemUiContext);
        _ = inventoryController.TryRunNetworkTransaction(foldEvent, callback);
    }

    /// <summary>
    /// Fold Item with Delay. Delays only in raid
    /// </summary>
    public static void FoldItemWithDelay(
        this ItemUiContext itemUiContext,
        Item item,
        ItemContextAbstractClass itemContextAbstractClass = null,
        Callback callback = null)
    {
        if (item is not IFoldable foldableItem)
        {
            itemUiContext.FoldItem(item, callback);
            return;
        }

        if (!GClass2340.InRaid || foldableItem.FoldingTime <= 0f)
        {
            // Close the "open" grid window when item is folded
            itemContextAbstractClass?.CloseDependentWindows();

            // If to fold but not empty, ask if player wants to spill container contents
            if (item.RequiresEmptyingBeforeFold())
                _ = HandleNonEmptyFoldingAsync(itemUiContext, item, callback);
            else
                itemUiContext.FoldItem(item, callback);

            return;
        }

        var inventoryController = _inventoryControllerField(itemUiContext);
        if (inventoryController is Player.PlayerInventoryController playerInventoryController)
        {
            inventoryController.StopProcesses();
            StopFolding();
            _foldingCts = new CancellationTokenSource();

            playerInventoryController.Player_0.StartCoroutine(
                FoldingDelay(
                    item,
                    foldableItem.FoldingTime,
                    inventoryController,
                    itemUiContext,
                    itemContextAbstractClass,
                    _foldingCts.Token,
                    callback
                ));
        }
    }

    private static async Task HandleNonEmptyFoldingAsync(ItemUiContext itemUiContext, Item item, Callback callback = null, InventoryController inventoryController = null)
    {
        inventoryController ??= _inventoryControllerField(itemUiContext);
        if (item.TryMoveContainedItemsToParent(inventoryController) && (!Foldables.ShowSpillDialog.Value || await itemUiContext.ShowSpillAndFoldDialogAsync(item)))
        {
            callback += (result) =>
            {
                if (result.Succeed)
                {
                    item.TryMoveContainedItemsToParent(inventoryController, false);
                }
            };
            itemUiContext.FoldItem(item, callback);
        }
        else
        {
            NotificationManagerClass.DisplayWarningNotification("Cannot fold the container with items inside".Localized());
            callback?.Fail("Cannot fold the container with items inside");
        }
    }

    private static async Task<bool> ShowSpillAndFoldDialogAsync(this ItemUiContext itemUiContext, Item item)
    {
        var inventoryController = _inventoryControllerField(itemUiContext);
        var itemName = inventoryController.Examined(item) ? item.ShortName : "Unknown item";
        var description = string.Format("Do you want to spill the contents of {0} and fold?".Localized(), itemName.Localized());

        return await itemUiContext.ShowMessageWindow(out _, description, null, true);
    }

    private static IEnumerator FoldingDelay(
        Item item,
        float seconds,
        InventoryController inventoryController,
        ItemUiContext itemUiContext,
        ItemContextAbstractClass itemContextAbstractClass,
        CancellationToken token,
        Callback callback = null)
    {
        // Use LoadMagazineEvent for our UI
        var owner = item.Owner;
        var startFoldEvent = new GEventArgs7(null, item, 1, seconds, CommandStatus.Begin, owner);
        var stopFoldEvent = new GEventArgs7(null, item, 1, seconds, CommandStatus.Succeed, owner);
        inventoryController.RaiseLoadMagazineEvent(startFoldEvent);

        var timer = Stopwatch.StartNew();
        while (timer.Elapsed.TotalSeconds < seconds)
        {
            if (token.IsCancellationRequested)
            {
                inventoryController.RaiseLoadMagazineEvent(stopFoldEvent);
                callback?.Fail("Folding was cancelled");
                yield break;
            }
            yield return null;
        }
        inventoryController.RaiseLoadMagazineEvent(stopFoldEvent);

        // Close the "open" grid window when item is folded
        if (itemContextAbstractClass == null)
        {
            Foldables.LogSource.LogWarning("ItemUiContextExtensions::FoldingAction Cannot close dependent windows, itemContextAbstractClass is null");
        }
        itemContextAbstractClass?.CloseDependentWindows();

        // If to fold but not empty, ask if player wants to spill container contents
        if (item.RequiresEmptyingBeforeFold())
            _ = HandleNonEmptyFoldingAsync(itemUiContext, item, callback, inventoryController);
        else
            itemUiContext.FoldItem(item, callback);
    }

    public static void StopFolding()
    {
        _foldingCts?.Cancel();
        _foldingCts?.Dispose();
        _foldingCts = null;
    }
}
