using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;

namespace Foldables.Utils;

public static class ItemHelper
{
    public static bool IsFoldableFolded(this Item item)
    {
        return item is IFoldable foldableItem && foldableItem.Folded;
    }

    public static void FoldItem(this Item item) => ItemUiContext.Instance.FoldItem(item);

    /// <summary>
    /// Fold Item with Delay. Delays only in raid
    /// </summary>
    public static void FoldItemWithDelay(this Item item, ItemContextAbstractClass itemContextAbstractClass = null, Callback callback = null) => ItemUiContext.Instance.FoldItemWithDelay(item, itemContextAbstractClass, callback);

    public static bool IsEmptyNonLinq(this Item item)
    {
        if (item is CompoundItem compoundItem)
        {
            foreach (StashGridClass grid in compoundItem.Grids)
            {
                if (grid.Gclass3120_0.Count > 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
