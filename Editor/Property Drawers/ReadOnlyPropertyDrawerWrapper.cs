using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kryz.EditorUtils
{
	public class ReadOnlyPropertyDrawerWrapper : PropertyDrawer
	{
		public readonly PropertyDrawer? InnerDrawer;

		public ReadOnlyPropertyDrawerWrapper(PropertyDrawer propertyDrawer)
		{
			InnerDrawer = propertyDrawer;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.DisabledScope(disabled: true))
			{
				if (InnerDrawer != null)
				{
					InnerDrawer.OnGUI(position, property, label);
				}
				else
				{
					EditorGUI.PropertyField(position, property, label, includeChildren: true);
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return InnerDrawer?.GetPropertyHeight(property, label) ?? EditorGUI.GetPropertyHeight(property);
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			VisualElement visualElement = InnerDrawer?.CreatePropertyGUI(property) ?? new PropertyField(property);
			visualElement.SetEnabled(false);
			return visualElement;
		}
	}
}