using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Kryz.Utils;

namespace Kryz.UnityUtils.Editor
{
	public static class SerializedPropertyExtensions
	{
		public readonly struct SerializedPropertyInfo
		{
			public readonly SerializedProperty Property;
			public readonly object Value;
			public readonly Type Type;
			public readonly FieldInfo? FieldInfo;
			public readonly IList? List;
			public readonly int Index;

			public SerializedPropertyInfo(SerializedProperty property, object value, Type type, FieldInfo? fieldInfo, IList? list, int index)
			{
				Property = property;
				Value = value;
				Type = type;
				FieldInfo = fieldInfo;
				List = list;
				Index = index;
			}
		}

		/// <summary>
		/// Unity can be really stupid sometimes... It doesn't have a way to retrieve the value of a SerializedProperty when the value is a regular C# class.
		/// "objectReferenceValue" only works if the value is a Unity.Object. Not even "boxedValue", introduced in version 2022.1 works for this.
		/// </summary>
		public static object GetValue(this SerializedProperty property) => property.GetInfo().Value;
		public static Type GetPropertyType(this SerializedProperty property) => property.GetInfo().Type;
		public static FieldInfo? GetFieldInfo(this SerializedProperty property) => property.GetInfo().FieldInfo;

		public static SerializedPropertyInfo GetInfo(this SerializedProperty property)
		{
			object obj = property.serializedObject.targetObject;
			Type type = obj.GetType();
			FieldInfo? fieldInfo = null;
			IList? list = null;
			int index = 0;

			foreach (string part in property.propertyPath.Replace(".Array.data[", "[").Split('.'))
			{
				int bracketIndex = part.IndexOf('[');
				string fieldName = bracketIndex < 0 ? part : part[..bracketIndex];

				const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				fieldInfo = type.GetFieldInSubclasses(fieldName, bindingFlags);
				if (fieldInfo == null) break;

				obj = fieldInfo.GetValue(obj);
				type = fieldInfo.FieldType;

				if (obj is IList l)
				{
					ReadOnlySpan<char> span = part.AsSpan();
					span = span[(bracketIndex + 1)..span.IndexOf(']')];

					if (int.TryParse(span, out index) && index >= 0)
					{
						list = l;
						obj = l[index];
						type = type.IsGenericType ? type.GenericTypeArguments[0] : type.GetElementType();
					}
				}
			}
			return new SerializedPropertyInfo(property, obj, type, fieldInfo, list, index);
		}

		public static bool HasAttribute(this SerializedProperty property, Type attributeType, bool inherit = true)
		{
			FieldInfo? fieldInfo = property.GetFieldInfo();
			return fieldInfo != null && fieldInfo.IsDefined(attributeType, inherit);
		}

		public static bool HasAttribute<T>(this SerializedProperty property, bool inherit = true)
		{
			return property.HasAttribute(typeof(T), inherit);
		}

		public static SerializedProperty FindProperty(this SerializedProperty property, string propertyName)
		{
			SerializedObject serializedObject = property.serializedObject;
			SerializedProperty? prop = null;
			SerializedProperty result = serializedObject.FindProperty(propertyName);

			foreach (string part in property.propertyPath.Replace(".Array.data[", "[").Split('.'))
			{
				SerializedProperty? potentialResult = prop?.FindPropertyRelative(propertyName);
				if (potentialResult != null) result = potentialResult;

				int bracketIndex = part.IndexOf('[');
				string path = bracketIndex < 0 ? part : part[..bracketIndex];
				prop = prop == null ? serializedObject.FindProperty(path) : prop.FindPropertyRelative(path);
			}
			return result;
		}

		public static float GetContentsHeight(this SerializedProperty property, GUIContent label)
		{
			float position = 0;
			foreach (SerializedProperty prop in property.EnumerateChildren())
			{
				position += EditorGUI.GetPropertyHeight(prop, label);
			}
			return position;
		}

		/// <summary>
		/// This removes the indentation/foldout of a property and directly draws its contents. EditorGUI version (for custom PropertyDrawers).
		/// </summary>
		/// <returns>Total height of all the drawn properties.</returns>
		public static float DrawContents(this SerializedProperty serializedProperty, Rect position, GUIContent label, Func<SerializedProperty, DrawType>? drawFunc = null)
		{
			float startingPosition = position.y;

			foreach (SerializedProperty property in serializedProperty.EnumerateChildren())
			{
				DrawType visibility = drawFunc?.Invoke(property) ?? DrawType.Draw;
				if (visibility == DrawType.DontDraw) { continue; }

				bool isScript = property.propertyPath.Equals("m_Script", StringComparison.Ordinal);
				using (new EditorGUI.DisabledScope(isScript || visibility == DrawType.Disable))
				{
					label.text = property.displayName;
					position.height = EditorGUI.GetPropertyHeight(property, label);
					position.y += position.height;
					EditorGUI.PropertyField(position, property, label, includeChildren: true);
				}
			}
			return position.y + position.height - startingPosition;
		}

		/// <summary>
		/// This removes the indentation/foldout of a property and directly draws its contents. EditorGUILayout version (for custom Editors).
		/// </summary>
		public static void DrawContents(this SerializedProperty serializedProperty, Func<SerializedProperty, DrawType>? drawFunc = null)
		{
			foreach (SerializedProperty property in serializedProperty.EnumerateChildren())
			{
				DrawType visibility = drawFunc?.Invoke(property) ?? DrawType.Draw;
				if (visibility == DrawType.DontDraw) { continue; }

				bool isScript = property.propertyPath.Equals("m_Script", StringComparison.Ordinal);
				using (new EditorGUI.DisabledScope(isScript || visibility == DrawType.Disable))
				{
					EditorGUILayout.PropertyField(property, true);
				}
			}
		}

		public static void DrawProperties(this SerializedObject obj, Func<SerializedProperty, DrawType>? drawFunc = null)
		{
			EditorGUI.BeginChangeCheck();
			obj.UpdateIfRequiredOrScript();

			SerializedProperty iterator = obj.GetIterator();
			iterator.Next(true);
			iterator.DrawContents(drawFunc);

			obj.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}

		public static void DrawPropertiesExcluding(this SerializedObject obj, params string[] propertiesToExclude)
		{
			obj.DrawProperties(property => Array.IndexOf(propertiesToExclude, property.name) < 0 ? DrawType.Disable : DrawType.Draw);
		}

		public static SerializedPropertyEnumerator EnumerateChildren(this SerializedProperty property)
		{
			return new SerializedPropertyEnumerator(property);
		}
	}
}