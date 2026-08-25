using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kryz.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(SerializeReferencePickerAttribute))]
	public class SerializeReferencePickerPropertyDrawer : PropertyDrawer
	{
		private sealed class Dummy { }

		public class PopupFieldCustom : PopupField<Type>
		{
			public override Type value
			{
				get => base.value;
				set
				{
					// Force notify whenever the user clicks an option on the dropdown.
					// This is needed for multi object editing to work properly, since the dropdown will default to the value of ONE of the objects in the selection
					// And if you click on that value, it won't change ANY selected object, even if they have different values than the one that appears selected
					base.value = typeof(Dummy);
					base.value = value;
				}
			}

			public PopupFieldCustom(string label, List<Type> choices, int defaultIndex, Func<Type, string>? formatSelectedValueCallback = null, Func<Type, string>? formatListItemCallback = null)
				: base(label, choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
			{
			}
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			Type elementType = GetElementType(fieldInfo.FieldType);
			List<Type> types = TypeCache.GetTypesDerivedFrom(elementType).Where(t => !t.IsAbstract).OrderBy(t => t.FullName).Prepend(null!).ToList();

			Type? propType = property.managedReferenceValue?.GetType();

			// Reassign if the type of the variable changed and the serialized reference no longer matches the variable type
			if (propType != null && !elementType.IsAssignableFrom(propType))
			{
				SetManagedReferenceValue(property, null, overwriteIfSameType: true);
			}

			VisualElement root = new();

			PopupFieldCustom popup = new(property.displayName, types, 0, t => FormatSelectedType(t, types, property), FormatType);
			popup.style.flexGrow = 1;
			popup.style.marginLeft = 0;
			popup.pickingMode = PickingMode.Ignore; // Ignore input on the popup parent element
			popup.AddToClassList("unity-base-field__aligned");

			popup.labelElement.pickingMode = PickingMode.Ignore; // And on the label as well
			popup.labelElement.AddToClassList("unity-property-field__label");

			VisualElement popupButton = popup.Q<VisualElement>(className: "unity-base-popup-field__input");
			popupButton.style.backgroundColor = new Color(0, 0.275f, 0, 1);
			popupButton.pickingMode = PickingMode.Position; // Enable input on the popup button only

			TextElement popupButtonText = popupButton.Q<TextElement>();
			popupButtonText.style.unityTextAlign = TextAnchor.MiddleCenter;

			PropertyField propertyField = new(property);
			propertyField.Bind(property.serializedObject);
			root.Add(propertyField);

			propertyField.RegisterCallback<GeometryChangedEvent>(evt =>
			{
				// Disable the original PropertyField label and put the PopupField in its place.
				// Since we disabled the popup's input except for the button, clicking the label or any empty space
				// will open the foldout, but clicking the popup button itself will open the popup dropdown list.
				VisualElement element = (VisualElement)evt.target;
				Label label = element.Q<Label>();

				if (label != null)
				{
					label.style.display = DisplayStyle.None;
					label.parent.Add(popup);
				}
			});

			UpdatePopup(property, popup);

			popup.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue == typeof(Dummy)) return; // This is so fucking stupid omg... Unity can shove UI Toolkit somwhere..

				property.serializedObject.Update();
				UpdateProperty(evt.newValue, property);
				UpdatePopup(property, popup);
			});

			// Re-evaluate the popup when the inspector is rebound/updated.
			// root.RegisterCallback<AttachToPanelEvent>(_ => { UpdatePicker(); });

			return root;
		}

		private static void UpdatePopup(SerializedProperty property, PopupFieldCustom popup)
		{
			Type? currentType = property.managedReferenceValue?.GetType();
			popup.SetValueWithoutNotify(currentType!);
			popup.tooltip = popup.value?.FullName;
		}

		private static void UpdateProperty(Type type, SerializedProperty property)
		{
			if (property.serializedObject.isEditingMultipleObjects)
			{
				GetPropertiesAndTypesForTargetObjects(property, out SerializedProperty[] properties, out _);

				foreach (SerializedProperty prop in properties)
				{
					SetManagedReferenceValue(prop, type, overwriteIfSameType: false);
				}
			}
			else
			{
				SetManagedReferenceValue(property, type, overwriteIfSameType: false);
			}
		}

		private static Type GetElementType(Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			else if (typeof(IList).IsAssignableFrom(type))
			{
				return type.IsGenericType ? type.GenericTypeArguments[0] : typeof(object);
			}
			return type;
		}

		private static string FormatType(Type type)
		{
			return type == null ? "Null" : type.Name;
		}

		private string FormatSelectedType(Type type, List<Type> types, SerializedProperty property)
		{
			if (types.Count == 1)
			{
				return "No implementations";
			}

			if (property.serializedObject.isEditingMultipleObjects)
			{
				GetPropertiesAndTypesForTargetObjects(property, out _, out Type?[] propertyTypes);
				bool multipleValues = propertyTypes.Skip(1).Any(t => t != propertyTypes[0]);
				return multipleValues ? "Multiple values" : FormatType(type);
			}

			return FormatType(type);
		}

		private static void SetManagedReferenceValue(SerializedProperty property, Type? type, bool overwriteIfSameType)
		{
			if (overwriteIfSameType || type != property.managedReferenceValue?.GetType())
			{
				property.managedReferenceValue = type == null ? null : ObjectCreator.Create(type);
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		private static void GetPropertiesAndTypesForTargetObjects(SerializedProperty property, out SerializedProperty[] properties, out Type?[] types)
		{
			Object[] objects = property.serializedObject.targetObjects;

			properties = new SerializedProperty[objects.Length];
			types = new Type[objects.Length];

			for (int i = 0; i < objects.Length; i++)
			{
				SerializedObject serializedObject = new(objects[i]);
				properties[i] = serializedObject.FindProperty(property.propertyPath);
				types[i] = properties[i]?.managedReferenceValue?.GetType();
			}
		}
	}
}