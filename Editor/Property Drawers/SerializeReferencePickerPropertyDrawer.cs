using System;
using System.Collections;
using System.Collections.Generic;
using Kryz.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(SerializeReferencePickerAttribute))]
	public class SerializeReferencePickerPropertyDrawer : PropertyDrawer
	{
		private readonly Dictionary<object, string> existing = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var pickerAttribute = (SerializeReferencePickerAttribute)attribute;
			if (pickerAttribute.RemoveDuplicates && IsDuplicate(property))
			{
				SetManagedReferenceValue(property, null, overwriteIfSameType: true);
			}

			Type elementType = GetElementType(fieldInfo.FieldType);
			object? obj = property.managedReferenceValue;
			Type? currentType = obj != null ? GetElementType(obj.GetType()) : null;

			// Reassign if the type of the variable changed and the serialized reference no longer matches the variable type
			if (!elementType.IsAssignableFrom(currentType))
			{
				SetManagedReferenceValue(property, null, overwriteIfSameType: false);
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

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, includeChildren: true);
		}

		private bool IsDuplicate(SerializedProperty property)
		{
			if (property.managedReferenceValue == null)
				return false;

			if (!existing.TryGetValue(property.managedReferenceValue, out string propertyPath))
			{
				existing[property.managedReferenceValue] = property.propertyPath;
				return false;
			}
			return !propertyPath.Equals(property.propertyPath, StringComparison.OrdinalIgnoreCase);
		}

		private static void SetManagedReferenceValue(SerializedProperty property, Type? type, bool overwriteIfSameType)
		{
			if (overwriteIfSameType || type != property.managedReferenceValue?.GetType())
			{
				property.managedReferenceValue = type == null ? null : ObjectCreator.Create(type);
				property.serializedObject.ApplyModifiedProperties();
			}
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
				SetManagedReferenceValue(property, type, overwriteIfSameType: true);
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