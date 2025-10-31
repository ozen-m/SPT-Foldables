using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using Foldables.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Foldables.Patches.Operations.InRaid;

public class FoldItemPatch : ModulePatch
{
    public static bool IsFolding = false;
    public static bool IsCancelled = false;
    protected static Stopwatch timer = new();
    protected static AccessTools.FieldRef<ItemUiContext, InventoryController> inventoryControllerField;

    protected override MethodBase GetTargetMethod()
    {
        inventoryControllerField = AccessTools.FieldRefAccess<ItemUiContext, InventoryController>("inventoryController_0");

        // todo: change method to ItemUiContext.FoldItem?
        return typeof(ContextInteractionsAbstractClass).GetMethod(nameof(ContextInteractionsAbstractClass.method_32));
    }

    [PatchPrefix]
    protected static bool Prefix(ContextInteractionsAbstractClass __instance)
    {
        if (!GClass2340.InRaid)
        {
            return true;
        }

        if (__instance.Item_0 is IFoldable)
        {
            if (IsFolding)
            {
                return false;
            }

            var inventoryController = inventoryControllerField(__instance.ItemUiContext_1);
            inventoryController.StopProcesses();
            if (inventoryController is Player.PlayerInventoryController playerInventoryController)
            {
                playerInventoryController.Player_0.StartCoroutine(FoldingAction(__instance.Item_0, 3f, inventoryController));
            }
            return false;
        }
        return true;
    }

    protected static IEnumerator FoldingAction(Item item, float seconds, InventoryController inventoryController)
    {
        IsFolding = true;
        IItemOwner owner = item.Owner;
        GEventArgs7 startFoldEvent = new(null, item, 1, seconds, CommandStatus.Begin, owner);
        GEventArgs7 stopFoldEvent = new(null, item, 1, seconds, CommandStatus.Succeed, owner);
        inventoryController.RaiseLoadMagazineEvent(startFoldEvent);
        // Use LoadMagazine event for our UI

        timer.Restart();
        while (seconds > timer.Elapsed.TotalSeconds)
        {
            if (IsCancelled)
            {
                Stop(inventoryController, stopFoldEvent);
                yield break;
            }
            yield return null;
        }
        Stop(inventoryController, stopFoldEvent);
        item.FoldItem();
    }

    protected static void Stop(InventoryController inventoryController, GEventArgs7 stopFoldEvent)
    {
        inventoryController.RaiseLoadMagazineEvent(stopFoldEvent);
        IsCancelled = false;
        IsFolding = false;
    }
}
