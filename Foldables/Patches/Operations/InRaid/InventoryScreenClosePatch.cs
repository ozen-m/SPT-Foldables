using System.Reflection;
using EFT.UI;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// ContinuousLoadAmmo compatibility
/// </summary>
[IgnoreAutoPatch]
public class InventoryScreenClosePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(InventoryScreen).GetMethod(nameof(InventoryScreen.Close));
    }

    [PatchPostfix]
    protected static void Postfix()
    {
        if (ItemUiContextExtensions.IsFolding)
        {
            ItemUiContextExtensions.StopFolding();
        }
    }
}
