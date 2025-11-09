using EFT.InventoryLogic;
using Foldables.Models.Templates;
using JetBrains.Annotations;

namespace Foldables.Models.Items;

public class FoldableVestItemClass : VestItemClass, IFoldable
{
    [GAttribute26]
    [UsedImplicitly]
    public readonly FoldableComponent Foldable;

    private readonly FoldableVestTemplateClass _foldableVestTemplateClass;

    public FoldableVestItemClass(string id, FoldableVestTemplateClass template)
        : base(id, template)
    {
        _foldableVestTemplateClass = template;
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

    public int SizeReduceDown => _foldableVestTemplateClass.SizeReduceDown;

    public float FoldingTime => _foldableVestTemplateClass.FoldingTime;

    public string FoldedSlot => _foldableVestTemplateClass.FoldedSlot;

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
