using EFT;
using EFT.InventoryLogic;
using Foldables.Models.Templates;
using JetBrains.Annotations;

namespace Foldables.Models.Items;

public class FoldableHeadphones : Headphones, IFoldable
{
    [Component]
    [UsedImplicitly]
    public readonly FoldableComponent Foldable;

    private readonly FoldableHeadphonesTemplate _foldableHeadphonesTemplate;

    public FoldableHeadphones(string id, FoldableHeadphonesTemplate template)
        : base(id, template)
    {
        _foldableHeadphonesTemplate = template;
        if (template.Foldable)
        {
            Foldable = new FoldableComponent(this, template);
            Components.Add(Foldable);
        }
    }

    public bool Folded => Foldable is { Folded: true };

    public int SizeReduceRight => Foldable.SizeReduceRight;

    public int SizeReduceDown => _foldableHeadphonesTemplate.SizeReduceDown;

    public float FoldingTime => _foldableHeadphonesTemplate.FoldingTime;

    public string FoldedSlot => _foldableHeadphonesTemplate.FoldedSlot;

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
