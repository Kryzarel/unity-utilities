using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kryz.Utils;

namespace Kryz.UnityUtils.Editor
{
	public static class MemberFinder
	{
		public static MemberInfo[] FindMembers(this Type parentType, BindingFlags bindingFlags, MemberTypes memberTypes, Type type, bool allowDerivedTypes)
		{
			MemberInfo[] members = parentType.FindMembers(memberTypes, bindingFlags, MemberFilter, null);
			return members.Where(m => !m.IsDuplicate(members)).ToArray();

			bool MemberFilter(MemberInfo member, object? filterCriteria)
			{
				return member.IsMemberTypeMatch(memberTypes) && member.IsTypeMatch(type, allowDerivedTypes);
			}
		}

		private static bool IsDuplicate(this MemberInfo member, IReadOnlyList<MemberInfo> memberInfos)
		{
			if (member is MethodInfo methodInfo)
			{
				for (int i = 0; i < memberInfos.Count; i++)
				{
					if (memberInfos[i] is PropertyInfo property && (property.GetMethod == methodInfo || property.SetMethod == methodInfo))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool IsMemberTypeMatch(this MemberInfo member, MemberTypes memberTypes)
		{
			return ((int)member.MemberType).HasFlag((int)memberTypes);
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