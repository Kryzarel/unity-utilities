using System;

namespace Kryz.UnityUtils
{
	[Serializable]
	public struct SerializableTuple<T1, T2>
	{
		public T1 Item1;
		public T2 Item2;
	}

	[Serializable]
	public struct SerializableTuple<T1, T2, T3>
	{
		public T1 Item1;
		public T2 Item2;
		public T3 Item3;
	}

	[Serializable]
	public struct SerializableTuple<T1, T2, T3, T4>
	{
		public T1 Item1;
		public T2 Item2;
		public T3 Item3;
		public T4 Item4;
	}
}