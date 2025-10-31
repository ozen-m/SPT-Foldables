using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// ContinuousLoadAmmo compatibility
/// </summary>
public class InventoryScreenClosePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(InventoryScreen).GetMethod(nameof(InventoryScreen.Close));
    }

    [PatchPostfix]
    protected static void Postfix()
    {
        if (FoldItemPatch.IsFolding)
        {
            FoldItemPatch.IsCancelled = true;
        }
    }
}
