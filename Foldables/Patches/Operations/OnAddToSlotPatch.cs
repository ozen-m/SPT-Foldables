using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Unfold item on add to SearchableSlotView
/// </summary>
public class OnAddToSlotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SearchableSlotView).GetMethod(nameof(SearchableSlotView.OnAddToSlot));
    }

    [PatchPostfix]
    protected static void Postfix(SearchableSlotView __instance, Item item, GEventArgs2 args)
    {
        if (args.Status == CommandStatus.Succeed && item.IsFoldableFolded())
        {
            item.FoldItemWithDelay(__instance.ParentItemContext);
        }
    }
}
