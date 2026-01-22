using UnityEngine;

namespace Kryz.UnityUtils
{
	public class SerializeReferencePickerAttribute : PropertyAttribute
	{
		public readonly bool RemoveDuplicates;

		public SerializeReferencePickerAttribute(bool removeDuplicates = false)
		{
			RemoveDuplicates = removeDuplicates;
		}
	}
}