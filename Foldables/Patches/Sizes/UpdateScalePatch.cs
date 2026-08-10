using System.Reflection;
using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;

namespace Foldables.Patches.Sizes;

public class UpdateScalePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GridItemView).GetMethod(nameof(GridItemView.UpdateScale));
    }

    [PatchPostfix]
    protected static void Postfix(GridItemView __instance, Image ___MainImage)
    {
        if (!__instance.Item.IsFoldableFolded() || __instance is SlotItemView)
        {
            return;
        }

        // Thanks Tyfon!
        var itemViewSizeDelta = __instance.RectTransform.sizeDelta;
        var sizeDelta = ___MainImage.rectTransform.sizeDelta;
        var x = sizeDelta.x;
        var y = sizeDelta.y;

        // Calculate scale and multiply to preserve aspect ratio
        var scale = __instance.ItemRotation == ItemRotation.Horizontal
            ? Mathf.Min(itemViewSizeDelta.x / x, itemViewSizeDelta.y / y)
            : Mathf.Min(itemViewSizeDelta.y / x, itemViewSizeDelta.x / y);
        ___MainImage.rectTransform.sizeDelta = new Vector2(x * scale, y * scale);
    }
}
