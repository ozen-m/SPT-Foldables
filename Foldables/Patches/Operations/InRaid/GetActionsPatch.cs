using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
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
                if (owner.Player.CurrentState is not IdleStateClass)
                {
                    NotificationManagerClass.DisplayWarningNotification("Cannot fold item while moving".Localized());
                    return;
                }

                var foldableComponent = rootItem.GetItemComponent<FoldableComponent>();
                GStruct154<GClass3428> foldingResult = InteractionsHandlerClass.Fold(foldableComponent, !foldableComponent.Folded, false);

                owner.Player.CurrentManagedState.Plant(true, false, foldableItem.FoldingTime, (successful) =>
                {
                    // Might appear that the operation failed (due to delay in callback) so do not simulate
                    if (successful)
                    {
                        controller.TryRunNetworkTransaction(foldingResult, (_) =>
                        {
                            // "Take" action missing unless forced to update interactions
                            owner.InteractionsChangedHandler();
                        });
                        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.TacticalClothingApply);
                    }
                    else
                    {
                        foldingResult.Value.RollBack();
                    }
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
            __result.SetIsFolded(true);
        }
    }
}
