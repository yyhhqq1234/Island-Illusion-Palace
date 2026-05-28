using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Buffs;
using Unliving.Mobs.AbilityTriggers;

namespace Unliving.Mobs
{
	// Token: 0x020001B2 RID: 434
	public sealed class BuffsBasedStatus
	{
		// Token: 0x17000230 RID: 560
		// (get) Token: 0x06000C4D RID: 3149 RVA: 0x0002678F File Offset: 0x0002498F
		public bool IsActive
		{
			get
			{
				return this._isActive;
			}
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x00026797 File Offset: 0x00024997
		public BuffsBasedStatus(IBuffsController buffsController, IBuffsGenerator[] buffsGenerators, MobAbilityTriggerBase[] statusActivators)
		{
			this.buffsController = buffsController;
			this.statusActivators = statusActivators;
			this.BuffsGenerators = buffsGenerators;
		}

		// Token: 0x06000C4F RID: 3151 RVA: 0x000267CC File Offset: 0x000249CC
		private void RemoveAllBuffs()
		{
			for (int i = 0; i < this.activeBuffs.Count; i++)
			{
				this.activeBuffs[i].Complete();
			}
			this.activeBuffs.Clear();
		}

		// Token: 0x06000C50 RID: 3152 RVA: 0x0002680C File Offset: 0x00024A0C
		public void Update(MobAbilityTriggerArgs activationArgs)
		{
			this.activatorsUpdateArgs.additionalContext = activationArgs;
			bool flag = true;
			for (int i = 0; i < this.statusActivators.Length; i++)
			{
				MobAbilityTriggerBase mobAbilityTriggerBase = this.statusActivators[i];
				if (mobAbilityTriggerBase.IsSharedExtension && !mobAbilityTriggerBase.IsConditionReached(null, this.activatorsUpdateArgs))
				{
					flag = false;
					break;
				}
			}
			if (this._isActive != flag)
			{
				if (flag)
				{
					for (int j = 0; j < this.BuffsGenerators.Length; j++)
					{
						IBuff buff = this.BuffsGenerators[j].GenerateBuff(this.buffsController.Owner, true);
						if (this.buffsController.AddBuff(buff))
						{
							this.activeBuffs.Add(buff);
						}
					}
				}
				else if (this.activeBuffs.Count != 0)
				{
					this.RemoveAllBuffs();
				}
				this._isActive = flag;
			}
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x000268D0 File Offset: 0x00024AD0
		public void ForceDeactivate()
		{
			this.RemoveAllBuffs();
			this._isActive = false;
		}

		// Token: 0x04000707 RID: 1799
		public readonly IBuffsGenerator[] BuffsGenerators;

		// Token: 0x04000708 RID: 1800
		private readonly MobAbilityTriggerBase[] statusActivators;

		// Token: 0x04000709 RID: 1801
		private readonly List<IBuff> activeBuffs = new List<IBuff>(8);

		// Token: 0x0400070A RID: 1802
		private readonly BaseAbility.UsingArgs activatorsUpdateArgs = new BaseAbility.UsingArgs();

		// Token: 0x0400070B RID: 1803
		private readonly IBuffsController buffsController;

		// Token: 0x0400070C RID: 1804
		private bool _isActive;
	}
}
