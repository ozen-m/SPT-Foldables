using EFT.InventoryLogic;

namespace Foldables.Models.Templates;

public interface IExtendedFoldableComponentTemplate : IFoldableComponentTemplate
{
    int SizeReduceDown { get; set; }
    float FoldingTime { get; set; }
}
