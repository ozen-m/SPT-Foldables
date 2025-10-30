using EFT;
using EFT.InventoryLogic;
using Foldables.Models;
using Foldables.Utils;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// Add actions for foldable items in raid
/// </summary>
public class GetActionsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GetActionsClass).GetMethod(nameof(GetActionsClass.smethod_9));
    }

    [PatchPostfix]
    protected static void Postfix(GamePlayerOwner owner, Item rootItem, string lootItemName, ref ActionsReturnClass __result)
    {
        if (rootItem is not IFoldable foldableItem)
        {
            return;
        }

        InventoryController controller = owner.Player.InventoryController;
        bool isExamined = controller.Examined(rootItem);

        __result.Actions.Add(new ActionsTypesClass
        {
            Name = (foldableItem.Folded ? "Unfold" : "Fold"),
            TargetName = (isExamined ? lootItemName : "Unknown item".Localized()),
            Action = () =>
            {
                owner.Player.CurrentManagedState.Pickup(true, () =>
                {
                    rootItem.ForceFold(null, (result) =>
                    {
                        if (result.Succeed)
                        {
                            owner.Player.UpdateInteractionCast();
                        }
                        if (owner.Player.CurrentState is PickupStateClass pickupStateClass)
                        {
                            pickupStateClass.Pickup(false, null);
                        }
                    });
                });
            },
            Disabled = (!foldableItem.Folded && !rootItem.IsEmptyNonLinq())
        });

        if (foldableItem.Folded)
        {
            foreach (ActionsTypesClass action in __result.Actions)
            {
                if (action.Name == "Search")
                {
                    action.Disabled = true;
                }
            }
            __result.SetIsFolded(value: true);
        }
    }
}

