using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

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
