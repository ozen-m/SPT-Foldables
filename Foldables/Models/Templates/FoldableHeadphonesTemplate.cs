// ReSharper disable UnassignedField.Global

using EFT.InventoryLogic;

namespace Foldables.Models.Templates;

public class FoldableHeadphonesTemplate : HeadphonesTemplate, IFoldableComponentTemplate
{
    public bool Foldable;
    public int SizeReduceRight;
    public int SizeReduceDown;
    public float FoldingTime;
    public string FoldedSlot;

    int IFoldableComponentTemplate.SizeReduceRight => SizeReduceRight;

    string IFoldableComponentTemplate.FoldedSlot => FoldedSlot;
}
