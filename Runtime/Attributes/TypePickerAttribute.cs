using System;
using UnityEngine;

namespace Kryz.UnityUtils
{
	public class TypePickerAttribute : PropertyAttribute
	{
		public readonly Type Type;
		public readonly TypeFlags Flags;

		public TypePickerAttribute(Type type, TypeFlags flags = TypeFlags.Everything)
		{
			Type = type;
			Flags = flags;
		}
	}
}