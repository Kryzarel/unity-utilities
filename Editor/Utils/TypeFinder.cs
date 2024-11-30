using System;
using System.Collections.Generic;
using Kryz.Utils;
using UnityEditor;

namespace Kryz.UnityUtils.Editor
{
	public static class TypeFinder
	{
		public static Type[] FindDerivedTypes(this Type type, TypeFlags typeFlags)
		{
			TypeCache.TypeCollection typeCollection = TypeCache.GetTypesDerivedFrom(type);
			List<Type> types = new(typeCollection.Count);

			for (int i = 0; i < typeCollection.Count; i++)
			{
				Type t = typeCollection[i];
				if (t.IsTypeMatch(typeFlags))
				{
					types.Add(t);
				}
			}
			return types.ToArray();
		}

		private static bool IsTypeMatch(this Type t, TypeFlags typeFlags)
		{
			int flags = (int)typeFlags;

			if (t.IsInterface)
			{
				return flags.HasFlag((int)TypeFlags.Interface);
			}
			else if (t.IsAbstract)
			{
				return flags.HasFlag((int)TypeFlags.Abstract);
			}
			else
			{
				return flags.HasFlag((int)TypeFlags.Concrete);
			}
		}
	}
}