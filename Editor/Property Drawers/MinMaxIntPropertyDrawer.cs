using UnityEditor;
using UnityEngine.UIElements;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(MinMaxInt), useForChildren: true)]
	public class MinMaxIntPropertyDrawer : MinMaxPropertyDrawer<int>
	{
		protected override TextValueField<int> CreateField(string label) => new IntegerField(label);

		protected override void MinFieldCallback(ChangeEvent<int> e)
		{
			if (e.newValue > maxProp!.intValue)
			{
				maxProp.intValue = e.newValue;
				maxProp.serializedObject.ApplyModifiedProperties();
			}
		}

		protected override void MaxFieldCallback(ChangeEvent<int> e)
		{
			if (e.newValue < minProp!.intValue)
			{
				minProp.intValue = e.newValue;
				minProp.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}