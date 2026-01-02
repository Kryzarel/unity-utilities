using UnityEditor;
using UnityEngine.UIElements;

namespace Kryz.UnityUtils.Editor
{
	[CustomPropertyDrawer(typeof(MinMaxFloat), useForChildren: true)]
	public class MinMaxFloatPropertyDrawer : MinMaxPropertyDrawer<float>
	{
		protected override TextValueField<float> CreateField(string label) => new FloatField(label);

		protected override void MinFieldCallback(ChangeEvent<float> e)
		{
			if (e.newValue > maxProp!.floatValue)
			{
				maxProp.floatValue = e.newValue;
				maxProp.serializedObject.ApplyModifiedProperties();
			}
		}

		protected override void MaxFieldCallback(ChangeEvent<float> e)
		{
			if (e.newValue < minProp!.floatValue)
			{
				minProp.floatValue = e.newValue;
				minProp.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}