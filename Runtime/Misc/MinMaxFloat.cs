using System;
using UnityEngine;

namespace Kryz.UnityUtils
{
	[Serializable]
	public struct MinMaxFloat
	{
		public float Min;
		public float Max;

		public static implicit operator Vector2(MinMaxFloat m) => new(m.Min, m.Max);
	}
}