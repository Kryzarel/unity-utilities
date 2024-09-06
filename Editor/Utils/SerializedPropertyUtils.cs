using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Kryz.SharpUtils;

namespace Kryz.EditorUtils
{
	public static class SerializedPropertyUtils
	{
		/// <summary>
		/// Unity can be really stupid sometimes... It doesn't have a way to retrieve the value of a SerializedProperty
		/// when the value is a regular C# class. "objectReferenceValue" only works if the value is a Unity.Object.
		/// not even "boxedValue", introduced in version 2022.1 works for this.
		/// </summary>
		public static object GetValue(this SerializedProperty property)
		{
			return property.GetValueAndType(out _);
		}

		public static Type GetPropertyType(this SerializedProperty property)
		{
			property.GetValueAndType(out Type type);
			return type;
		}

		public static object GetValueAndType(this SerializedProperty property, out Type type)
		{
			object obj = property.serializedObject.targetObject;
			type = obj.GetType();

			foreach ((ReadOnlySpan<char> part, int index) in property.EnumeratePathParts())
			{
				const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				FieldInfo fieldInfo = type.GetFieldInSubclasses(part.ToString(), bindingFlags);
				obj = fieldInfo.GetValue(obj);
				type = fieldInfo.FieldType;

				if (obj is IList list)
				{
					obj = list[index];
					type = type.GetElementType();
				}
			}
			return obj;
		}

		public static SerializedProperty FindProperty(this SerializedProperty property, string propertyName)
		{
			SerializedObject serializedObject = property.serializedObject;
			SerializedProperty? prop = null;
			SerializedProperty result = serializedObject.FindProperty(propertyName);

			foreach (ReadOnlySpan<char> part in property.EnumeratePathParts())
			{
				SerializedProperty? potentialResult = prop?.FindPropertyRelative(propertyName);
				if (potentialResult != null) result = potentialResult;

				string partStr = part.ToString();
				prop = prop == null ? serializedObject.FindProperty(partStr) : prop.FindPropertyRelative(partStr);
			}
			return result;
		}

		public static float GetContentsHeight(this SerializedProperty property, GUIContent label)
		{
			float position = 0;
			float height = EditorGUI.GetPropertyHeight(property, label);
			int first = 0;

			foreach (SerializedProperty prop in property.EnumerateChildren())
			{
				position += (height + EditorGUIUtility.standardVerticalSpacing) * first;
				height = EditorGUI.GetPropertyHeight(prop, label);
				first = 1;
			}
			return position + height;
		}

		/// <summary>
		/// This removes the indentation/foldout of a property and directly draws its contents. EditorGUI version (for custom PropertyDrawers).
		/// </summary>
		/// <returns>Total height of all the drawn properties.</returns>
		public static float DrawContents(this SerializedProperty property, Rect position, GUIContent label)
		{
			float startingPosition = position.y;
			int first = 0;
			string labelText = label.text;

			foreach (SerializedProperty prop in property.EnumerateChildren())
			{
				using (new EditorGUI.DisabledScope("m_Script" == prop.propertyPath))
				{
					label.text = labelText + ": " + prop.name;
					position.y += (position.height + EditorGUIUtility.standardVerticalSpacing) * first;
					position.height = EditorGUI.GetPropertyHeight(prop, label);
					EditorGUI.PropertyField(position, prop, label, includeChildren: true);
					first = 1;
				}
			}
			return position.y + position.height - startingPosition;
		}

		/// <summary>
		/// This removes the indentation/foldout of a property and directly draws its contents. EditorGUILayout version (for custom Editors).
		/// </summary>
		public static void DrawContents(this SerializedProperty property)
		{
			SerializedObject obj = property.serializedObject;
			EditorGUI.BeginChangeCheck();
			obj.UpdateIfRequiredOrScript();

			foreach (SerializedProperty prop in property.EnumerateChildren())
			{
				using (new EditorGUI.DisabledScope("m_Script" == prop.propertyPath))
				{
					EditorGUILayout.PropertyField(prop, true);
				}
			}

			obj.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}

		public static void DrawPropertiesExcluding(this SerializedObject obj, params string[] propertiesToExclude)
		{
			EditorGUI.BeginChangeCheck();
			obj.UpdateIfRequiredOrScript();

			SerializedProperty iterator = obj.GetIterator();
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren))
			{
				enterChildren = false;
				if (Array.IndexOf(propertiesToExclude, iterator.name) < 0)
				{
					using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
					{
						EditorGUILayout.PropertyField(iterator, true);
					}
				}
			}

			obj.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}

		public static SerializedPropertyEnumerator EnumerateChildren(this SerializedProperty property)
		{
			return new SerializedPropertyEnumerator(property);
		}

		public static SerializedPropertyPathEnumerator EnumeratePathParts(this SerializedProperty property)
		{
			return new SerializedPropertyPathEnumerator(property.propertyPath);
		}
	}
}