using System;

namespace Kryz.UnityUtils
{
	[Serializable]
	public struct SerializableTuple<T1, T2>
	{
		public T1 Item1;
		public T2 Item2;
	}
}