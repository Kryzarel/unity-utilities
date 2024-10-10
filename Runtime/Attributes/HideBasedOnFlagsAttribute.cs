using UnityEngine;

namespace Kryz.UnityUtils
{
#if !UNITY_2023_3_OR_NEWER
	/// <summary>
	/// Hide this variable in the inspector unless a specific flag in an enum bitmask is set.
	/// Doesn't work very well for arrays since PropertyDrawers are applied to each element and not the array as a whole.
	/// There's really no way to change how the array itself is drawn (the part with + and - buttons, size, etc)
	/// unless we use a custom Editor or PropertyDrawer that applies to the declaring type/object.
	/// </summary>
#else
	/// <summary>
	/// Hide this variable in the inspector unless a specific flag in an enum bitmask is set.
	/// </summary>
#endif
	public class HideBasedOnFlagsAttribute : PropertyAttribute
	{
		public readonly int FlagValue;
		public readonly string VariableName;

		public HideBasedOnFlagsAttribute(int flagValue, string variableName)
#if UNITY_2023_3_OR_NEWER
		: base(applyToCollection: true)
#endif
		{
			FlagValue = flagValue;
			VariableName = variableName;
		}
	}
}