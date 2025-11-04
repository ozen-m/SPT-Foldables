using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Foldables.Patches.Operations;

public class SearchableViewPatches
{
    public void Enable()
    {
        new OnAddToSlotPatch().Enable();
        new OnRemoveFromSlotPatch().Enable();
        new SearchableItemViewShowPatch().Enable();
    }
}

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

/// <summary>
/// Prevent from opening the grid when item is folded in SearchableSlotView
/// </summary>
public class SearchableItemViewShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(SearchableItemView).GetMethod(nameof(SearchableItemView.method_0));
    }

    [PatchPrefix]
    protected static bool Prefix(CompoundItem ___compoundItem_0)
    {
        if (___compoundItem_0.IsFoldableFolded())
        {
            return false;
        }
        return true;
    }
}
