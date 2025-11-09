using EFT.InventoryLogic;

namespace Foldables.Models;

public class FoldedInsertError(Item item) : InventoryError
{
    public override string ToString()
    {
        return $"Cannot insert {item} when container is folded";
    }

    public override string GetLocalizedDescription()
    {
        return "Container is folded".Localized();
    }
}
