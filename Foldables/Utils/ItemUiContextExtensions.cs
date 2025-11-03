using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using HarmonyLib;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace Foldables.Utils;

public static class ItemUiContextExtensions
{
    private static int _activeCount;
    private static CancellationTokenSource _cancellationTokenSource;
    private static readonly AccessTools.FieldRef<ItemUiContext, InventoryController> _inventoryControllerField = AccessTools.FieldRefAccess<ItemUiContext, InventoryController>("inventoryController_0");
    private static readonly AccessTools.FieldRef<ItemUiContext, TraderControllerClass> _traderControllerClassField = AccessTools.FieldRefAccess<ItemUiContext, TraderControllerClass>("traderControllerClass");

    public static bool IsFolding => _activeCount > 0;

    /// <summary>
    /// <seealso cref="ItemUiContext.FoldItem"/> with callback
    /// </summary>
    public static void FoldItem(this ItemUiContext itemUiContext, Item item, Callback callback)
    {
        if (!InteractionsHandlerClass.CanFold(item, out FoldableComponent foldableComponent))
        {
            return;
        }
        Singleton<GUISounds>.Instance.PlayUISound(item is IFoldable ? EUISoundType.TacticalClothingApply : EUISoundType.MenuStock);
        GStruct154<GClass3428> foldEvent = InteractionsHandlerClass.Fold(foldableComponent, !foldableComponent.Folded, true);
        var traderControllerClass = _traderControllerClassField(itemUiContext);
        traderControllerClass.TryRunNetworkTransaction(foldEvent, callback);
    }

    /// <summary>
    /// Fold Item with Delay. Delays only in raid
    /// </summary>
    public static void FoldItemWithDelay(this ItemUiContext itemUiContext, Item item, ItemContextAbstractClass itemContextAbstractClass = null, Callback callback = null)
    {
        if (!GClass2340.InRaid || item is not IFoldable foldableItem || foldableItem.FoldingTime <= 0)
        {
            // Close the "open" grid window when item is folded
            itemContextAbstractClass?.CloseDependentWindows();

            itemUiContext.FoldItem(item, callback);
            return;
        }

        var inventoryController = _inventoryControllerField(itemUiContext);
        if (inventoryController is Player.PlayerInventoryController playerInventoryController)
        {
            inventoryController.StopProcesses();
            StopFolding();
            _cancellationTokenSource = new();
            playerInventoryController.Player_0.StartCoroutine(FoldingAction(item, foldableItem.FoldingTime, inventoryController, itemUiContext, itemContextAbstractClass, _cancellationTokenSource.Token, callback));
        }
    }

    private static IEnumerator FoldingAction(Item item, float seconds, InventoryController inventoryController, ItemUiContext itemUiContext, ItemContextAbstractClass itemContextAbstractClass, CancellationToken token, Callback callback = null)
    {
        _activeCount++;

        // Use LoadMagazineEvent for our UI
        IItemOwner owner = item.Owner;
        GEventArgs7 startFoldEvent = new(null, item, 1, seconds, CommandStatus.Begin, owner);
        GEventArgs7 stopFoldEvent = new(null, item, 1, seconds, CommandStatus.Succeed, owner);
        inventoryController.RaiseLoadMagazineEvent(startFoldEvent);

        Stopwatch timer = Stopwatch.StartNew();
        while (seconds > timer.Elapsed.TotalSeconds)
        {
            if (token.IsCancellationRequested)
            {
                _activeCount--;
                inventoryController.RaiseLoadMagazineEvent(stopFoldEvent);
                callback?.Fail("Folding was cancelled");
                yield break;
            }
            yield return null;
        }
        _activeCount--;
        inventoryController.RaiseLoadMagazineEvent(stopFoldEvent);

        // Final check before folding
        var interactionsSwitcher = itemUiContext.ContextInteractionsSwitcher;
        interactionsSwitcher.Item_0_1 = item;
        var foldCheck = interactionsSwitcher.IsInteractive(EItemInfoButton.Fold);
        if (foldCheck.Succeed)
        {
            // Close the "open" grid window when item is folded
            if (itemContextAbstractClass == null)
            {
                Foldables.LogSource.LogWarning("Called for delayed folding but itemContextAbstractClass is null");
            }
            itemContextAbstractClass?.CloseDependentWindows();

            itemUiContext.FoldItem(item, callback);
        }
        else
        {
            NotificationManagerClass.DisplayWarningNotification(foldCheck.Error.Localized());
            callback?.Fail(foldCheck.Error);
        }
    }

    public static void StopFolding()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}
