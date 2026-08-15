using System.Runtime.CompilerServices;
using EFT.UI;
using JetBrains.Annotations;

namespace Foldables.Utils;

// Thanks Tyfon!
public static class ExtraActionsReturnClassProperties
{
    private static readonly ConditionalWeakTable<AvailableInteractionState, Properties> _properties = [];

    [UsedImplicitly]
    private class Properties
    {
        public bool Folded;
    }

    public static bool GetIsFolded(this AvailableInteractionState actionsReturnClass)
    {
        return _properties.GetOrCreateValue(actionsReturnClass).Folded;
    }

    public static void SetIsFolded(this AvailableInteractionState actionsReturnClass, bool value)
    {
        _properties.GetOrCreateValue(actionsReturnClass).Folded = value;
    }
}
