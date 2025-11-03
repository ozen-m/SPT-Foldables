using EFT.InventoryLogic;
using Foldables.Models;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace Foldables.Patches.Sizes;

public class ResizeHelperPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(InteractionsHandlerClass).GetMethod(nameof(InteractionsHandlerClass.Resize_Helper));
    }

    // So hacky I hate it, but bsg checking the item itself instead of the component...well
    [PatchPrefix]
    protected static bool Prefix(Item item, ItemAddress location, InteractionsHandlerClass.EResizeAction resizeAction, bool backwards, bool simulate, ref GStruct154<GClass3416> __result)
    {
        List<Item> list = simulate ? null : new();
        if (location is GClass3390)
        {
            __result = new GClass3416(item, location, resizeAction, list, default(GStruct424));
            return false;
        }
        Item item2 = resizeAction == InteractionsHandlerClass.EResizeAction.Fold || resizeAction == InteractionsHandlerClass.EResizeAction.Unfold ? item : location.Container.ParentItem;
        GInterface407 gInterface = default(GStruct424);
        while (item2 is CompoundItem compoundItem && (compoundItem is Weapon || compoundItem is Mod || compoundItem is IFoldable) && compoundItem.Parent is not GClass3390)
        {
            if (compoundItem.Parent is GClass3393)
            {
                XYCellSizeStruct xYCellSizeStruct = compoundItem.CalculateCellSize();
                XYCellSizeStruct xYCellSizeStruct2 = resizeAction switch
                {
                    InteractionsHandlerClass.EResizeAction.Unfold => compoundItem.GetSizeAfterFolding(location, item.GetItemComponent<FoldableComponent>(), folded: false),
                    InteractionsHandlerClass.EResizeAction.Fold => compoundItem.GetSizeAfterFolding(location, item.GetItemComponent<FoldableComponent>(), folded: true),
                    InteractionsHandlerClass.EResizeAction.Removal => compoundItem.GetSizeAfterDetachment(location, item),
                    InteractionsHandlerClass.EResizeAction.Addition => compoundItem.GetSizeAfterAttachment(location, item),
                    _ => compoundItem.CalculateCellSize(),
                };
                XYCellSizeStruct oldSize = backwards ? xYCellSizeStruct2 : xYCellSizeStruct;
                XYCellSizeStruct newSize = backwards ? xYCellSizeStruct : xYCellSizeStruct2;
                GStruct154<GInterface407> gStruct = InteractionsHandlerClass.smethod_21(compoundItem, oldSize, newSize, simulate);
                if (!gStruct.Succeeded)
                {
                    if (!simulate)
                    {
                        foreach (Item item3 in list)
                        {
                            InteractionsHandlerClass.smethod_21(item3, oldSize, item3.CalculateCellSize(), simulate: false);
                        }
                        gInterface.RollBack();
                    }
                    __result = new InteractionsHandlerClass.GClass1605(item, compoundItem, location, newSize);
                    return false;
                }
                if (gStruct.Value.IsRealResize)
                {
                    gInterface = gStruct.Value;
                }
                if (!simulate)
                {
                    list.Add(compoundItem);
                }
            }
            // Don't check parent item if IFoldable
            // Double check for side effects
            item2 = compoundItem is IFoldable ? null : compoundItem.Parent.Container.ParentItem;
        }
        __result = new GClass3416(item, location, resizeAction, list, gInterface);
        return false;
    }
}
