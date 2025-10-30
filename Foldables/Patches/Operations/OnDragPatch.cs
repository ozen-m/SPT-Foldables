using System.Reflection;
using EFT.UI;
using EFT.UI.DragAndDrop;
using Foldables.Models;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Foldables.Patches.Operations;

/// <summary>
/// Fold/unfold an item while dragging; Works but with bugs: When new item view is off screen, dragging stops
/// </summary>
public class OnDragPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(ItemView).GetMethod(nameof(ItemView.Update));
	}

	[PatchPrefix]
	protected static void Prefix(ItemView __instance, PointerEventData ___pointerEventData_0, ItemUiContext ___ItemUiContext)
	{
		if (__instance.BeingDragged && __instance.Item is IFoldable && Input.GetKey(KeyCode.F))
		{
			__instance.ExecuteMiddleClick(); // Fold/unfold
			RecreateDraggedItemView(__instance, ___ItemUiContext);
		}
	}

	protected static void RecreateDraggedItemView(ItemView itemView, ItemUiContext itemUiContext)
	{
		itemView.DraggedItemView.Kill();
		Object.DestroyImmediate(itemView.DraggedItemView.gameObject);

		itemView.DraggedItemView = DraggedItemView.Create(itemView.ItemContext, itemView.ItemRotation, itemView.Examined ? Color.white : new Color(0f, 0f, 0f, 0.85f), itemUiContext);
		((RectTransform)itemView.DraggedItemView.transform).position = itemView.transform.position;
		itemView.DraggedItemView.method_2(itemView.DraggedItemView.ItemContext.ItemRotation);
	}
}
