using Foldables.Models.Items;
using Foldables.Models.Templates;

namespace Foldables.Patches.Mappings;

public class MappingsPatches
{
    public void Enable()
    {
        // Backpacks
        TemplateIdToObjectMappingsClass.TypeTable["5448e53e4bdc2d60728b4567"] = typeof(FoldableBackpackItemClass);
        TemplateIdToObjectMappingsClass.TemplateTypeTable["5448e53e4bdc2d60728b4567"] = typeof(FoldableBackpackTemplateClass);
        TemplateIdToObjectMappingsClass.ItemConstructors["5448e53e4bdc2d60728b4567"] = (id, template) => new FoldableBackpackItemClass(id, (FoldableBackpackTemplateClass)template);

        // Vests
        TemplateIdToObjectMappingsClass.TypeTable["5448e5284bdc2dcb718b4567"] = typeof(FoldableVestItemClass);
        TemplateIdToObjectMappingsClass.TemplateTypeTable["5448e5284bdc2dcb718b4567"] = typeof(FoldableVestTemplateClass);
        TemplateIdToObjectMappingsClass.ItemConstructors["5448e5284bdc2dcb718b4567"] = (id, template) => new FoldableVestItemClass(id, (FoldableVestTemplateClass)template);

        new GetItemTypePatch().Enable();
    }
}
