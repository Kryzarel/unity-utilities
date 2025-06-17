using System;
using Kryz.Utils;
using Kryz.Collections;
using UnityEditor;

namespace Kryz.UnityUtils.Editor
{
	public static class TypeFinder
	{
		public static Type[] FindDerivedTypes(this Type type, TypeFlags typeFlags)
		{
			TypeCache.TypeCollection typeCollection = TypeCache.GetTypesDerivedFrom(type);
			using PooledList<Type> list = PooledList<Type>.Rent(typeCollection.Count);
			foreach (Type item in typeCollection)
			{
				if (item.IsTypeMatch(typeFlags))
				{
					list.Add(item);
				}
			}
			return list.ToArray();
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