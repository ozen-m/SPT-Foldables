using System.Runtime.CompilerServices;

namespace Foldables.Utils;

// Thanks Tyfon!
public static class ExtraActionsReturnClassProperties
{
	private static readonly ConditionalWeakTable<ActionsReturnClass, Properties> properties = [];

	private class Properties
	{
		public bool Folded = false;
	}

	public static bool GetIsFolded(this ActionsReturnClass actionsReturnClass) => properties.GetOrCreateValue(actionsReturnClass).Folded;
	public static void SetIsFolded(this ActionsReturnClass actionsReturnClass, bool value) => properties.GetOrCreateValue(actionsReturnClass).Folded = value;
}
