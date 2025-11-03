using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;

namespace Foldables.Patches.Operations;

/// <summary>
/// Replace call to ItemUiContext.FoldItem with ours with delay
/// </summary>
public class CallToFoldItemPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ContextInteractionsAbstractClass).GetMethod(nameof(ContextInteractionsAbstractClass.method_32));
    }

    [PatchPrefix]
    protected static bool Prefix(ContextInteractionsAbstractClass __instance)
    {
        if (__instance.Item_0 is IFoldable foldableItem)
        {
            _ = FoldItemAction(__instance.ItemUiContext_1, __instance.Item_0, foldableItem, __instance.ItemContextAbstractClass);
            return false;
        }
        return true;
    }

    // Naming is hard
    protected static async Task FoldItemAction(ItemUiContext itemUiContext, Item item, IFoldable foldableItem, ItemContextAbstractClass itemContextAbstractClass)
    {
        Callback callback = null;

        // If to fold but not empty, ask if want to spill container contents 
        if (!foldableItem.Folded && !item.IsEmptyNonLinq())
        {
            var toSpillItems = await itemUiContext.ShowSpillAndFoldDialog(item);
            if (toSpillItems)
            {
                callback = (result) =>
                {
                    if (result.Succeed)
                    {
                        item.TryMoveContainedItemsToParent(ItemUiContextExtensions.InventoryControllerField(itemUiContext), false);
                    }
                };
            }
            else
            {
                NotificationManagerClass.DisplayWarningNotification("Cannot fold the container with items inside".Localized());
                return;
            }
        }

        itemUiContext.FoldItemWithDelay(item, itemContextAbstractClass, callback);
    }
}
