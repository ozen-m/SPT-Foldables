using EFT.UI.DragAndDrop;
using Foldables.Models.Items;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace Foldables.Patches.Mappings;

/// <summary>
/// Add custom foldable classes to ItemType
/// </summary>
public class GetItemTypePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemViewFactory).GetMethod(nameof(ItemViewFactory.GetItemType));
    }

    [PatchPrefix]
    protected static bool Prefix(Type itemType, ref EItemType __result)
    {
        if (typeof(FoldableBackpackItemClass).IsAssignableFrom(itemType))
        {
            __result = EItemType.Backpack;
            return false;
        }
        if (typeof(FoldableVestItemClass).IsAssignableFrom(itemType))
        {
            __result = EItemType.Equipment;
            return false;
        }
        return true;
    }
}

