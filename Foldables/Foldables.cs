using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.InventoryLogic;
using Foldables.Models.Items;
using Foldables.Models.Templates;
using Foldables.Patches.Operations.InRaid;
using SPT.Reflection.Patching;
using UnityEngine;

#pragma warning disable CA2211

namespace Foldables;

[BepInPlugin("com.ozen.foldables", "Foldables", "1.1.0")]
[BepInDependency("com.tyfon.uifixes", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.ozen.continuousloadammo", BepInDependency.DependencyFlags.SoftDependency)]
public class Foldables : BaseUnityPlugin
{
    private const string BackpackId = "5448e53e4bdc2d60728b4567";
    private const string VestId = "5448e5284bdc2dcb718b4567";
    private const string HeadphonesId = "5645bcb74bdc2ded0b8b4578";

    public static ManualLogSource LogSource;
    public static ConfigEntry<bool> FoldWhileEquipped;
    public static ConfigEntry<bool> ShowSpillDialog;
    public static ConfigEntry<bool> FoldWhileDragging;
    public static ConfigEntry<KeyboardShortcut> FoldWhileDragHotkey;

    protected void Awake()
    {
        LogSource = Logger;

        FoldWhileEquipped = Config.Bind("General", "Fold While Equipped", true, new ConfigDescription("Allow folding while gear is equipped", null, new ConfigurationManagerAttributes() { Order = 3 }));
        ShowSpillDialog = Config.Bind("General", "Spill Confirmation", true, new ConfigDescription("Confirm if player wants to spill non empty container items, if possible. If disabled, always spill items without asking", null, new ConfigurationManagerAttributes() { Order = 2 }));
        FoldWhileDragging = Config.Bind("Experimental", "Fold While Dragging", false, new ConfigDescription("Enable folding/unfolding while dragging an item using a hotkey", null, new ConfigurationManagerAttributes() { Order = 1, IsAdvanced = true }));
        FoldWhileDragHotkey = Config.Bind("Experimental", "Dragging Hotkey", new KeyboardShortcut(KeyCode.None), new ConfigDescription("Key used to fold/unfold while dragging", null, new ConfigurationManagerAttributes() { Order = 0, IsAdvanced = true }));

        // Mappings
        // Backpacks
        JsonTypes.TypeTable[BackpackId] = typeof(FoldableBackpack);
        JsonTypes.TemplateTypeTable[BackpackId] = typeof(FoldableBackpackTemplate);
        JsonTypes.ItemConstructors[BackpackId] = (id, template) => new FoldableBackpack(id, (FoldableBackpackTemplate)template);

        // Vests
        JsonTypes.TypeTable[VestId] = typeof(FoldableVest);
        JsonTypes.TemplateTypeTable[VestId] = typeof(FoldableVestTemplate);
        JsonTypes.ItemConstructors[VestId] = (id, template) => new FoldableVest(id, (FoldableVestTemplate)template);

        // Headphones
        JsonTypes.TypeTable[HeadphonesId] = typeof(FoldableHeadphones);
        JsonTypes.TemplateTypeTable[HeadphonesId] = typeof(FoldableHeadphonesTemplate);
        JsonTypes.ItemConstructors[HeadphonesId] = (id, template) => new FoldableHeadphones(id, (FoldableHeadphonesTemplate)template);

        /*AddToMappingsClass(BackpackId, typeof(FoldableBackpackItemClass), typeof(FoldableBackpackTemplateClass));
        AddToMappingsClass(VestId, typeof(FoldableVestItemClass), typeof(FoldableVestTemplateClass));
        AddToMappingsClass(VestId, typeof(FoldableHeadphonesItemClass), typeof(FoldableHeadphonesTemplateClass));*/

        // Add custom types to sorting
        AddTypesToItemSorter();

        var patchManager = new PatchManager(this, true);
        patchManager.EnablePatches();

        // Mod compatibilities
        if (Chainloader.PluginInfos.ContainsKey("com.ozen.continuousloadammo"))
        {
            new InventoryScreenClosePatch().Enable();
        }
    }

    private static void AddTypesToItemSorter()
    {
        // Insert instead of replace?
        var backpackIndex = ItemSorter.IndexOf(typeof(Backpack));
        ItemSorter._itemSuccessors.Insert(backpackIndex, typeof(FoldableBackpack));

        var vestIndex = ItemSorter.IndexOf(typeof(Vest));
        ItemSorter._itemSuccessors.Insert(vestIndex, typeof(FoldableVest));

        var headphonesIndex = ItemSorter.IndexOf(typeof(Headphones));
        ItemSorter._itemSuccessors.Insert(headphonesIndex, typeof(FoldableHeadphones));
    }

    /*private static void AddToMappingsClass(string itemId, Type itemType, Type itemTemplateType)
    {
        JsonTypes.TypeTable[itemId] = itemType;
        JsonTypes.TemplateTypeTable[itemId] = itemTemplateType;
        JsonTypes.ItemConstructors[itemId] = (id, template) =>
            (Item)Activator.CreateInstance(itemType, id, template);
    }*/
}
