using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Foldables.Utils;

// Thanks Tyfon!
public static class ExtraActionsReturnClassProperties
{
    private static readonly ConditionalWeakTable<ActionsReturnClass, Properties> _properties = [];

    [UsedImplicitly]
    private class Properties
    {
        public bool Folded;
    }

    public static bool GetIsFolded(this ActionsReturnClass actionsReturnClass) => _properties.GetOrCreateValue(actionsReturnClass).Folded;
    public static void SetIsFolded(this ActionsReturnClass actionsReturnClass, bool value) => _properties.GetOrCreateValue(actionsReturnClass).Folded = value;
}
