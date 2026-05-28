using System;
using Game.Buffs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Abilities.Buffs
{
	// Token: 0x020003D9 RID: 985
	[Serializable]
	public sealed class AbilitiesUsingRestrictionBuffsGenerator : BuffsGeneratorBase
	{
		// Token: 0x170006CF RID: 1743
		// (get) Token: 0x06002184 RID: 8580 RVA: 0x00068D2B File Offset: 0x00066F2B
		public AbilityDescription[] RestrictedAbilitiesDescriptions
		{
			get
			{
				return this._restrictedAbilitiesDescriptions;
			}
		}

		// Token: 0x06002185 RID: 8581 RVA: 0x00068D33 File Offset: 0x00066F33
		protected override BuffBase CreateBuff(object buffSender, BuffBase.InitializationArgs buffInitializationArgs)
		{
			return new AbilitiesUsingRestrictionBuff(base.BuffID, buffSender, buffInitializationArgs, this._restrictedAbilitiesDescriptions, this.tryToBlockAbilitiesImmediately);
		}

		// Token: 0x06002186 RID: 8582 RVA: 0x00068D4E File Offset: 0x00066F4E
		protected override BuffsGeneratorBase.AmountPropertyAccessor[] GetAmountProperties(out int propertiesCount)
		{
			propertiesCount = 0;
			return null;
		}

		// Token: 0x06002187 RID: 8583 RVA: 0x00068D54 File Offset: 0x00066F54
		public AbilitiesUsingRestrictionBuffsGenerator(int buffsID) : base(buffsID)
		{
		}

		// Token: 0x06002188 RID: 8584 RVA: 0x00068D64 File Offset: 0x00066F64
		public AbilitiesUsingRestrictionBuffsGenerator(int buffsID, AbilityDescription[] restrictedAbilitiesDescriptions) : base(buffsID)
		{
			this._restrictedAbilitiesDescriptions = restrictedAbilitiesDescriptions;
		}

		// Token: 0x06002189 RID: 8585 RVA: 0x00068D7C File Offset: 0x00066F7C
		public override IBuffsGenerator Clone()
		{
			AbilitiesUsingRestrictionBuffsGenerator abilitiesUsingRestrictionBuffsGenerator = new AbilitiesUsingRestrictionBuffsGenerator(base.BuffID, this._restrictedAbilitiesDescriptions)
			{
				tryToBlockAbilitiesImmediately = this.tryToBlockAbilitiesImmediately
			};
			base.CloneBaseParameters(abilitiesUsingRestrictionBuffsGenerator);
			return abilitiesUsingRestrictionBuffsGenerator;
		}

		// Token: 0x040014EB RID: 5355
		[SerializeField]
		private AbilityDescription[] _restrictedAbilitiesDescriptions;

		// Token: 0x040014EC RID: 5356
		[FormerlySerializedAs("interruptActiveAbilities")]
		[Tooltip("Если активно, то абилити будут заблокированы сразу. В противном случае они будут блокироваться только на следующей итерации использования.")]
		public bool tryToBlockAbilitiesImmediately = true;
	}
}
