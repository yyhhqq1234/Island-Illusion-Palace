using System;
using Common.Factories;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Factories;
using Unliving.LeveledItems;

namespace Unliving.Abilities
{
	// Token: 0x0200039D RID: 925
	public sealed class AbilityFactoryArgs : MultiRepresentationObjectInstantiator.IArgs, IBaseObjectDescription, IItemLevelProvider
	{
		// Token: 0x17000625 RID: 1573
		// (get) Token: 0x06001E7F RID: 7807 RVA: 0x00060AE1 File Offset: 0x0005ECE1
		// (set) Token: 0x06001E80 RID: 7808 RVA: 0x00060AE9 File Offset: 0x0005ECE9
		int IBaseObjectDescription.ObjectID
		{
			get
			{
				return (int)this.abilityID;
			}
			set
			{
				this.abilityID = (AbilityID)value;
			}
		}

		// Token: 0x17000626 RID: 1574
		// (get) Token: 0x06001E81 RID: 7809 RVA: 0x00060AF2 File Offset: 0x0005ECF2
		// (set) Token: 0x06001E82 RID: 7810 RVA: 0x00060AFA File Offset: 0x0005ECFA
		MultiRepresentationObjectInstantiator.ObjectType MultiRepresentationObjectInstantiator.IArgs.Type
		{
			get
			{
				return this.objectType;
			}
			set
			{
				this.objectType = value;
			}
		}

		// Token: 0x17000627 RID: 1575
		// (get) Token: 0x06001E83 RID: 7811 RVA: 0x00060B03 File Offset: 0x0005ED03
		Vector3 MultiRepresentationObjectInstantiator.IArgs.SpawnPosition
		{
			get
			{
				return this.spawnPosition;
			}
		}

		// Token: 0x17000628 RID: 1576
		// (get) Token: 0x06001E84 RID: 7812 RVA: 0x00060B10 File Offset: 0x0005ED10
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.abilityLevel;
			}
		}

		// Token: 0x06001E85 RID: 7813 RVA: 0x00060B18 File Offset: 0x0005ED18
		public void SetLevelFromAbility(BaseAbility ability)
		{
			int num;
			if (!ability.TryGetAbilityLevel(out num, 1))
			{
				return;
			}
			this.abilityLevel = num;
		}

		// Token: 0x06001E86 RID: 7814 RVA: 0x00060B38 File Offset: 0x0005ED38
		public void Update(AbilityInfo abilityDescription)
		{
			this.abilityID = abilityDescription.abilityID;
			this.abilityLevel = abilityDescription.abilityLevel;
			this.specialBehaviourDescription = abilityDescription.specialBehaviourDescription;
		}

		// Token: 0x04001130 RID: 4400
		public BaseAbility abilityPrototype;

		// Token: 0x04001131 RID: 4401
		public AbilityID abilityID;

		// Token: 0x04001132 RID: 4402
		public int abilityLevel = 1;

		// Token: 0x04001133 RID: 4403
		public int? targetsLayersOverride;

		// Token: 0x04001134 RID: 4404
		public float? rangeOverride;

		// Token: 0x04001135 RID: 4405
		public float? reloadingTimeOverride;

		// Token: 0x04001136 RID: 4406
		public float? reloadingProgressOverride;

		// Token: 0x04001137 RID: 4407
		public bool canGenerateBuffs = true;

		// Token: 0x04001138 RID: 4408
		public BaseAbility parentAbility;

		// Token: 0x04001139 RID: 4409
		public object abilityOwner;

		// Token: 0x0400113A RID: 4410
		public object abilityEffectSender;

		// Token: 0x0400113B RID: 4411
		public MultiRepresentationObjectInstantiator.ObjectType objectType;

		// Token: 0x0400113C RID: 4412
		public Vector2 spawnPosition;

		// Token: 0x0400113D RID: 4413
		public AbilitySpecialBehaviourDescription specialBehaviourDescription;

		// Token: 0x0400113E RID: 4414
		public bool preventRandomSpecialBehaviourGeneration;
	}
}
