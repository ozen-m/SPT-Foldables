using EFT.InventoryLogic;
using Foldables.Models.Templates;

namespace Foldables.Models;

public class ExtendedFoldableComponent(Item item, IExtendedFoldableComponentTemplate template) : FoldableComponent(item, template)
{
    public int SizeReduceDown => template.SizeReduceDown;

    public float FoldingTime => template.FoldingTime;
}
