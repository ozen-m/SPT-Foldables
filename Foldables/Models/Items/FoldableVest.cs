using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using Foldables.Models.Templates;
using JetBrains.Annotations;

namespace Foldables.Models.Items;

public class FoldableVest : Vest, IFoldable
{
    [Component]
    [UsedImplicitly]
    public readonly ExtendedFoldableComponent Foldable;

    public bool Folded => Foldable is {Folded: true};
    public int SizeReduceRight => GetTemplate<FoldableVestTemplate>().SizeReduceRight;
    public int SizeReduceDown => GetTemplate<FoldableVestTemplate>().SizeReduceDown;
    public float FoldingTime => GetTemplate<FoldableVestTemplate>().FoldingTime;

    public FoldableVest(string id, FoldableVestTemplate template) : base(id, template)
    {
        if (template.Foldable)
        {
            Foldable = new ExtendedFoldableComponent(this, template);
            Components.Add(Foldable);
        }
    }

    public override OperationResult Apply(ItemController itemController, Item item, int count, bool simulate)
    {
        if (Folded)
        {
            return new FoldedInsertError(item);
        }
        return base.Apply(itemController, item, count, simulate);
    }

    public override int GetHashSum()
    {
        var hashSum = base.GetHashSum();
        if (Foldable != null)
        {
            hashSum = hashSum * 27 + Foldable.Folded.GetHashCode();
        }
        return hashSum;
    }
}
