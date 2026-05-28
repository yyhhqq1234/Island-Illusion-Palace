using System;
using Common.UnityExtensions;
using Game.Damage;
using UnityEngine;

namespace Unliving.Voiceover
{
	// Token: 0x02000026 RID: 38
	[CreateAssetMenu(fileName = "MultiKillTrigger", menuName = "Game/VoiceoverTriggers/MultiKillTrigger")]
	public class MultiKillTrigger : BaseVoiceoverTrigger
	{
		// Token: 0x06000161 RID: 353 RVA: 0x00005CF4 File Offset: 0x00003EF4
		public override void InitializeTriggerLogic()
		{
			this.currentPlayer.DamageApplied += this.OnDamageApplied;
			if (this.AllowMobsDamage)
			{
				this.currentPlayer.GroupMobDamageApplied += this.OnDamageApplied;
			}
			this.currentKillsCount = 0;
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00005D34 File Offset: 0x00003F34
		private void OnDamageApplied(IDamageable damagedObject, float damagedAmount)
		{
			if (damagedObject.IsAlive)
			{
				return;
			}
			if (this.currentKillsCount == 0 || Time.time - this.lastKillTime < this.KillTimeout)
			{
				this.currentKillsCount++;
				this.lastKillTime = Time.time;
				if (this.currentKillsCount >= this.TargetKillsCount)
				{
					base.QueueTrigger(this.Messages);
					this.currentKillsCount = 0;
					return;
				}
			}
			else
			{
				this.currentKillsCount = 0;
			}
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00005DA8 File Offset: 0x00003FA8
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.DamageApplied -= this.OnDamageApplied;
				if (this.AllowMobsDamage)
				{
					this.currentPlayer.GroupMobDamageApplied -= this.OnDamageApplied;
				}
			}
		}

		// Token: 0x040000A7 RID: 167
		[Header("Пул сообщений")]
		public VoiceoverMessage[] Messages;

		// Token: 0x040000A8 RID: 168
		[Header("Время между фрагами")]
		public float KillTimeout = 3f;

		// Token: 0x040000A9 RID: 169
		[Header("Количество фрагов для достижения цели")]
		public int TargetKillsCount = 3;

		// Token: 0x040000AA RID: 170
		[Header("Учитывать фраги мобов игрока")]
		public bool AllowMobsDamage;

		// Token: 0x040000AB RID: 171
		[NonSerialized]
		private int currentKillsCount = 3;

		// Token: 0x040000AC RID: 172
		[NonSerialized]
		private float lastKillTime;
	}
}
