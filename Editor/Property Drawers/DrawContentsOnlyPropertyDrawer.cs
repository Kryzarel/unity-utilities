using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(DrawContentsOnlyAttribute))]
	public class DrawContentsOnlyPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			property.DrawContents(position, label);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return property.GetContentsHeight(label);
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			VisualElement element = new();
			foreach (SerializedProperty current in property.EnumerateChildren())
			{
				element.Add(new PropertyField(current, current.displayName));
			}
			return element;
		}
	}
}