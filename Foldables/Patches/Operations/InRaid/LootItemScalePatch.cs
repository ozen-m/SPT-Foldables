using EFT.Interactive;
using EFT.InventoryLogic;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace Foldables.Patches.Operations.InRaid;

public class LootItemScalePatch : ModulePatch
{
    private static readonly Vector3 newScale = new Vector3(0.66f, 0.66f, 0.66f);

    protected override MethodBase GetTargetMethod()
    {
        return typeof(LootItem).GetMethod(nameof(LootItem.Init));
    }

    [PatchPostfix]
    protected static void Postfix(Item item, ref LootItem __result)
    {
        if (item.IsFoldableFolded())
        {
            __result.gameObject.transform.localScale = newScale;
        }
    }
}
