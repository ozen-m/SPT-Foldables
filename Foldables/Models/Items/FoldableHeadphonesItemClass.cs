using EFT.InventoryLogic;
using Foldables.Models.Templates;
using JetBrains.Annotations;

namespace Foldables.Models.Items;

public class FoldableHeadphonesItemClass : HeadphonesItemClass, IFoldable
{
    [GAttribute26]
    [UsedImplicitly]
    public readonly FoldableComponent Foldable;

    private readonly FoldableHeadphonesTemplateClass _foldableHeadphonesTemplateClass;

    public FoldableHeadphonesItemClass(string id, FoldableHeadphonesTemplateClass template)
        : base(id, template)
    {
        _foldableHeadphonesTemplateClass = template;
        if (template.Foldable)
        {
            Foldable = new FoldableComponent(this, template);
            Components.Add(Foldable);
        }
    }

    public bool Folded => Foldable is { Folded: true };

    public int SizeReduceRight => Foldable.SizeReduceRight;

    public int SizeReduceDown => _foldableHeadphonesTemplateClass.SizeReduceDown;

    public float FoldingTime => _foldableHeadphonesTemplateClass.FoldingTime;

    public string FoldedSlot => _foldableHeadphonesTemplateClass.FoldedSlot;

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
