using UnityEngine;

namespace Kryz.UnityUtils
{
	public class DefaultElementOnAddAttribute : PropertyAttribute
	{
		public DefaultElementOnAddAttribute() : base(applyToCollection: true) { }
	}
}