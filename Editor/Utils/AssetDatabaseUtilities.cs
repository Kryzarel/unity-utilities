using System;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Kryz.UnityUtils.Editor
{
	public static class AssetDatabaseUtilities
	{
		public static T[] FindAssetsOfType<T>(string[]? folders = null) where T : Object
		{
			GUID[] guids = AssetDatabase.FindAssetGUIDs("t:" + typeof(T).Name, folders);
			T[] assets = new T[guids.Length];

			for (int i = 0; i < guids.Length; i++)
			{
				assets[i] = AssetDatabase.LoadAssetByGUID<T>(guids[i]);
			}
			return assets;
		}

		public static GUID[] FindAssetGUIDsOfType<T>(string[]? folders = null) where T : Object
		{
			return AssetDatabase.FindAssetGUIDs("t:" + typeof(T).Name, folders);
		}

		public static string GetPathRelativeToResources(Object asset)
		{
			if (asset == null) return string.Empty;
			string assetPath = AssetDatabase.GetAssetPath(asset);
			return GetPathRelativeToResources(assetPath);
		}

		public static ReadOnlySpan<char> GetPathRelativeToResources(in ReadOnlySpan<char> path)
		{
			const string Resources = "/Resources/";

			if (path.IsEmpty)
				return ReadOnlySpan<char>.Empty;

			int indexOfResources = path.IndexOf(Resources, StringComparison.Ordinal);
			if (indexOfResources < 0)
				return ReadOnlySpan<char>.Empty;

			ReadOnlySpan<char> extension = Path.GetExtension(path);
			int startIndex = indexOfResources + Resources.Length;
			int length = path.Length - startIndex - extension.Length;
			return path.Slice(startIndex, length);
		}

		public static string GetPathRelativeToResources(string path)
		{
			if (string.IsNullOrEmpty(path)) return string.Empty;
			ReadOnlySpan<char> result = GetPathRelativeToResources(path.AsSpan());
			return result.IsEmpty ? string.Empty : new string(result);
		}
	}
}