using System.Reflection;
using EFT;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// Stop folding item on stop processes
/// </summary>
public class StopProcessesPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player.PlayerInventoryController).GetMethod(nameof(Player.PlayerInventoryController.StopProcesses));
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
