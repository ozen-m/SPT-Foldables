// ReSharper disable UnassignedField.Global

using EFT.InventoryLogic;

namespace Foldables.Models.Templates;

public class FoldableHeadphonesTemplate : HeadphonesTemplate, IExtendedFoldableComponentTemplate
{
    public bool Foldable { get; set; }
    public string FoldedSlot { get; set; }
    public int SizeReduceRight { get; set; }
    public int SizeReduceDown { get; set; }
    public float FoldingTime { get; set; }
}
