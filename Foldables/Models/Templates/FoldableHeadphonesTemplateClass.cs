// ReSharper disable UnassignedField.Global

namespace Foldables.Models.Templates;

public class FoldableHeadphonesTemplateClass : HeadphonesTemplateClass, GInterface389
{
    public bool Foldable;
    public int SizeReduceRight;
    public int SizeReduceDown;
    public float FoldingTime;
    public string FoldedSlot;

    int GInterface389.SizeReduceRight => SizeReduceRight;

    string GInterface389.FoldedSlot => FoldedSlot;
}
