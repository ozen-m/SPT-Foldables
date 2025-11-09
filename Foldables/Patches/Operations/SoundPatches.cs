using System;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

/// <summary>
/// UI sound for folding
/// </summary>
[Obsolete($"Bypassed by {nameof(ItemHelper.FoldItem)} with callback")]
public class SoundPatches
{
    private static bool _inPatch;

    public void Enable()
    {
        new FoldItemPatch().Enable();
        new PlayUISoundPatch().Enable();
    }

    [IgnoreAutoPatch]
    public class FoldItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ItemUiContext).GetMethod(nameof(ItemUiContext.FoldItem));
        }

        [PatchPrefix]
        protected static void Prefix(Item item)
        {
            if (item is IFoldable)
            {
                _inPatch = true;
            }
        }

        [PatchPostfix]
        protected static void Postfix()
        {
            _inPatch = false;
        }
    }

    [IgnoreAutoPatch]
    public class PlayUISoundPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GUISounds).GetMethod(nameof(GUISounds.PlayUISound));
        }

        [PatchPrefix]
        protected static void Prefix(ref EUISoundType soundType)
        {
            if (_inPatch && soundType == EUISoundType.MenuStock)
            {
                soundType = EUISoundType.TacticalClothingApply;
            }
        }
    }
}
