using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kryz.UnityUtils
{
	[Serializable]
	public struct SerializeInterface<T> where T : class
	{
		[SerializeField]
		private Object? unityObject;

		public T? Value
		{
			readonly get => unityObject as T;
			set => unityObject = value as Object;
		}

		public SerializeInterface(T value)
		{
			unityObject = value as Object;
		}
	}
}