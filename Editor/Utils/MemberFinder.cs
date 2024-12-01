using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kryz.UnityUtils.Editor
{
	public static class MemberFinder
	{
		public static MemberInfo[] FindMembers(this Type type, BindingFlags bindingFlags, MemberTypes memberTypes, Type filterType, bool allowDerived)
		{
			MemberInfo[] members = type.FindMembers(memberTypes, bindingFlags, MemberFilter, new Tuple<Type, bool>(filterType, allowDerived));
			return members.Where(m => !m.IsDuplicate(members)).ToArray();
		}

		private static bool MemberFilter(MemberInfo member, object? filterCriteria)
		{
			return filterCriteria is Tuple<Type, bool> data && member.IsTypeMatch(data.Item1, data.Item2);
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

		private static bool IsTypeMatch(this MemberInfo member, Type type, bool allowDerived)
		{
			return allowDerived ? type.IsAssignableFrom(member.GetMemberType()) : type == member.GetMemberType();
		}

		private static Type GetMemberType(this MemberInfo memberInfo)
		{
			return memberInfo switch
			{
				FieldInfo fieldInfo => fieldInfo.FieldType,
				PropertyInfo propertyInfo => propertyInfo.PropertyType,
				MethodInfo methodInfo => methodInfo.ReturnType,
				_ => throw new NotImplementedException(),
			};
		}
	}
}