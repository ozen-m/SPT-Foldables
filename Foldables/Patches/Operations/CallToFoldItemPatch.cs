using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;

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
        __instance.ItemUiContext_1.FoldItemWithDelay(__instance.Item_0, __instance.ItemContextAbstractClass);
        return false;
    }
}
