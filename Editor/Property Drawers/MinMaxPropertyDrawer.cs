using UnityEditor;
using UnityEngine.UIElements;

namespace Kryz.UnityUtils.Editor
{
	public abstract class MinMaxPropertyDrawer<T> : PropertyDrawer
	{
		protected SerializedProperty? minProp;
		protected SerializedProperty? maxProp;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			minProp = property.FindPropertyRelative("Min");
			maxProp = property.FindPropertyRelative("Max");

			VisualElement element = new();
			element.AddToClassList("unity-base-field");
			element.style.flexDirection = FlexDirection.Row;
			element.tooltip = property.tooltip;

			Label label = new(property.displayName);
			label.AddToClassList("unity-base-field__label");

			TextValueField<T> minField = CreateField(minProp.displayName);
			minField.AddToClassList("unity-base-field__input");
			minField.bindingPath = minProp.propertyPath;
			minField.label = minProp.displayName;

			TextValueField<T> maxField = CreateField(maxProp.displayName);
			maxField.AddToClassList("unity-base-field__input");
			maxField.bindingPath = maxProp.propertyPath;
			maxField.label = maxProp.displayName;

			minField.labelElement.style.minWidth = minField.labelElement.MeasureTextSize(minField.label, 0, VisualElement.MeasureMode.AtMost, 0, default).x;
			maxField.labelElement.style.minWidth = maxField.labelElement.MeasureTextSize(maxField.label, 0, VisualElement.MeasureMode.AtMost, 0, default).x;

			minField.RegisterValueChangedCallback(MinFieldCallback);
			maxField.RegisterValueChangedCallback(MaxFieldCallback);

			element.Add(label);
			element.Add(minField);
			element.Add(maxField);
			return element;
		}

		protected abstract TextValueField<T> CreateField(string label);
		protected abstract void MinFieldCallback(ChangeEvent<T> e);
		protected abstract void MaxFieldCallback(ChangeEvent<T> e);
	}
}