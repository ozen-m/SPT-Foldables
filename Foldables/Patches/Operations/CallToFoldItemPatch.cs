using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Threading.Tasks;
using UIFixesInterop;

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
            if (MultiSelect.Count > 1)
            {
                MultiSelect.ApplyAll(
                    (itemContext) =>
                    {
                        var multiSelectItem = itemContext.Item;
                        if (multiSelectItem is IFoldable multiSelectFoldable)
                        {
                            var tcs = new TaskCompletionClass();
                            FoldItemInteraction(__instance.ItemUiContext_1, multiSelectItem, multiSelectFoldable, itemContext, (_) =>
                            {
                                tcs.Complete();
                            });
                            return tcs.Task;
                        }
                        return Task.CompletedTask;
                    },
                    foldableItem.Folded ? EItemInfoButton.Unfold : EItemInfoButton.Fold,
                    false,
                    __instance.ItemUiContext_1
                    );
            }
            else
            {
                FoldItemInteraction(__instance.ItemUiContext_1, __instance.Item_0, foldableItem, __instance.ItemContextAbstractClass);
            }
            return false;
        }
        return true;
    }

    protected static void FoldItemInteraction(ItemUiContext itemUiContext, Item item, IFoldable foldableItem, ItemContextAbstractClass itemContextAbstractClass, Callback callback = null)
    {
        // If to fold but not empty, ask if want to spill container contents 
        if (!foldableItem.Folded && !item.IsEmptyNonLinq())
        {
            _ = HandleNonEmptyFolding(itemUiContext, item, itemContextAbstractClass, callback);
        }
        else
        {
            itemUiContext.FoldItemWithDelay(item, itemContextAbstractClass, callback);
        }
    }

    protected static async Task HandleNonEmptyFolding(ItemUiContext itemUiContext, Item item, ItemContextAbstractClass itemContextAbstractClass, Callback callback = null)
    {
        var toSpillContents = await itemUiContext.ShowSpillAndFoldDialog(item);
        if (toSpillContents)
        {
            callback += (result) =>
            {
                if (result.Succeed)
                {
                    item.TryMoveContainedItemsToParent(ItemUiContextExtensions.InventoryControllerField(itemUiContext), false);
                }
            };
            itemUiContext.FoldItemWithDelay(item, itemContextAbstractClass, callback);
        }
        else
        {
            NotificationManagerClass.DisplayWarningNotification("Cannot fold the container with items inside".Localized());
            callback?.Fail("Cannot fold the container with items inside");
        }
    }
}
