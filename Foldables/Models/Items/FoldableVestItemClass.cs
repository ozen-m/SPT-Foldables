using EFT.InventoryLogic;
using Foldables.Models.Templates;

namespace Foldables.Models.Items;

public class FoldableVestItemClass : VestItemClass, IFoldable
{
    [GAttribute26]
    public readonly FoldableComponent Foldable;

    public FoldableVestTemplateClass FoldableVestTemplateClass { get; }

    public FoldableVestItemClass(string id, FoldableVestTemplateClass template)
        : base(id, template)
    {
        FoldableVestTemplateClass = template;
        if (template.Foldable)
        {
            Foldable = new FoldableComponent(this, template);
            Components.Add(Foldable);
        }
    }

    public override GStruct153 Apply(TraderControllerClass itemController, Item item, int count, bool simulate)
    {
        if (Folded)
        {
            return new FoldedInsertError(item);
        }
        return base.Apply(itemController, item, count, simulate);
    }

    public bool Folded => Foldable is { Folded: true };

    public int SizeReduceRight => Foldable.SizeReduceRight;

    public int SizeReduceDown => FoldableVestTemplateClass.SizeReduceDown;

    public float FoldingTime => FoldableVestTemplateClass.FoldingTime;

    public string FoldedSlot => FoldableVestTemplateClass.FoldedSlot;

    public override int GetHashSum()
    {
        int hashSum = base.GetHashSum();
        if (Foldable != null)
        {
            hashSum = hashSum * 27 + Foldable.Folded.GetHashCode();
        }
        return hashSum;
    }
}
