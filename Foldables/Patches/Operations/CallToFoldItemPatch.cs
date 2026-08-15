using System.Reflection;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models.Items;
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
        return typeof(BaseItemContextInteractions).GetMethod(nameof(BaseItemContextInteractions.method_32));
    }

    [PatchPrefix]
    protected static bool Prefix(BaseItemContextInteractions __instance)
    {
        if (__instance.Item is not IFoldable foldableItem)
        {
            return true;
        }

        if (MultiSelectInterop.Count > 1)
        {
            MultiSelectInterop.ApplyAll(
                (itemContext) =>
                {
                    var multiSelectItem = itemContext.Item;
                    if (multiSelectItem is not IFoldable)
                    {
                        return Task.CompletedTask;
                    }

                    var tcs = new TaskCompletionSource<IResult>();
                    ItemUiContext.Instance.FoldItemWithDelay(multiSelectItem, itemContext, tcs.SetResult);
                    return tcs.Task;
                },
                foldableItem.Folded ? EItemInfoButton.Unfold : EItemInfoButton.Fold,
                false,
                __instance.ItemUiContext
            );
        }
        else
        {
            __instance.ItemUiContext.FoldItemWithDelay(__instance.Item, __instance.ItemContext);
        }
        return false;
    }
}
