using System;
using UnityEngine;

namespace Kryz.EditorUtils
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ReadOnlyDecoratorAttribute : PropertyAttribute { }
}