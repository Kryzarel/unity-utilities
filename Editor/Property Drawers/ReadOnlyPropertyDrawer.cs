using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.DisabledScope(disabled: true))
			{
				EditorGUI.PropertyField(position, property, label, includeChildren: true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property);
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			PropertyField propertyField = new(property);
			propertyField.SetEnabled(false);
			return propertyField;
		}
	}
}