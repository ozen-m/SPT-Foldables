using System.Collections.Generic;
using System.Reflection;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using Foldables.Models.Items;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Sizes;

// TODO: Maybe change to transpiler
public class ResizeHelperPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemManipulator).GetMethod(nameof(ItemManipulator.Resize_Helper));
    }

    [PatchPrefix]
    protected static bool Prefix(Item item, ItemAddress location, ItemManipulator.EResizeAction resizeAction, bool backwards, bool simulate, ref OperationResult<ResizeResult> __result)
    {
	    var list = simulate ? null : new List<Item>();
		if (location is OwnerItself)
		{
			__result = new ResizeResult(item, location, resizeAction, list, default(NoContainerResizeResult));
			return false;
		}
		var item2 = resizeAction is ItemManipulator.EResizeAction.Fold or ItemManipulator.EResizeAction.Unfold ? item : location.Container.ParentItem;
		IContainerResizeResult containerResizeResult = default(NoContainerResizeResult);
		while (item2 is CompoundItem compoundItem and (Weapon or Mod or IFoldable /* Insert IFoldable */) && compoundItem.Parent is not OwnerItself)
		{
			if (compoundItem.Parent is GridItemAddress)
			{
				var intVec = compoundItem.CalculateCellSize();
				var intVec2 = resizeAction switch
				{
					ItemManipulator.EResizeAction.Unfold => compoundItem.GetSizeAfterFolding(location, item.GetItemComponent<FoldableComponent>(), folded: false),
					ItemManipulator.EResizeAction.Fold => compoundItem.GetSizeAfterFolding(location, item.GetItemComponent<FoldableComponent>(), folded: true),
					ItemManipulator.EResizeAction.Removal => compoundItem.GetSizeAfterDetachment(location, item),
					ItemManipulator.EResizeAction.Addition => compoundItem.GetSizeAfterAttachment(location, item),
					_ => compoundItem.CalculateCellSize(),
				};
				var oldSize = backwards ? intVec2 : intVec;
				var newSize = backwards ? intVec : intVec2;
				var operationResult = ItemManipulator.Resize(compoundItem, oldSize, newSize, simulate);
				if (!operationResult.Succeeded)
				{
					if (!simulate)
					{
						foreach (var item3 in list)
						{
							ItemManipulator.Resize(item3, oldSize, item3.CalculateCellSize(), simulate: false);
						}
						containerResizeResult.RollBack();
					}
					__result = new ItemManipulator.ResizeError(item, compoundItem, location, newSize);
					return false;
				}
				if (operationResult.Value.IsRealResize)
				{
					containerResizeResult = operationResult.Value;
				}
				if (!simulate)
				{
					list.Add(compoundItem);
				}
			}
			// Don't check parent item if IFoldable
			// Double check for side effects
			item2 = compoundItem is not IFoldable ? compoundItem.Parent.Container.ParentItem : null;
		}
		__result = new ResizeResult(item, location, resizeAction, list, containerResizeResult);
		return false;
    }
}
