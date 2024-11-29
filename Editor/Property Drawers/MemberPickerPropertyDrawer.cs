using System;
using System.Collections.Generic;
using System.Reflection;
using Kryz.Utils;
using UnityEditor;
using UnityEngine;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(MemberPickerAttribute))]
	public class MemberPickerPropertyDrawer : PropertyDrawer
	{
		private class MemberGUIContent : GUIContent
		{
			public readonly MemberInfo MemberInfo;

			public MemberGUIContent(MemberInfo memberInfo)
			{
				MemberInfo = memberInfo;
				text = memberInfo.Name;
			}
		}

		private readonly Dictionary<Type, MemberGUIContent[]> guiContentCache = new();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				Debug.LogError($"{nameof(MemberPickerAttribute)} can only be used with {nameof(String)} variables.");
				return;
			}

			MemberPickerAttribute pickerAttribute = (MemberPickerAttribute)attribute;
			string parentPropertyPath = property.propertyPath[..(property.propertyPath.LastIndexOf('.') + 1)] + pickerAttribute.ParentTypeProperty;
			SerializedProperty parentTypeProperty = property.serializedObject.FindProperty(parentPropertyPath);
			Type? parentType = parentTypeProperty != null ? Type.GetType(parentTypeProperty.stringValue) : null;

			if (parentType == null)
			{
				Debug.LogError($"Failed to find type via property: {pickerAttribute.ParentTypeProperty}.");
				return;
			}

			FieldInfo? field = fieldInfo.DeclaringType?.GetField(pickerAttribute.MemberTypeField, BindingFlags.NonPublic | BindingFlags.Static);
			Type? filterType = field?.GetValue(null) as Type;

			MemberGUIContent[] guiContents = GetGUIContents(pickerAttribute, parentType, filterType);

			int selectedIndex = IndexOf(guiContents, property.stringValue);
			selectedIndex = EditorGUI.Popup(position, selectedIndex, guiContents);
			property.stringValue = selectedIndex >= 0 ? guiContents[selectedIndex].MemberInfo?.Name : null;
		}

		private MemberGUIContent[] GetGUIContents(MemberPickerAttribute pickerAttribute, Type parentType, Type? filterType)
		{
			if (!guiContentCache.TryGetValue(parentType, out MemberGUIContent[] guiContents))
			{
				MemberInfo[] memberInfos = parentType.GetMembers(pickerAttribute.BindingFlags);
				List<MemberGUIContent> list = new(memberInfos.Length);

				foreach (MemberInfo memberInfo in memberInfos)
				{
					if (!((int)memberInfo.MemberType).HasFlag((int)pickerAttribute.MemberTypes))
					{
						continue;
					}

					if (filterType != null)
					{
						Type memberType = GetMemberType(memberInfo);
						if (pickerAttribute.AllowDerived && !filterType.IsAssignableFrom(memberType) || filterType != memberType)
						{
							continue;
						}
					}
					list.Add(new MemberGUIContent(memberInfo));
				}
				/* guiContentCache[parentType] =  */guiContents = list.ToArray();
			}
			return guiContents;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property);
		}

		private static Type GetMemberType(MemberInfo memberInfo)
		{
			return memberInfo switch
			{
				FieldInfo fieldInfo => fieldInfo.FieldType,
				PropertyInfo propertyInfo => propertyInfo.PropertyType,
				MethodInfo methodInfo => methodInfo.ReturnType,
				_ => throw new NotImplementedException(),
			};
		}

		private int IndexOf(MemberGUIContent[] contents, string memberName)
		{
			for (int i = 0; i < contents.Length; i++)
			{
				if (contents[i].MemberInfo.Name.Equals(memberName, StringComparison.Ordinal))
				{
					return i;
				}
			}
			return -1;
		}
	}
}