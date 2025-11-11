using System.Reflection;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Utils;
using SPT.Reflection.Patching;

#pragma warning disable VSTHRD003
#pragma warning disable VSTHRD100
// ReSharper disable AsyncVoidMethod

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// Force unfold headphones if folded
/// </summary>
public class InventoryScreenShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemsPanel).GetMethod(nameof(ItemsPanel.Show));
    }

    [PatchPostfix]
    protected static async void Postfix(InventoryController inventoryController, ItemsPanel.EItemsTab currentTab, bool inRaid, Task __result)
    {
        if (!inRaid || currentTab != ItemsPanel.EItemsTab.Gear) return;

        await __result;

        var headphoneSlot = inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Earpiece);
        if (headphoneSlot.ContainedItem.IsFoldableFolded())
        {
            headphoneSlot.ContainedItem.FoldItemWithDelay(force: true);
        }
    }
}
