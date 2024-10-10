using UnityEngine;

namespace Kryz.UnityUtils
{
	public class SerializeReferencePickerAttribute : PropertyAttribute
	{
		public readonly bool AllowDuplicates;

		public SerializeReferencePickerAttribute(bool allowDuplicates = false)
		{
			AllowDuplicates = allowDuplicates;
		}
	}
}