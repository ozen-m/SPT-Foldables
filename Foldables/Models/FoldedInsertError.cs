using EFT.InventoryLogic;

namespace Foldables.Models;

public class FoldedInsertError(Item item) : InventoryError
{
    public readonly Item Item = item;

    public override string ToString()
    {
        return $"Cannot insert {Item} when container is folded";
    }

    public override string GetLocalizedDescription()
    {
        return "Container is folded".Localized();
    }
}
