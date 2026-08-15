using System.Reflection;
using EFT;
using EFT.UI;
using Foldables.Utils;
using SPT.Reflection.Patching;
using TMPro;

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// Add (Folded) to folded items in-raid
/// </summary>
public class ActionsNamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ActionPanel).GetMethod(nameof(ActionPanel.AvailableInteractionStateChangedHandler));
    }

    [PatchPostfix]
    protected static void Postfix(ActionPanel __instance, AvailableInteractionState interactionState)
    {
        if (interactionState != null && interactionState.GetIsFolded())
        {
            __instance._itemName.text += " (Folded)".Localized().ToUpper();
        }
    }
}
