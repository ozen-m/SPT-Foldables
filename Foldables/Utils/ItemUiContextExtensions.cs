using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using HarmonyLib;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace Foldables.Utils;

public static class ItemUiContextExtensions
{
    private static int _activeCount;
    private static CancellationTokenSource _cancellationTokenSource;
    private static readonly AccessTools.FieldRef<ItemUiContext, InventoryController> _inventoryControllerField = AccessTools.FieldRefAccess<ItemUiContext, InventoryController>("inventoryController_0");

    public static bool IsFolding => _activeCount > 0;

    /// <summary>
    /// Fold Item with Delay. Delays only in raid
    /// </summary>
    public static void FoldItemWithDelay(this ItemUiContext itemUiContext, Item item, Callback callback = null)
    {
        if (!GClass2340.InRaid || item is not IFoldable foldableItem || foldableItem.FoldingTime <= 0)
        {
            item.FoldItem();
            callback?.Succeed();
            return;
        }

        var inventoryController = _inventoryControllerField(itemUiContext);
        inventoryController.StopProcesses();
        if (inventoryController != null && inventoryController is Player.PlayerInventoryController playerInventoryController)
        {
            StopFolding();
            _cancellationTokenSource = new();
            playerInventoryController.Player_0.StartCoroutine(FoldingAction(item, foldableItem.FoldingTime, inventoryController, itemUiContext.ContextInteractionsSwitcher, _cancellationTokenSource.Token, callback));
        }
        return;
    }

    private static IEnumerator FoldingAction(Item item, float seconds, InventoryController inventoryController, ContextInteractionSwitcherClass interactionSwitcher, CancellationToken token, Callback callback = null)
    {
        _activeCount++;
        // Use LoadMagazine event for our UI
        IItemOwner owner = item.Owner;
        GEventArgs7 startFoldEvent = new(null, item, 1, seconds, CommandStatus.Begin, owner);
        GEventArgs7 stopFoldEvent = new(null, item, 1, seconds, CommandStatus.Succeed, owner);
        inventoryController.RaiseLoadMagazineEvent(startFoldEvent);

        Stopwatch timer = Stopwatch.StartNew();
        while (seconds > timer.Elapsed.TotalSeconds)
        {
            if (token.IsCancellationRequested)
            {
                inventoryController.RaiseLoadMagazineEvent(stopFoldEvent);
                callback?.Fail("Folding was cancelled.");
                _activeCount--;
                yield break;
            }
            yield return null;
        }

        inventoryController.RaiseLoadMagazineEvent(stopFoldEvent);
        interactionSwitcher.Item_0_1 = item;
        if (interactionSwitcher.IsInteractive(EItemInfoButton.Fold).Succeed) // Final check before folding
        {
            item.FoldItem();
            callback?.Succeed();
            _activeCount--;
            yield break;
        }
        callback?.Fail("Fold check failed.");
        _activeCount--;
    }

    public static void StopFolding()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}
