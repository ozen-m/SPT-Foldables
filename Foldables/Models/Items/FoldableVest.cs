using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using Foldables.Models.Templates;
using JetBrains.Annotations;

namespace Foldables.Models.Items;

public class FoldableVest : Vest, IFoldable
{
    [Component]
    [UsedImplicitly]
    public readonly FoldableComponent Foldable;

    private readonly FoldableVestTemplate _foldableVestTemplate;

    public FoldableVest(string id, FoldableVestTemplate template)
        : base(id, template)
    {
        _foldableVestTemplate = template;
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

    public int SizeReduceDown => _foldableVestTemplate.SizeReduceDown;

    public float FoldingTime => _foldableVestTemplate.FoldingTime;

    public string FoldedSlot => _foldableVestTemplate.FoldedSlot;

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
