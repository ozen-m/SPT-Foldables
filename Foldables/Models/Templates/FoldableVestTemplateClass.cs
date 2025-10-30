namespace Foldables.Models.Templates;

public class FoldableVestTemplateClass : VestTemplateClass, GInterface389
{
	public bool Foldable;

	public int SizeReduceRight;

	public int SizeReduceDown;

	public string FoldedSlot;

	int GInterface389.SizeReduceRight => SizeReduceRight;

	string GInterface389.FoldedSlot => FoldedSlot;
}
