namespace Foldables.Models.Items;

public interface IFoldable
{
    bool Folded { get; }
    int SizeReduceRight { get; }
    int SizeReduceDown { get; }
    float FoldingTime { get; }
}
