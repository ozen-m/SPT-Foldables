using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Foldables.Patches;
using UnityEngine;

namespace Foldables;

[BepInPlugin("com.ozen.foldables", "Foldables", "1.0.0")]
[BepInDependency("Tyfon.UIFixes", BepInDependency.DependencyFlags.SoftDependency)]
public class Foldables : BaseUnityPlugin
{
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

        new FoldablesPatches().Enable();
    }
}
