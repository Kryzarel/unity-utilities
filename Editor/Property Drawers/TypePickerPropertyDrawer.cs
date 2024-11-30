using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(TypePickerAttribute))]
	public class TypePickerPropertyDrawer : PropertyDrawer
	{
		private class CacheData
		{
			public Type[] Types = Array.Empty<Type>();
			public GUIContent[] Contents = Array.Empty<GUIContent>();
		}

		private static readonly Dictionary<string, CacheData> cache = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				Debug.LogError($"{nameof(TypePickerAttribute)} can only be used with {nameof(String)} variables.");
				return;
			}

			if (!cache.TryGetValue(property.propertyPath, out CacheData data))
			{
				TypePickerAttribute pickerAttribute = (TypePickerAttribute)attribute;

				cache[property.propertyPath] = data = new CacheData();
				data.Types = pickerAttribute.Type.FindDerivedTypes(pickerAttribute.Flags);
				data.Contents = new GUIContent[data.Types.Length];

				for (int i = 0; i < data.Contents.Length; i++)
				{
					Type type = data.Types[i];
					data.Contents[i] = new GUIContent(type.Name, type.AssemblyQualifiedName);
				}
			}

			Type selectedType = Type.GetType(property.stringValue);
			int previousIndex = Array.IndexOf(data.Types, selectedType);
			int selectedIndex = EditorGUI.Popup(position, previousIndex, data.Contents);
			property.stringValue = selectedIndex < 0 ? null : data.Types[selectedIndex].AssemblyQualifiedName;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property);
		}
	}
}