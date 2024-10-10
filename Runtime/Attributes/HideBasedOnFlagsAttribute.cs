using UnityEngine;

namespace Kryz.UnityUtils
{
	/// <summary>
	/// Hide this variable in the inspector unless a specific flag in an enum bitmask is set.
	/// Doesn't work very well for arrays since PropertyDrawers are applied to each element and not the array as a whole.
	/// There's really no way to change how the array itself is drawn (the part with + and - buttons, size, etc)
	/// unless we use a custom Editor or PropertyDrawer that applies to the declaring type/object.
	/// </summary>
	public class HideBasedOnFlagsAttribute : PropertyAttribute
	{
		public readonly int FlagValue;
		public readonly string VariableName;

		public HideBasedOnFlagsAttribute(int flagValue, string variableName)
		{
			FlagValue = flagValue;
			VariableName = variableName;
		}
	}
}