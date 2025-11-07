using System;
using System.Threading.Tasks;
using BepInEx.Bootstrap;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;

namespace Foldables.Utils;

public static class MultiSelectInterop
{
    private static readonly Version _requiredVersion = new(5, 0);

    private static bool? _uiFixesLoaded;
    private static Func<int> _getCountMethod;
    private static Action<ItemUiContext, EItemInfoButton, Func<ItemContextAbstractClass, Task>, bool> _applyAllMethod;

    /// <value><c>Count</c> represents the number of items in the current selection, null if UI Fixes is not present.</value>
    public static int Count => Loaded() ? _getCountMethod() : 0;

    /// <summary>
    /// This method takes a <c>Func</c> and calls it *sequentially* on each item in the current selection.
    /// Will no-op if UI Fixes is not present.
    /// </summary>
    /// <param name="func">The function to call on each item.</param>
    /// <param name="interaction">The type of interaction to be done.</param>
    /// <param name="allOrNothing">If the function must be possible for all selected items.</param>
    /// <param name="itemUiContext">Optional <c>ItemUiContext</c>; will use <c>ItemUiContext.Instance</c> if not provided.</param>
    public static void ApplyAll(Func<ItemContextAbstractClass, Task> func, EItemInfoButton interaction, bool allOrNothing, ItemUiContext itemUiContext = null)
    {
        if (!Loaded())
        {
            return;
        }

        itemUiContext ??= ItemUiContext.Instance;
        _applyAllMethod(itemUiContext, interaction, func, allOrNothing);
    }

    private static bool Loaded()
    {
        if (_uiFixesLoaded.HasValue) return _uiFixesLoaded.Value;

        bool present = Chainloader.PluginInfos.TryGetValue("Tyfon.UIFixes", out var pluginInfo);
        _uiFixesLoaded = present && pluginInfo.Metadata.Version >= _requiredVersion;

        if (_uiFixesLoaded.Value)
        {
            var multiSelectControllerType = Type.GetType("UIFixes.MultiSelectController, Tyfon.UIFixes");
            if (multiSelectControllerType != null)
            {
                var getCountMethodInfo = AccessTools.Method(multiSelectControllerType, "GetCount");
                _getCountMethod = AccessTools.MethodDelegate<Func<int>>(getCountMethodInfo);
            }
            var multiSelectType = Type.GetType("UIFixes.MultiSelect, Tyfon.UIFixes");
            if (multiSelectType != null)
            {
                var applyAllMethodInfo = AccessTools.Method(multiSelectType, "ApplyAll");
                _applyAllMethod = AccessTools.MethodDelegate<Action<ItemUiContext, EItemInfoButton, Func<ItemContextAbstractClass, Task>, bool>>(applyAllMethodInfo);
            }
        }

        return _uiFixesLoaded.Value;
    }
}
