using System;

namespace Kryz.UnityUtils
{
	[Flags]
	public enum MemberFlags
	{
		Derived = 1 << 0,
		Exact = 1 << 1,
		Fields = 1 << 2,
		Properties = 1 << 3,
		Methods = 1 << 4,
		Everything = ~0,
	}
}