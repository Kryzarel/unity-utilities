using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Kryz.SharpUtils;

namespace Kryz.UnityUtils.Editor
{
#if !UNITY_2023_3_OR_NEWER
	/// <summary>
	/// Doesn't work very well for arrays since PropertyDrawers are applied to each element and not the array as a whole.
	/// There's really no way to change how the array itself is drawn (the part with + and - buttons, size, etc)
	/// unless we use a custom Editor or PropertyDrawer that applies to the declaring type/object.
	/// </summary>
#endif
	[CustomPropertyDrawer(typeof(HideBasedOnFlagsAttribute))]
	public class HideBasedOnFlagsPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (HasFlag(property))
			{
				EditorGUI.PropertyField(position, property, label, includeChildren: true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (HasFlag(property))
			{
				return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
			}
			return 0;
		}

		private bool HasFlag(SerializedProperty property)
		{
			HideBasedOnFlagsAttribute hideAttribute = (HideBasedOnFlagsAttribute)attribute;
			SerializedProperty enumProperty = property.FindProperty(hideAttribute.VariableName);
			return enumProperty == null || enumProperty.enumValueFlag.HasFlag(hideAttribute.FlagValue);
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			HideBasedOnFlagsAttribute hideAttribute = (HideBasedOnFlagsAttribute)attribute;
			SerializedProperty enumProperty = property.FindProperty(hideAttribute.VariableName);

			VisualElement element = new();
			PropertyField propertyField = new(property);
			element.Add(propertyField);

			if (enumProperty != null)
			{
				SetPropertyDisplay(enumProperty);
				PropertyField enumPropertyField = new(enumProperty);
				enumPropertyField.style.display = DisplayStyle.None;
				enumPropertyField.RegisterValueChangeCallback(evt => SetPropertyDisplay(evt.changedProperty));
				element.Add(enumPropertyField);
			}

			void SetPropertyDisplay(SerializedProperty enumProperty)
			{
				bool hasFlag = enumProperty.enumValueFlag.HasFlag(hideAttribute.FlagValue);
				propertyField.style.display = hasFlag ? DisplayStyle.Flex : DisplayStyle.None;
			}
			return element;
		}
	}
}