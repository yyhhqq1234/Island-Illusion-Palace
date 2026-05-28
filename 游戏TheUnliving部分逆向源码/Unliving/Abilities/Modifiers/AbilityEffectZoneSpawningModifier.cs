using System;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003BC RID: 956
	[Serializable]
	public sealed class AbilityEffectZoneSpawningModifier : AbilityBuffsBasedModifierBase
	{
		// Token: 0x17000696 RID: 1686
		// (get) Token: 0x06002050 RID: 8272 RVA: 0x00065D07 File Offset: 0x00063F07
		public override bool IsActive
		{
			get
			{
				return base.IsActive && this.baseZoneRange > 0f;
			}
		}

		// Token: 0x06002051 RID: 8273 RVA: 0x00065D20 File Offset: 0x00063F20
		protected override void OnUse(AbilityModifierUsingArgs usingArgs)
		{
			MonoBehaviour ownerBehaviour = usingArgs.targetAbility.OwnerBehaviour;
			float range = this.baseZoneRange + Mathf.Max(this.baseZoneRangeAddition * (float)usingArgs.modifiersUsingCount, 0f);
			float buffsDuration = base.GetBuffsDuration(usingArgs);
			Vector3 targetPosition = usingArgs.targetsInfo.targetPosition;
			BuffsGeneratorBuilderAsset.ReferenceBase[] buffsGenerators = this.buffsGenerators;
			IBuffsGenerator[] array;
			buffsGenerators.Instantiate(out array);
			for (int i = 0; i < array.Length; i++)
			{
				base.SetBuffsPowerGainsActive(array[i], usingArgs, true);
				base.SetBuffsStatsActive(array[i], usingArgs, true);
			}
			AbilityEffectZone abilityEffectZone = AbilityEffectZone.Create(array, targetPosition, ownerBehaviour, buffsDuration, range);
			if (abilityEffectZone != null)
			{
				abilityEffectZone.affectableObjectsLayers = (usingArgs.targetAbility.ValidObjectLayers | this.additionalAffectableObjectsLayers);
				abilityEffectZone.visualEffectPrefab = this.zoneVFXPrefab;
			}
		}

		// Token: 0x06002052 RID: 8274 RVA: 0x00065DF7 File Offset: 0x00063FF7
		protected override void OnReset(AbilityModifierUsingArgs usingArgs)
		{
		}

		// Token: 0x06002053 RID: 8275 RVA: 0x00065DFC File Offset: 0x00063FFC
		public AbilityEffectZoneSpawningModifier(AbilityEffectZoneSpawningModifier modifierPrototype) : base(modifierPrototype)
		{
			this.additionalAffectableObjectsLayers = modifierPrototype.additionalAffectableObjectsLayers;
			this.baseZoneRange = modifierPrototype.baseZoneRange;
			this.baseZoneRangeAddition = modifierPrototype.baseZoneRangeAddition;
			this.zoneVFXPrefab = modifierPrototype.zoneVFXPrefab;
		}

		// Token: 0x06002054 RID: 8276 RVA: 0x00065E4B File Offset: 0x0006404B
		public override AbilityModifierBase Clone()
		{
			return new AbilityEffectZoneSpawningModifier(this);
		}

		// Token: 0x04001450 RID: 5200
		public LayerMask additionalAffectableObjectsLayers;

		// Token: 0x04001451 RID: 5201
		public float baseZoneRange = 5f;

		// Token: 0x04001452 RID: 5202
		public float baseZoneRangeAddition;

		// Token: 0x04001453 RID: 5203
		public GameObject zoneVFXPrefab;
	}
}
