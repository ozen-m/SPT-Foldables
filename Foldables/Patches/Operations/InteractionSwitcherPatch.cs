using System.Reflection;
using Comfort.Common;
using EFT.InventoryLogic;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Additional checks for folding an item
/// </summary>
public class InteractionSwitcherPatch : ModulePatch
{
    protected static readonly FailedResult openContainerFail = new("Item is folded and can't unfold in place");
    protected static readonly FailedResult itemsInsideFail = new("Cannot fold the container with items inside");
    protected static readonly FailedResult foldItemFail = new("Cannot fold the item now");

    protected override MethodBase GetTargetMethod()
    {
        return typeof(ContextInteractionSwitcherClass).GetMethod(nameof(ContextInteractionSwitcherClass.IsInteractive));
    }

    [PatchPostfix]
    protected static void Postfix(ContextInteractionSwitcherClass __instance, EItemInfoButton button, ref IResult __result)
    {
        if (__instance.Item_0_1 is not IFoldable)
        {
            return;
        }

        if (__result.Failed && (button == EItemInfoButton.Fold || button == EItemInfoButton.Unfold))
        {
            __result = foldItemFail;
        }
        else if (button == EItemInfoButton.Open && __instance.Item_0_1.IsFoldableFolded())
        {
            if (__instance.IsInteractive(EItemInfoButton.Unfold).Failed)
            {
                __result = openContainerFail;
            }
        }
        else if (button == EItemInfoButton.Fold && !__instance.Item_0_1.IsEmptyNonLinq())
        {
            __result = itemsInsideFail;
        }
    }
}
