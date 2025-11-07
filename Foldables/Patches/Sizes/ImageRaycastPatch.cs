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
    private static readonly AccessTools.FieldRef<GridItemView, Image> _mainImageField = AccessTools.FieldRefAccess<GridItemView, Image>("MainImage");

    protected override MethodBase GetTargetMethod()
    {
        return typeof(GridItemView).GetMethod(nameof(GridItemView.NewItemView));
    }

    [PatchPostfix]
    protected static void Postfix(GridItemView __instance)
    {
        if (__instance.Item is IFoldable)
        {
            var mainImage = _mainImageField(__instance);
            mainImage.raycastTarget = false;
        }
    }
}
