using EFT;
using EFT.InventoryLogic;
using Foldables.Models.Templates;
using JetBrains.Annotations;

namespace Foldables.Models.Items;

public class FoldableHeadphones : Headphones, IFoldable
{
    [Component]
    [UsedImplicitly]
    public readonly ExtendedFoldableComponent Foldable;

    public ExtendedFoldableComponent FoldableComponent => Foldable;

    public FoldableHeadphones(string id, FoldableHeadphonesTemplate template)
        : base(id, template)
    {
        if (template.Foldable)
        {
            Foldable = new ExtendedFoldableComponent(this, template);
            Components.Add(Foldable);
        }
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
