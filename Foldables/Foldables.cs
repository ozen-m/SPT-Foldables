using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Foldables.Patches;
using UnityEngine;

namespace Foldables;

[BepInPlugin("com.ozen.foldables", "Foldables", "0.0.6")]
[BepInDependency("Tyfon.UIFixes", BepInDependency.DependencyFlags.SoftDependency)]
public class Foldables : BaseUnityPlugin
{
    public static ManualLogSource LogSource;

    public static ConfigEntry<bool> DelayInRaid;
    public static ConfigEntry<bool> ShowSpillDialog;
    public static ConfigEntry<bool> FoldWhileDragging;
    public static ConfigEntry<KeyboardShortcut> FoldWhileDragHotkey;

    protected void Awake()
    {
        LogSource = Logger;

        ShowSpillDialog = Config.Bind("General", "Spill Confirmation", true, new ConfigDescription("Confirm if player wants to spill non empty container items, if possible. If disabled, always spill items without asking", null, new ConfigurationManagerAttributes() { Order = 3 }));
        DelayInRaid = Config.Bind("General", "Delay In Raid", true, new ConfigDescription("Recommended to be enabled. Simulate time it takes to fold/unfold an item in raid. If enabled, this will add a delay", null, new ConfigurationManagerAttributes() { Order = 2, IsAdvanced = true }));
        FoldWhileDragging = Config.Bind("EXPERIMENTAL", "Fold While Dragging (EXPERIMENTAL)", false, new ConfigDescription("Enable folding/unfolding while dragging an item using a hotkey", null, new ConfigurationManagerAttributes() { Order = 1, IsAdvanced = true}));
        FoldWhileDragHotkey = Config.Bind("EXPERIMENTAL", "Dragging Hotkey", new KeyboardShortcut(KeyCode.None), new ConfigDescription("Key used to fold/unfold while dragging", null, new ConfigurationManagerAttributes() { Order = 0, IsAdvanced = true}));

        new FoldablesPatches().Enable();
    }
}
