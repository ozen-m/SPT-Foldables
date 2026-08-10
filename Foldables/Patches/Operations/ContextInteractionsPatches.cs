using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Unfold on opening a folded item
/// </summary>
public class UnfoldOnOpenInteractionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BaseItemContextInteractions).GetMethod(nameof(BaseItemContextInteractions.method_17));
    }

    // Must happen before
    [PatchPrefix]
    protected static bool Prefix(BaseItemContextInteractions __instance)
    {
        if (!__instance.Item.IsFoldableFolded())
        {
            return true;
        }

        __instance.ItemUiContext.FoldItemWithDelay(__instance.Item, __instance.ItemContext, (result) =>
        {
            if (result.Failed)
            {
                return;
            }

            __instance._onCloseAction();
            if (__instance.Item is CompoundItem item)
            {
                __instance.ItemUiContext.OpenItem(item, __instance.ItemContext);
                // return;
            }
            // UnityEngine.Debug.LogError("Trying to open an item which is not a CompoundItem!");
        });
        return false;
    }
}
