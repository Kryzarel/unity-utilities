using System.Reflection;
using UnityEngine;

namespace Kryz.UnityUtils
{
	public class MemberPickerAttribute : PropertyAttribute
	{
		public readonly string ParentTypeProperty;
		public readonly BindingFlags BindingFlags;
		public readonly MemberTypes MemberTypes;
		public readonly string? MemberTypeField;
		public readonly bool AllowDerived;

		public MemberPickerAttribute(string parentTypeProperty, BindingFlags bindingFlags, MemberTypes memberTypes = MemberTypes.All, string? memberTypeField = null, bool allowDerived = true)
		{
			ParentTypeProperty = parentTypeProperty;
			BindingFlags = bindingFlags;
			MemberTypes = memberTypes;
			MemberTypeField = memberTypeField;
			AllowDerived = allowDerived;
		}
	}
}