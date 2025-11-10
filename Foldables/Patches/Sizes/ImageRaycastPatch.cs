using System.Reflection;
using EFT.UI.DragAndDrop;
using Foldables.Models;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine.UI;

namespace Foldables.Patches.Sizes;

/// <summary>
/// Images when resized can extend past the item's cellsize, disable raycast on them instead of messing with scaling
/// </summary>
public class ImageRaycastPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GridItemView).GetMethod(nameof(GridItemView.NewItemView));
    }

    [PatchPostfix]
    protected static void Postfix(GridItemView __instance, Image ___MainImage)
    {
        if (__instance.Item is IFoldable)
        {
            ___MainImage.raycastTarget = false;
        }
    }
}
