namespace Foldables.Models.Templates;

public class FoldableBackpackTemplateClass : BackpackTemplateClass, GInterface389
{
    public bool Foldable;
    public int SizeReduceRight;
    public int SizeReduceDown;
    public float FoldingTime;
    public string FoldedSlot;

    int GInterface389.SizeReduceRight => SizeReduceRight;

    string GInterface389.FoldedSlot => FoldedSlot;
}
