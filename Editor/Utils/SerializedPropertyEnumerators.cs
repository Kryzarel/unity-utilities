using UnityEditor;

namespace Kryz.UnityUtils.Editor
{
	public struct SerializedPropertyEnumerator
	{
		private readonly SerializedProperty current;
		private readonly SerializedProperty end;

		private bool enterChildren;

		public readonly SerializedProperty Current => current;

		public SerializedPropertyEnumerator(SerializedProperty property)
		{
			current = property.Copy();
			end = property.GetEndProperty();
			enterChildren = true;
		}

		public bool MoveNext()
		{
			bool success = current.NextVisible(enterChildren) && !SerializedProperty.EqualContents(current, end);
			enterChildren = false;
			return success;
		}

		public readonly void Dispose()
		{
			current.Dispose();
			end.Dispose();
		}

		public readonly SerializedPropertyEnumerator GetEnumerator() => this;
	}
}