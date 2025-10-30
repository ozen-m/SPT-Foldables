using EFT.UI;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;
using TMPro;

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// Add (Folded) to folded items inraid
/// </summary>
public class ActionsNamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ActionPanel).GetMethod(nameof(ActionPanel.method_0));
    }

    [PatchPostfix]
    protected static void Postfix(ActionsReturnClass interactionState, ref TextMeshProUGUI ____itemName)
    {
        if (interactionState != null && interactionState.GetIsFolded())
        {
            ____itemName.text += " (Folded)".Localized().ToUpper();
        }
    }
}
