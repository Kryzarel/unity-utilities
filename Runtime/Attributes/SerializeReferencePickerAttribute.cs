using UnityEngine;

namespace Kryz.EditorUtils
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