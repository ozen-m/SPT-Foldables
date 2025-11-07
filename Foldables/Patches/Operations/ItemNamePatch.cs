using System.Reflection;
using EFT.InventoryLogic;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// Add (Folded) to name
/// </summary>
public class ItemNamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemFactoryClass).GetMethod(nameof(ItemFactoryClass.BriefItemName));
    }

    [PatchPostfix]
    protected static void Postfix(Item item, ref string __result)
    {
        if (item.IsFoldableFolded())
        {
            __result += " (Folded)".Localized();
        }
    }
}
