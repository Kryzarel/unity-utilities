using System;
using UnityEditor;

namespace Kryz.EditorUtils
{
	public struct SerializedPropertyEnumerator
	{
		private readonly SerializedProperty current;
		private readonly SerializedProperty end;

		private bool firstTime;

		public readonly SerializedProperty Current => current;

		public SerializedPropertyEnumerator(SerializedProperty property)
		{
			current = property.Copy();
			end = property.GetEndProperty();
			firstTime = true;
		}

		public bool MoveNext()
		{
			if (firstTime)
			{
				firstTime = false;
				return current.NextVisible(enterChildren: true);
			}
			return current.NextVisible(enterChildren: false) && !SerializedProperty.EqualContents(current, end);
		}

		public readonly SerializedPropertyEnumerator GetEnumerator() => this;
	}

	public ref struct SerializedPropertyPathEnumerator
	{
		private ReadOnlySpan<char> path;
		private PropertyPathEntry current;

		public readonly ReadOnlySpan<char> OriginalPath;
		public readonly PropertyPathEntry Current => current;

		public SerializedPropertyPathEnumerator(ReadOnlySpan<char> path)
		{
			OriginalPath = path;
			this.path = path;
			current = default;
		}

		public bool MoveNext()
		{
			if (path.IsEmpty)
			{
				current = default;
				return false;
			}

			int indexOfDot = path.IndexOf('.');
			if (indexOfDot < 0)
			{
				current = new PropertyPathEntry(path);
				path = ReadOnlySpan<char>.Empty;
			}
			else
			{
				ReadOnlySpan<char> part = path.Slice(0, indexOfDot);

				if (path.Slice(indexOfDot).StartsWith(".Array.data[", StringComparison.Ordinal))
				{
					int bracket1 = path.IndexOf('[');
					int bracket2 = path.IndexOf(']');
					int index = int.Parse(path.Slice(bracket1 + 1, bracket2 - bracket1 - 1));
					path = path.Slice(bracket2 + 1);
					current = new PropertyPathEntry(part, index);
				}
				else
				{
					path = path.Slice(indexOfDot + 1);
					current = new PropertyPathEntry(part);
				}
			}
			return true;
		}

		public readonly SerializedPropertyPathEnumerator GetEnumerator() => this;
	}

	public readonly ref struct PropertyPathEntry
	{
		public readonly ReadOnlySpan<char> Part;
		public readonly int Index;

		public PropertyPathEntry(ReadOnlySpan<char> part, int index = -1)
		{
			Part = part;
			Index = index;
		}

		// This method allows writing foreach (var (part, index) in property.EnumeratePathParts()) { }
		// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
		public void Deconstruct(out ReadOnlySpan<char> part, out int index)
		{
			part = Part;
			index = Index;
		}

		public static implicit operator ReadOnlySpan<char>(PropertyPathEntry entry) => entry.Part;
	}
}