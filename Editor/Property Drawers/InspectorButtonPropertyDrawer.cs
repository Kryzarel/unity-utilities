using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
	public class InspectorButtonPropertyDrawer : PropertyDrawer
	{
		private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			InspectorButtonAttribute buttonAttribute = (InspectorButtonAttribute)attribute;
			Rect pos = buttonAttribute.ButtonWidth <= 0 ? position : new Rect(position.x, position.y, buttonAttribute.ButtonWidth, position.height);

			if (GUI.Button(pos, buttonAttribute.MethodName))
			{
				MethodInfo methodInfo = fieldInfo.DeclaringType.GetMethod(buttonAttribute.MethodName, flags);
				methodInfo.Invoke(property.serializedObject.targetObject, null);
			}
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			InspectorButtonAttribute buttonAttribute = (InspectorButtonAttribute)attribute;
			MethodInfo methodInfo = fieldInfo.DeclaringType.GetMethod(buttonAttribute.MethodName, flags);

			Object target = property.serializedObject.targetObject;
			Button button = new(() => methodInfo.Invoke(target, null)) { text = buttonAttribute.MethodName };

			if (buttonAttribute.ButtonWidth > 0)
			{
				button.style.width = buttonAttribute.ButtonWidth;
			}
			return button;
		}
	}
}