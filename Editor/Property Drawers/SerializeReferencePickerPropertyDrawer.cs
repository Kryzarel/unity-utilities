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

		private static readonly Dictionary<Type, HashSet<string>> managedPropertyPaths = new();

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			SerializeReferencePickerAttribute pickerAttribute = (SerializeReferencePickerAttribute)attribute;

			string propertyPath = property.propertyPath;
			SerializedObject serializedObject = property.serializedObject;
			GetPropertyPaths(property).Add(propertyPath);

			Type elementType = GetElementType(fieldInfo.FieldType);
			List<Type> types = TypeCache.GetTypesDerivedFrom(elementType).Where(t => !t.IsAbstract).OrderBy(t => t.FullName).Prepend(null!).ToList();

			VisualElement root = new();

			PopupFieldCustom popup = new(property.displayName, types, 0, t => FormatSelectedType(t, types, serializedObject, propertyPath), FormatType);
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

			UpdatePicker();

			popup.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue == typeof(Dummy)) return; // This is so fucking stupid omg... Unity can shove UI Toolkit somwhere..
				UpdatePicker(updateFromUser: true, evt.newValue);
			});

			// Re-evaluate the popup when the inspector is rebound/updated.
			root.RegisterCallback<AttachToPanelEvent>(_ => { UpdatePicker(); });

			void UpdatePicker(bool updateFromUser = false, Type? type = null)
			{
				serializedObject.Update();
				SerializedProperty property = serializedObject.FindProperty(propertyPath);
				UpdateWithMultiSelectSupport(updateFromUser, type, pickerAttribute.RemoveDuplicates, elementType, property);

				Type? currentType = property.managedReferenceValue?.GetType();
				popup.SetValueWithoutNotify(currentType!);
				popup.tooltip = popup.value?.FullName;
			}

			return root;
		}

		private static HashSet<string> GetPropertyPaths(SerializedProperty property)
		{
			Type objType = property.serializedObject.targetObject.GetType();

			if (!managedPropertyPaths.TryGetValue(objType, out HashSet<string> propertyPaths))
			{
				managedPropertyPaths[objType] = propertyPaths = new HashSet<string>();
			}

			return propertyPaths;
		}

		private static void UpdateWithMultiSelectSupport(bool updateFromUser, Type? type, bool removeDuplicates, Type elementType, SerializedProperty property)
		{
			if (property.serializedObject.isEditingMultipleObjects)
			{
				GetPropertiesAndTypesForTargetObjects(property, out SerializedProperty[] properties, out _);

				foreach (SerializedProperty prop in properties)
				{
					UpdateProperty(updateFromUser, type, removeDuplicates, elementType, prop);
				}
			}
			else
			{
				UpdateProperty(updateFromUser, type, removeDuplicates, elementType, property);
			}
		}

		private static void UpdateProperty(bool updateFromUser, Type? type, bool removeDuplicates, Type elementType, SerializedProperty property)
		{
			if (updateFromUser)
			{
				SetManagedReferenceValue(property, type, overwriteIfSameType: false);
			}
			else
			{
				Type? propType = property.managedReferenceValue?.GetType();

				// Reassign if the type of the variable changed and the serialized reference no longer matches the variable type
				if (propType != null && !elementType.IsAssignableFrom(propType))
				{
					SetManagedReferenceValue(property, null, overwriteIfSameType: true);
				}
				else if (removeDuplicates && IsDuplicate(property))
				{
					SetManagedReferenceValue(property, propType, overwriteIfSameType: true);
				}
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

		private string FormatSelectedType(Type type, List<Type> types, SerializedObject serializedObject, string propertyPath)
		{
			if (types.Count == 1)
			{
				return "No implementations";
			}

			if (serializedObject.isEditingMultipleObjects)
			{
				SerializedProperty property = serializedObject.FindProperty(propertyPath);
				GetPropertiesAndTypesForTargetObjects(property, out _, out Type?[] propertyTypes);
				bool multipleValues = propertyTypes.Skip(1).Any(t => t != propertyTypes[0]);
				return multipleValues ? "Multiple values" : FormatType(type);
			}

			return FormatType(type);
		}

		private static bool IsDuplicate(SerializedProperty property)
		{
			if (property.managedReferenceValue == null)
				return false;

			foreach (string path in GetPropertyPaths(property))
			{
				SerializedProperty prop = property.serializedObject.FindProperty(path);

				if (prop == null || prop.propertyType == SerializedPropertyType.ManagedReference)
					continue;

				if (prop.managedReferenceId == property.managedReferenceId)
				{
					return true;
				}
			}
			return false;
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

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializeReferencePickerAttribute pickerAttribute = (SerializeReferencePickerAttribute)attribute;

			if (pickerAttribute.RemoveDuplicates && IsDuplicate(property))
			{
				SetManagedReferenceValue(property, null, overwriteIfSameType: true);
			}

			object? obj = property.managedReferenceValue;
			Type elementType = GetElementType(fieldInfo.FieldType);
			Type? currentType = obj != null ? GetElementType(obj.GetType()) : null;

			// Reassign if the type of the variable changed and the serialized reference no longer matches the variable type
			if (currentType != null && !elementType.IsAssignableFrom(currentType))
			{
				SetManagedReferenceValue(property, null, overwriteIfSameType: true);
			}

			Color color = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0, 0.8f, 0.15f, 0.7f);

			Rect buttonRect = GetButtonRect(position);
			GetPropertiesAndTypesForTargetObjects(property, out SerializedProperty[] properties, out Type?[] propertyTypes);

			TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(elementType);

			if (GUI.Button(buttonRect, GetButtonGuiContent(types, propertyTypes)))
			{
				GenericMenu menu = GetTypesMenu(propertyTypes, types, obj => MenuFunction(properties, (Type)obj));
				menu.DropDown(buttonRect);
			}

			GUI.backgroundColor = color;
			EditorGUI.PropertyField(position, property, label, includeChildren: true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, includeChildren: true);
		}

		private static Rect GetButtonRect(Rect position)
		{
			float width = EditorGUIUtility.labelWidth;
			float height = EditorGUIUtility.singleLineHeight;
			return new Rect(position.x + width, position.y, position.width - width, height);
		}

		private static GUIContent GetGuiContent(Type? type)
		{
			return new GUIContent(type?.Name ?? "Null", type?.FullName);
		}

		private static GUIContent GetButtonGuiContent(TypeCache.TypeCollection derivedTypes, IReadOnlyList<Type?> propertyTypes)
		{
			if (derivedTypes.Count == 0)
			{
				return new GUIContent("No implementations");
			}

			for (int i = 1; i < propertyTypes.Count; i++)
			{
				if (propertyTypes[i] != propertyTypes[i - 1])
				{
					return new GUIContent("Multiple values");
				}
			}

			return GetGuiContent(propertyTypes[0]);
		}

		private static GenericMenu GetTypesMenu(IReadOnlyList<Type?> propertyTypes, TypeCache.TypeCollection types, GenericMenu.MenuFunction2 menuFunction)
		{
			GenericMenu menu = new();

			menu.AddItem(GetGuiContent(null), on: propertyTypes.Contains<Type?, IReadOnlyList<Type?>>(null), menuFunction, null);

			foreach (Type type in types)
			{
				if (!type.IsAbstract)
				{
					menu.AddItem(GetGuiContent(type), on: propertyTypes.Contains<Type?, IReadOnlyList<Type?>>(type), menuFunction, type);
				}
			}
			return menu;
		}

		private static void MenuFunction(IReadOnlyList<SerializedProperty> properties, Type type)
		{
			foreach (SerializedProperty property in properties)
			{
				SetManagedReferenceValue(property, type, overwriteIfSameType: false);
			}
		}
	}
}