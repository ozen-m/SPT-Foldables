using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using Foldables.Utils;
using SPT.Reflection.Patching;

namespace Foldables.Patches.Operations;

public class EquipBuildPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EquipmentBuildsScreen).GetMethod(nameof(EquipmentBuildsScreen.ApplyBuild));
    }

    [PatchPostfix]
    protected static void Postfix(BackEndInventoryController ____realInventoryController)
    {
        foreach (var slots in ____realInventoryController.Inventory.Equipment._cachedSlots)
        {
            var item = slots.ContainedItem;
            if (item.IsFoldableFolded())
            {
                item.FoldItem();
            }
        }
    }
}

public class SwapEquipmentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EquipmentBuildsScreen).GetMethod(nameof(EquipmentBuildsScreen.SwapEquipment));
    }

    [PatchPostfix]
    protected static void Postfix(BackEndInventoryController ____realInventoryController)
    {
        foreach (var slots in ____realInventoryController.Inventory.Equipment._cachedSlots)
        {
            var item = slots.ContainedItem;
            if (item.IsFoldableFolded())
            {
                item.FoldItem();
            }
        }
    }
}
