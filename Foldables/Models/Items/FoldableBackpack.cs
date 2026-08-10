using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using Foldables.Models.Templates;
using JetBrains.Annotations;

namespace Foldables.Models.Items;

public class FoldableBackpack : Backpack, IFoldable
{
    [Component]
    [UsedImplicitly]
    public readonly FoldableComponent Foldable;

    private readonly FoldableBackpackTemplate _foldableBackpackTemplate;

    public FoldableBackpack(string id, FoldableBackpackTemplate template)
        : base(id, template)
    {
        _foldableBackpackTemplate = template;
        if (template.Foldable)
        {
            Foldable = new FoldableComponent(this, template);
            Components.Add(Foldable);
        }
    }

    public override OperationResult Apply(ItemController itemController, Item item, int count, bool simulate)
    {
        if (Folded)
        {
            return new FoldedInsertError(item);
        }
        return base.Apply(itemController, item, count, simulate);
    }

    public bool Folded => Foldable is { Folded: true };

    public int SizeReduceRight => Foldable.SizeReduceRight;

    public int SizeReduceDown => _foldableBackpackTemplate.SizeReduceDown;

    public float FoldingTime => _foldableBackpackTemplate.FoldingTime;

    public string FoldedSlot => _foldableBackpackTemplate.FoldedSlot;

    public override int GetHashSum()
    {
        var hashSum = base.GetHashSum();
        if (Foldable != null)
        {
            hashSum = hashSum * 27 + Foldable.Folded.GetHashCode();
        }
        return hashSum;
    }
}
