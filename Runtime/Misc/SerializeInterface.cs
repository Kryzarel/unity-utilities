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

		private T? value;

		public T? Value
		{
			get
			{
				if (value == null || (object)value != unityObject)
				{
					value = unityObject as T;
				}
				return value;
			}

			set
			{
				unityObject = value as Object;
				this.value = unityObject as T;
			}
		}

		public SerializeInterface(T value)
		{
			unityObject = value as Object;
			this.value = unityObject as T;
		}
	}
}