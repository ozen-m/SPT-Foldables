using EFT.InventoryLogic;

namespace Foldables.Models;

public interface IFoldable : IFoldableComponentTemplate
{
    bool Folded { get; }

    int SizeReduceDown { get; }

    float FoldingTime { get; }
}
