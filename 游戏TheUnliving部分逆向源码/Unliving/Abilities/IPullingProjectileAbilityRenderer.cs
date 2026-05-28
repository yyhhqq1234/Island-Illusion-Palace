using System;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x020003AD RID: 941
	public interface IPullingProjectileAbilityRenderer
	{
		// Token: 0x06001F1F RID: 7967
		void GetRenderers(PullingProjectileAbility ability, out GameObject hookRenderer, out GameObject ropeRenderer);
	}
}
