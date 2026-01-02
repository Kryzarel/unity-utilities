using System;
using UnityEngine;

namespace Kryz.UnityUtils
{
	[Serializable]
	public struct MinMaxInt
	{
		public int Min;
		public int Max;

		public static implicit operator Vector2Int(MinMaxInt m) => new(m.Min, m.Max);
	}
}