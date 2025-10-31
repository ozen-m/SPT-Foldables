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
                rootItem.FoldItem();
                owner.Player.CurrentManagedState.Pickup(true, () =>
                {
                    owner.Player.UpdateInteractionCast();
                    if (owner.Player.CurrentState is PickupStateClass pickupStateClass)
                    {
                        pickupStateClass.Pickup(false, null);

                        // Set back to default value
                        pickupStateClass._timeForPikupAnimation = 0.6f;
                    }
                });
                if (owner.Player.CurrentState is PickupStateClass pickupStateClass)
                {
                    // Set delay for folding (TODO: add config)
                    // Max 5 seconds (hardcoded by BSG)
                    pickupStateClass._timeForPikupAnimation = 3f;
                }
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
            __result.SetIsFolded(true);
        }
    }
}

