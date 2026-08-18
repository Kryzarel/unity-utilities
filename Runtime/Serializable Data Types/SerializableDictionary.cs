using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kryz.UnityUtils
{
	[Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField] List<SerializableTuple<TKey, TValue>> entries = new();

		public void OnBeforeSerialize()
		{
			// Don't Clear() the List otherwise it becomes impossible to add new entries via the inspector
			int i = 0;
			foreach (KeyValuePair<TKey, TValue> item in this)
			{
				SerializableTuple<TKey, TValue> entry = new() { Item1 = item.Key, Item2 = item.Value };

				if (i < entries.Count)
				{
					entries[i] = entry;
				}
				else
				{
					entries.Add(entry);
				}
				i++;
			}
		}

		public void OnAfterDeserialize()
		{
			Clear();

			foreach (SerializableTuple<TKey, TValue> item in entries)
			{
				TryAdd(item.Item1, item.Item2);
			}
		}
	}
}