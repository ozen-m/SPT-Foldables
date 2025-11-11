using System.Reflection;
using EFT.InventoryLogic;
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
        return typeof(ContextInteractionsAbstractClass).GetMethod(nameof(ContextInteractionsAbstractClass.method_17));
    }

    // Must happen before
    [PatchPrefix]
    protected static bool Prefix(ContextInteractionsAbstractClass __instance)
    {
        if (!__instance.Item_0.IsFoldableFolded()) return true;

        __instance.ItemUiContext_1.FoldItemWithDelay(__instance.Item_0, __instance.ItemContextAbstractClass, (result) =>
        {
            if (result.Failed) return;

            __instance.Action_6();
            if (__instance.Item_0 is CompoundItem item)
            {
                __instance.ItemUiContext_1.OpenItem(item, __instance.ItemContextAbstractClass);
                // return;
            }
            // UnityEngine.Debug.LogError("Trying to open an item which is not a CompoundItem!");
        });
        return false;
    }
}
