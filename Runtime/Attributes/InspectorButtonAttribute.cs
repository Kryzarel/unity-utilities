using UnityEngine;

namespace Kryz.UnityUtils
{
	/// <summary>
	/// This will turn a variable in the inspector into a button that calls a method when pressed.
	/// Can only be applied to fields because PropertyDrawers only operate on fields (either public or tagged with the [SerializeField] attribute).
	/// </summary>
	public class InspectorButtonAttribute : PropertyAttribute
	{
		public readonly string MethodName;
		public readonly float ButtonWidth;

		public InspectorButtonAttribute(string methodName, float buttonWidth = 0)
		{
			MethodName = methodName;
			ButtonWidth = buttonWidth;
		}
	}
}