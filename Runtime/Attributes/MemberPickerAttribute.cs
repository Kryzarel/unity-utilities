using System;
using System.Reflection;
using UnityEngine;

namespace Kryz.UnityUtils
{
	public class MemberPickerAttribute : PropertyAttribute
	{
		public readonly string ParentTypeProperty;
		public readonly BindingFlags BindingFlags;
		public readonly MemberFlags MemberFlags;
		public readonly string? MemberTypeProperty;

		public MemberPickerAttribute(string parentTypeProperty, BindingFlags bindingFlags, MemberFlags memberFlags = MemberFlags.Everything, string? memberTypeProperty = null)
		{
			ParentTypeProperty = parentTypeProperty;
			BindingFlags = bindingFlags;
			MemberFlags = memberFlags;
			MemberTypeProperty = memberTypeProperty;
		}
	}
}