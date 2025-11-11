using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Unfold headphones when dragged into a GridItemView
/// </summary>
public class OnItemAddedPatch : ModulePatch
{
    private static MongoID _headwearId;

    protected override MethodBase GetTargetMethod()
    {
        _headwearId = new MongoID("5a341c4086f77401f2541505");

        return typeof(GridItemView).GetMethod(nameof(GridItemView.OnItemAdded));
    }

    [PatchPostfix]
    protected static void Postfix(GridItemView __instance, GEventArgs2 eventArgs, ItemUiContext ___ItemUiContext)
    {
        if (eventArgs.Status != CommandStatus.Succeed ||
            __instance.Item != eventArgs.To.Container.ParentItem || // Only views the same as target
            __instance.Item.Template.ParentId != _headwearId || // Only headwear
            !eventArgs.Item.IsFoldableFolded() // Only folded
           ) return;

        ___ItemUiContext.FoldItemWithDelay(eventArgs.Item, __instance.ItemContext, null, true);
    }
}
