using UnityEngine;

namespace Kryz.UnityUtils
{
	public class ReadOnlyAttribute : PropertyAttribute
	{
		// Since Unity 2023.3 (aka Unity 6) we FINALLY have support for attributes that apply to the entire collection,
		// rather than only to the individual items inside the collection.
#if UNITY_2023_3_OR_NEWER
		public ReadOnlyAttribute() : base(applyToCollection: true) { }
#endif
	}
}