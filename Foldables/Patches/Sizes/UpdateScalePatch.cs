using EFT.UI.DragAndDrop;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;
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
    protected static void Postfix(GridItemView __instance, RectTransform ___rectTransform_0, Image ___MainImage)
    {
        if (__instance.Item.IsFoldableFolded() && __instance is not SlotItemView)
        {
            // Thanks Tyfon!
            Vector2 itemViewSizeDelta = ___rectTransform_0.sizeDelta;
            Vector2 sizeDelta = ___MainImage.rectTransform.sizeDelta;
            float x = sizeDelta.x;
            float y = sizeDelta.y;

            // Calculate scale and multiply to preserve aspect ratio
            float scale = Mathf.Min(itemViewSizeDelta.x / x, itemViewSizeDelta.y / y);
            ___MainImage.rectTransform.sizeDelta = new Vector2(x * scale, y * scale);
        }
    }
}
