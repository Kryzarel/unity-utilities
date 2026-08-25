using System;
using System.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(DefaultElementOnAddAttribute))]
	public class DefaultElementOnAddPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			PropertyField propertyField = new(property);
			propertyField.Bind(property.serializedObject);

			if (property.isArray)
			{
				Type elementType = GetElementType(fieldInfo.FieldType);

				propertyField.RegisterCallbackOnce<GeometryChangedEvent>(evt =>
				{
					ListView listView = ((VisualElement)evt.target).Q<ListView>();
					listView.overridingAddButtonBehavior = (_, _) => AddElement(property, elementType);
				});
			}
			else
			{
				string path = property.propertyPath;
				UnityEngine.Object targetObj = property.serializedObject.targetObject;
				const string attributeName = nameof(DefaultElementOnAddAttribute);
				Debug.LogWarning($"{attributeName} applied to field that is not a collection ({path}). It will have no effect.", targetObj);
			}
			return propertyField;
		}

		private static void AddElement(SerializedProperty property, Type type)
		{
			int index = property.arraySize;

			property.InsertArrayElementAtIndex(index);
			SerializedProperty element = property.GetArrayElementAtIndex(index);

			if (element.propertyType == SerializedPropertyType.ManagedReference)
			{
				element.managedReferenceValue = null;
			}
			else if (!type.IsAbstract)
			{
				element.boxedValue = Activator.CreateInstance(type);
			}
			else
			{
				Debug.LogError($"Can't instantiate object of type {type.FullName}");
			}

			property.serializedObject.ApplyModifiedProperties();
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
	}
}