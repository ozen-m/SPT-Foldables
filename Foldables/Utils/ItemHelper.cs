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
    public static void FoldItemWithDelay(this Item item, Callback callback = null) => ItemUiContext.Instance.FoldItemWithDelay(item, callback);

    public static void ForceFold(this Item item, bool? toFolded = null, Callback callback = null)
    {
        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.TacticalClothingApply);
        FoldableComponent foldableComponent = item.GetItemComponent<FoldableComponent>();
        GStruct154<GClass3428> foldResult = InteractionsHandlerClass.Fold(foldableComponent, toFolded ?? (!foldableComponent.Folded), false);
        if (foldableComponent.Item.Owner is TraderControllerClass traderController)
        {
            traderController.TryRunNetworkTransaction(foldResult, callback);
        }
    }

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
