using System;
using System.Collections.Generic;
using Kryz.Utils;
using UnityEditor;
using UnityEngine;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(TypePickerAttribute))]
	public class TypePickerPropertyDrawer : PropertyDrawer
	{
		private class TypeGUIContent : GUIContent
		{
			public readonly Type? Type;

			public TypeGUIContent(Type? type)
			{
				Type = type;
				text = type == null ? "Null" : type.Name;
				tooltip = type?.AssemblyQualifiedName;
			}
		}

		private static readonly Dictionary<Type, TypeGUIContent[]> guiContentCache = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				Debug.LogError($"{nameof(TypePickerAttribute)} can only be used with {nameof(String)} variables.");
				return;
			}

			TypePickerAttribute pickerAttribute = (TypePickerAttribute)attribute;
			Type type = pickerAttribute.Type;
			int flags = (int)pickerAttribute.Flags;

			if (!guiContentCache.TryGetValue(type, out TypeGUIContent[] guiContent))
			{
				TypeCache.TypeCollection typeCollection = TypeCache.GetTypesDerivedFrom(type);
				List<TypeGUIContent> list = new(typeCollection.Count);
				// list.Add(new TypeGUIContent(null));

				for (int i = 0; i < typeCollection.Count; i++)
				{
					Type t = typeCollection[i];
					if (t.IsInterface && !flags.HasFlag((int)TypeFlags.Interface))
					{
						continue;
					}
					else if (!t.IsInterface && t.IsAbstract && !flags.HasFlag((int)TypeFlags.Abstract))
					{
						continue;
					}
					else if (!t.IsInterface && !t.IsAbstract && !flags.HasFlag((int)TypeFlags.Concrete))
					{
						continue;
					}
					list.Add(new TypeGUIContent(t));
				}
				guiContentCache[type] = guiContent = list.ToArray();
			}

			Type selectedType = Type.GetType(property.stringValue);
			int selectedIndex = IndexOf(guiContent, selectedType);
			selectedIndex = EditorGUI.Popup(position, selectedIndex, guiContent);
			property.stringValue = selectedIndex >= 0 ? guiContent[selectedIndex].Type?.AssemblyQualifiedName : null;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property);
		}

		private int IndexOf(TypeGUIContent[] contents, Type type)
		{
			for (int i = 0; i < contents.Length; i++)
			{
				if (contents[i].Type == type)
				{
					return i;
				}
			}
			return -1;
		}
	}
}