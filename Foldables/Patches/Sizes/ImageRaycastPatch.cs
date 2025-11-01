using EFT.UI.DragAndDrop;
using Foldables.Models;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine.UI;

namespace Foldables.Patches.Sizes;

/// <summary>
/// Images when resized can extend past the item's cellsize, disable raycast on them instead of messing with scaling
/// </summary>
public class ImageRaycastPatch : ModulePatch
{
    public static AccessTools.FieldRef<GridItemView, Image> mainImageField;

    protected override MethodBase GetTargetMethod()
    {
        mainImageField = AccessTools.FieldRefAccess<GridItemView, Image>("MainImage");

        return typeof(GridItemView).GetMethod(nameof(GridItemView.NewItemView));
    }

    [PatchPostfix]
    protected static void Postfix(GridItemView __instance)
    {
        if (__instance.Item is IFoldable foldableItem)
        {
            Image mainImage = mainImageField(__instance);
            mainImage.raycastTarget = false;
        }
    }
}
