using EFT.InventoryLogic;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Foldables.Patches.Operations;

public class ContextInteractionsPatches
{
    public void Enable()
    {
        new CloseDependentWindowsPatch().Enable();
        new UnfoldOnOpenInteractionPatch().Enable();
    }
}

/// <summary>
/// Close the "open" grid window when folding an item
/// </summary>
public class CloseDependentWindowsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ContextInteractionsAbstractClass).GetMethod(nameof(ContextInteractionsAbstractClass.method_32));
    }

    [PatchPostfix]
    protected static void Postfix(ContextInteractionsAbstractClass __instance)
    {
        if (__instance.Item_0 is IFoldable foldableItem && foldableItem.Folded)
        {
            __instance.ItemContextAbstractClass.CloseDependentWindows();
        }
    }
}

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
        if (__instance.Item_0.IsFoldableFolded())
        {
            __instance.Item_0.FoldItemWithDelay((result) =>
            {
                if (result.Succeed)
                {
                    __instance.Action_6();
                    if (__instance.Item_0 is not CompoundItem item)
                    {
                        UnityEngine.Debug.LogError("Trying to open an item which is not a CompoundItem!");
                    }
                    else
                    {
                        __instance.ItemUiContext_1.OpenItem(item, __instance.ItemContextAbstractClass);
                    }
                }
            });
            return false;
        }
        return true;
    }
}
