using System.Reflection;
using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace Foldables.Patches;

[Injectable]
public class GetSizePatch : AbstractPatch
{
    private static readonly MongoId[] _foldableItems = [BaseClasses.BACKPACK, BaseClasses.VEST, BaseClasses.HEADPHONES];
    private static ItemHelper _itemHelper;

    public GetSizePatch(ItemHelper itemHelper)
    {
        _itemHelper = itemHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InventoryHelper), "GetSizeByInventoryItemHash");
    }

    [PatchPostfix]
    protected static void Postfix(MongoId itemTpl, MongoId itemId, InventoryItemHash inventoryItemHash, ref (int x, int y) __result)
    {
        // Only foldables
        if (!_itemHelper.IsOfBaseclasses(itemTpl, _foldableItems))
        {
            return;
        }

        // Is the item a valid item
        var (isValidItem, itemTemplate) = _itemHelper.GetItem(itemTpl);
        if (!isValidItem || itemTemplate?.Properties is null)
        {
            return;
        }

        // Does item support being folded
        if (!itemTemplate.Properties.Foldable.GetValueOrDefault(false))
        {
            return;
        }

        // Is the item in the inventory hash
        if (!inventoryItemHash.ByItemId.TryGetValue(itemId, out var rootItem))
        {
            return;
        }

        // Is the item actively folded
        if (!(rootItem.Upd?.Foldable?.Folded.GetValueOrDefault(false) ?? false))
        {
            return;
        }

        // Does the item have size reduce down
        if (!itemTemplate.Properties.ExtensionData!.TryGetValue("SizeReduceDown", out var obj))
        {
            return;
        }

        // Item can be collapsed and has been collapsed
        __result = (__result.x, __result.y - (obj is int value ? value : 0));
    }
}
