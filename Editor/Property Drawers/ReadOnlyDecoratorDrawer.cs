using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Kryz.EditorUtils
{
	[CustomPropertyDrawer(typeof(ReadOnlyDecoratorAttribute))]
	public class ReadOnlyDecoratorDrawer : DecoratorDrawer
	{
		private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
		private static readonly Assembly editorAssembly = typeof(DecoratorDrawer).Assembly;

		private static readonly Type propertyHandlerType = editorAssembly.GetType("UnityEditor.PropertyHandler");
		private static readonly PropertyInfo propertyDrawerProperty = propertyHandlerType.GetProperty("propertyDrawer", bindingFlags);
		private static readonly PropertyInfo decoratorDrawersProperty = propertyHandlerType.GetProperty("decoratorDrawers", bindingFlags);

		private static readonly FieldInfo propertyDrawersField = propertyHandlerType.GetField("m_PropertyDrawers", bindingFlags);
		private static readonly FieldInfo nestingLevelField = propertyHandlerType.GetField("m_NestingLevel", bindingFlags);

		private static readonly IDictionary propertyHandlers;

		private bool didInject;

		static ReadOnlyDecoratorDrawer()
		{
			PropertyInfo propertyHandlerCacheProperty = editorAssembly.GetType("UnityEditor.ScriptAttributeUtility").GetProperty("propertyHandlerCache", bindingFlags);
			FieldInfo propertyHandlersField = editorAssembly.GetType("UnityEditor.PropertyHandlerCache").GetField("m_PropertyHandlers", bindingFlags);

			object propertyHandlerCache = propertyHandlerCacheProperty.GetValue(null);
			propertyHandlers = (IDictionary)propertyHandlersField.GetValue(propertyHandlerCache);
		}

		public override bool CanCacheInspectorGUI()
		{
			InjectReadOnlyDrawer();
			return false;
		}

		public override float GetHeight()
		{
			InjectReadOnlyDrawer();
			return 0;
		}

		private void InjectReadOnlyDrawer()
		{
			if (didInject) return;
			didInject = true;

			object? propertyHandler = GetPropertyHandler();
			if (propertyHandler == null) return;

			PropertyDrawer? propertyDrawer = GetPropertyDrawer(propertyHandler);

			if (propertyDrawer is not ReadOnlyPropertyDrawerWrapper)
			{
				SetPropertyDrawer(propertyHandler, new ReadOnlyPropertyDrawerWrapper(propertyDrawer));
			}
		}

		private object? GetPropertyHandler()
		{
			foreach (object? propertyHandler in propertyHandlers.Values)
			{
				List<DecoratorDrawer> decoratorDrawers = (List<DecoratorDrawer>)decoratorDrawersProperty.GetValue(propertyHandler);

				if (decoratorDrawers == null || decoratorDrawers.IndexOf(this) < 0)
				{
					continue;
				}
				return propertyHandler;
			}
			return null;
		}

		private static PropertyDrawer GetPropertyDrawer(object propertyHandler)
		{
			return (PropertyDrawer)propertyDrawerProperty.GetValue(propertyHandler);
		}

		private static void SetPropertyDrawer(object propertyHandler, PropertyDrawer propertyDrawer)
		{
			List<PropertyDrawer> propertyDrawers = (List<PropertyDrawer>)propertyDrawersField.GetValue(propertyHandler);
			if (propertyDrawers == null)
			{
				propertyDrawers = new List<PropertyDrawer>();
				propertyDrawers.Add(propertyDrawer);
				propertyDrawersField.SetValue(propertyHandler, propertyDrawers);
			}
			else
			{
				int index = (int)nestingLevelField.GetValue(propertyHandler);
				propertyDrawers[index] = propertyDrawer;
			}
		}
	}
}