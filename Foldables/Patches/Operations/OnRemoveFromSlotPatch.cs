using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Reopen grid when unfolding in SearchableSlotView
/// </summary>
public class OnRemoveFromSlotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SearchableSlotView).GetMethod(nameof(SearchableSlotView.OnRemoveFromSlot));
    }

    [PatchPostfix]
    protected static void Postfix(SearchableSlotView __instance, Item item, GEventArgs3 args)
    {
        if (args.Status == CommandStatus.Failed && !item.IsFoldableFolded())
        {
            __instance.DragCancelled();
        }
    }
}
