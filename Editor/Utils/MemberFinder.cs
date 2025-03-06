using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kryz.Utils;

namespace Kryz.UnityUtils.Editor
{
	public static class MemberFinder
	{
		private class FilterData
		{
			public readonly Type Type;
			public readonly bool AllowDerived;
			public readonly bool AllowImplictCast;

			public FilterData(Type Type, bool allowDerived, bool allowImplictCast)
			{
				this.Type = Type;
				AllowDerived = allowDerived;
				AllowImplictCast = allowImplictCast;
			}
		}

		private static readonly MemberFilter filterFunc = MemberFilter;

		public static MemberInfo[] FindMembers(this Type type, BindingFlags bindingFlags, MemberTypes memberTypes, Type filterType, bool allowDerived, bool allowImplictCast)
		{
			MemberInfo[] members = type.FindMembers(memberTypes, bindingFlags, filterFunc, new FilterData(filterType, allowDerived, allowImplictCast));
			return members.Where(m => !m.IsDuplicate(members)).ToArray();
		}

		private static bool MemberFilter(MemberInfo member, object? filterCriteria)
		{
			if (filterCriteria is FilterData data)
			{
				Type memberType = member.GetMemberType();
				return memberType.IsTypeMatch(data.Type, data.AllowDerived) || memberType.IsImplicitlyCastable(data.Type, data.AllowImplictCast);
			}
			return false;
		}

		private static bool IsDuplicate(this MemberInfo member, IReadOnlyList<MemberInfo> memberInfos)
		{
			if (member is MethodInfo methodInfo)
			{
				foreach (MemberInfo m in memberInfos)
				{
					if (m is PropertyInfo property && (property.GetMethod == methodInfo || property.SetMethod == methodInfo))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool IsTypeMatch(this Type memberType, Type type, bool allowDerived)
		{
			return allowDerived ? type.IsAssignableFrom(memberType) : type == memberType;
		}

		private static bool IsImplicitlyCastable(this Type memberType, Type type, bool allowImplictCast)
		{
			return allowImplictCast && memberType.IsImplicitlyCastableTo(type);
		}
	}
}