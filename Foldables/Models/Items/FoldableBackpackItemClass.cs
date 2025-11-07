using EFT.InventoryLogic;
using Foldables.Models.Templates;

namespace Foldables.Models.Items;

public class FoldableBackpackItemClass : BackpackItemClass, IFoldable
{
    [GAttribute26]
    public readonly FoldableComponent Foldable;

    public FoldableBackpackTemplateClass FoldableBackpackTemplateClass { get; }

    public FoldableBackpackItemClass(string id, FoldableBackpackTemplateClass template)
        : base(id, template)
    {
        FoldableBackpackTemplateClass = template;
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

    public int SizeReduceDown => FoldableBackpackTemplateClass.SizeReduceDown;

    public float FoldingTime => FoldableBackpackTemplateClass.FoldingTime;

    public string FoldedSlot => FoldableBackpackTemplateClass.FoldedSlot;

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
