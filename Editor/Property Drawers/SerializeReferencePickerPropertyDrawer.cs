using System;
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
		private readonly HashSet<object> existingReferences = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var pickerAttribute = (SerializeReferencePickerAttribute)attribute;
			if (!pickerAttribute.AllowDuplicates)
			{
				RemoveDuplicates(property);
			}

			// fieldInfo.FieldType doesn't work if the object is in a list or array
			// property.managedReferenceFieldTypename gives you the type as a string and formatted in a really stupid way
			Type baseType = property.GetPropertyType();
			Type? currentType = property.managedReferenceValue?.GetType();

			// Try to avoid nulls (if the base type has no concrete implementations it can still happen)
			// Also reassign if the type of the variable changed and the serialized reference no longer matches the variable type
			if (property.managedReferenceValue == null || !baseType.IsAssignableFrom(currentType))
			{
				TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(baseType);
				Type? type = FirstOrDefault(types, t => !t.IsAbstract);
				SetManagedReferenceValue(property, type, overwriteIfSameType: false);
			}

			Color color = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0, 0.8f, 0.15f, 0.7f);

			Rect buttonRect = GetButtonRect(position);
			GetPropertiesAndTypesForTargetObjects(property, out SerializedProperty[] properties, out Type?[] propertyTypes);

			if (GUI.Button(buttonRect, GetButtonGuiContent(propertyTypes)))
			{
				TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(baseType);
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

		private void RemoveDuplicates(SerializedProperty property)
		{
			existingReferences.Clear();
			SerializedObject serializedObject = property.serializedObject;
			SerializedProperty current = serializedObject.GetIterator();
			do
			{
				if (current.propertyType == SerializedPropertyType.ManagedReference)
				{
					object obj = current.managedReferenceValue;
					// If we can't add the object, that means it's already in the set, therefore it's a duplicate and we should remove it
					if (obj != null && !existingReferences.Add(obj))
					{
						SetManagedReferenceValue(current, null, overwriteIfSameType: true);
					}
				}
			}
			while (current.Next(enterChildren: true));
			existingReferences.Clear();
		}

		private static Type? FirstOrDefault(TypeCache.TypeCollection types, Func<Type, bool> predicate)
		{
			foreach (Type t in types)
			{
				if (predicate(t))
					return t;
			}
			return default;
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

		private static GUIContent GetButtonGuiContent(IReadOnlyList<Type?> types)
		{
			if (AreAllEqual(types))
			{
				Type? type = types[0];
				return new GUIContent(type?.Name ?? "No implementations", type?.FullName);
			}
			return new GUIContent("Multiple values");
		}

		private static GUIContent GetMenuGuiContent(Type type)
		{
			return new GUIContent(type?.Name ?? "Null", type?.FullName ?? "Null");
		}

		private static GenericMenu GetTypesMenu(IReadOnlyList<Type?> propertyTypes, TypeCache.TypeCollection types, GenericMenu.MenuFunction2 menuFunction)
		{
			GenericMenu menu = new GenericMenu();
			foreach (Type type in types)
			{
				if (!type.IsAbstract)
				{
					menu.AddItem(GetMenuGuiContent(type), on: propertyTypes.Contains(type), menuFunction, type);
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
				SerializedObject serializedObject = new SerializedObject(objects[i]);
				properties[i] = serializedObject.FindProperty(property.propertyPath);
				types[i] = properties[i]?.managedReferenceValue?.GetType();
			}
		}

		private static bool AreAllEqual<T>(IReadOnlyList<T?> list) where T : class
		{
			for (int i = 1; i < list.Count; i++)
			{
				if (list[i] != list[i - 1])
				{
					return false;
				}
			}
			return true;
		}
	}
}