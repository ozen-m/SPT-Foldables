using System.Reflection;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Replace call to ItemUiContext.FoldItem with ours with delay. Also handle MultiSelect
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
        if (__instance.Item_0 is not IFoldable foldableItem) return true;

        if (MultiSelectInterop.Count > 1)
        {
            MultiSelectInterop.ApplyAll(
                (itemContext) =>
                {
                    var multiSelectItem = itemContext.Item;
                    if (multiSelectItem is not IFoldable) return Task.CompletedTask;

                    var tcs = new TaskCompletionClass();
                    __instance.ItemUiContext_1.FoldItemWithDelay(
                        multiSelectItem,
                        itemContext,
                        (_) => { tcs.Complete(); }
                    );
                    return tcs.Task;
                },
                foldableItem.Folded ? EItemInfoButton.Unfold : EItemInfoButton.Fold,
                false,
                __instance.ItemUiContext_1
            );
        }
        else
        {
            __instance.ItemUiContext_1.FoldItemWithDelay(__instance.Item_0, __instance.ItemContextAbstractClass);
        }
        return false;
    }
}
