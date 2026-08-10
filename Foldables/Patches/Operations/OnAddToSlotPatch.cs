using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Unfold item on add to SlotViews
/// </summary>
public class OnAddToSlotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SlotView).GetMethod(nameof(SlotView.OnAddToSlot));
    }

    [PatchPostfix]
    protected static void Postfix(SlotView __instance, Item item, AddItemEventArgs args, ItemUiContext ___ItemUiContext)
    {
        if (args.Status != CommandStatus.Succeed
            || !item.IsFoldableFolded()
            || __instance is ModSlotView /*ModSlotView conflicts with OnItemAddedPatch*/
           )
            return;

        if (item is Headphones)
        {
            ___ItemUiContext.FoldItemWithDelay(item, __instance.ParentItemContext, null, true);
            return;
        }
        ___ItemUiContext.FoldItemWithDelay(item, __instance.ParentItemContext);
    }
}
