using System;

namespace Kryz.UnityUtils
{
	[Flags]
	public enum TypeFlags
	{
		Interface = 1 << 0,
		Abstract = 1 << 1,
		Concrete = 1 << 2,
		Everything = ~0,
	}
}