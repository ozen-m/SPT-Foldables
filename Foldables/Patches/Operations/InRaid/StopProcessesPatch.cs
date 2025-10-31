using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

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
        if (FoldItemPatch.IsFolding)
        {
            FoldItemPatch.IsCancelled = true;
        }
    }
}
