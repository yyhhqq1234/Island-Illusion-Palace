using System;
using Game.Abilities;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x0200036C RID: 876
	[CreateAssetMenu(fileName = "AbilityBuffsBehaviourExtender", menuName = "Abilities/Controllers/Buffs Behaviour Extender")]
	public sealed class AbilityBuffsBehaviourExtender : AbilityExtensionAssetBase
	{
		// Token: 0x170005FB RID: 1531
		// (get) Token: 0x06001CD9 RID: 7385 RVA: 0x0005B2DA File Offset: 0x000594DA
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001CDA RID: 7386 RVA: 0x0005B2DD File Offset: 0x000594DD
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ability.BuffSent += this.OnAbilityBuffSent;
		}

		// Token: 0x06001CDB RID: 7387 RVA: 0x0005B2F8 File Offset: 0x000594F8
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			SenderDeathBuffCancellationController senderDeathBuffCancellationController = this.buffsCancellationController;
			if (senderDeathBuffCancellationController != null)
			{
				senderDeathBuffCancellationController.UnregisterBuffsSender(ability.Owner);
			}
			ability.BuffSent -= this.OnAbilityBuffSent;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001CDC RID: 7388 RVA: 0x0005B32A File Offset: 0x0005952A
		private void OnAbilityBuffSent(BaseAbility ability, IBuffsController buffsReceiver, IBuff buff)
		{
			if (this.destroyBuffsAlongWithSenders)
			{
				if (this.buffsCancellationController == null)
				{
					this.buffsCancellationController = new SenderDeathBuffCancellationController();
				}
				this.buffsCancellationController.DestroyWithSender(buff);
			}
		}

		// Token: 0x04001056 RID: 4182
		public bool destroyBuffsAlongWithSenders;

		// Token: 0x04001057 RID: 4183
		private SenderDeathBuffCancellationController buffsCancellationController;
	}
}
