using System;
using Common.Editor;
using Common.PivotGroup;
using Common.UnityExtensions;
using UnityEngine;

namespace Unliving.Mobs.VFX
{
	// Token: 0x02000202 RID: 514
	[Serializable]
	public struct AttachableVisualEffectSpawner
	{
		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x06001143 RID: 4419 RVA: 0x00035D10 File Offset: 0x00033F10
		public bool IsActive
		{
			get
			{
				return this.effectPrefab != null;
			}
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x00035D20 File Offset: 0x00033F20
		public ParticleSystem CreateEffect(TaggedPivotGroup pivotsGroup)
		{
			ParticleSystem particleSystem = this.effectPrefab.InstantiateParticleSystem();
			if (particleSystem != null)
			{
				Transform transform = particleSystem.transform;
				TaggedPivot pivot = pivotsGroup.GetPivot(this.targetPivotTag);
				transform.position = ((pivot != null) ? pivot.WorldPosition : pivotsGroup.GroupTransform.position);
				particleSystem.DestroyAfterEmission(false, true);
			}
			return particleSystem;
		}

		// Token: 0x040009C2 RID: 2498
		public GameObject effectPrefab;

		// Token: 0x040009C3 RID: 2499
		[Tag]
		public string targetPivotTag;
	}
}
