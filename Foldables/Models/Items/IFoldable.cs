namespace Foldables.Models.Items;

public interface IFoldable
{
    ExtendedFoldableComponent FoldableComponent { get; }
    bool Folded => FoldableComponent.Folded;
    int SizeReduceRight => FoldableComponent.SizeReduceRight;
    int SizeReduceDown => FoldableComponent.SizeReduceDown;
    float FoldingTime => FoldableComponent.FoldingTime;
}
