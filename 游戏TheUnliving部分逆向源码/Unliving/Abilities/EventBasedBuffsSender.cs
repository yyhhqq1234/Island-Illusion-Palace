using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Buffs;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000399 RID: 921
	public sealed class EventBasedBuffsSender : MonoBehaviour
	{
		// Token: 0x17000622 RID: 1570
		// (get) Token: 0x06001E5F RID: 7775 RVA: 0x0006029C File Offset: 0x0005E49C
		// (set) Token: 0x06001E60 RID: 7776 RVA: 0x000602A4 File Offset: 0x0005E4A4
		public BuffsGeneratorBuilderAsset.Reference[] BuffsGeneratorsAssets
		{
			get
			{
				return this.buffsGeneratorsAssets;
			}
			set
			{
				this.buffsGeneratorsAssets = value;
			}
		}

		// Token: 0x06001E61 RID: 7777 RVA: 0x000602AD File Offset: 0x0005E4AD
		private bool IsValidTarget(Component target)
		{
			return this.sendBuffsToThisObject || this.buffsTargetsDescription.IsBlank() || this.buffsTargetsDescription.IsMatch(target.CastOrGetComponent<BaseGameMob>());
		}

		// Token: 0x06001E62 RID: 7778 RVA: 0x000602D8 File Offset: 0x0005E4D8
		private void SendBuffs(Component target)
		{
			IBuffsController buffsController;
			if (!this.sendBuffsToThisObject)
			{
				IBuffableObject buffableObject = target.CastOrGetComponent<IBuffableObject>();
				buffsController = ((buffableObject != null) ? buffableObject.BuffsController : null);
			}
			else
			{
				buffsController = this.selfBuffsController;
			}
			IBuffsController buffsController2 = buffsController;
			if (buffsController2 == null)
			{
				return;
			}
			for (int i = 0; i < this.buffsGenerators.Length; i++)
			{
				buffsController2.AddBuff(this.buffsGenerators[i].GenerateBuff(this, false));
			}
		}

		// Token: 0x06001E63 RID: 7779 RVA: 0x00060338 File Offset: 0x0005E538
		private void OnHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (args.IsDamage && args.Amount != 0f)
			{
				Component component = sender as Component;
				if (component != null)
				{
					if (!this.IsValidTarget(component))
					{
						return;
					}
					int instanceID = component.gameObject.GetInstanceID();
					bool flag = this.damagers.Add(instanceID);
					EventBasedBuffsSender.Event @event = this.eventType;
					if (@event != EventBasedBuffsSender.Event.AnyDamage)
					{
						if (@event == EventBasedBuffsSender.Event.FirstDamage)
						{
							if (flag)
							{
								this.SendBuffs(component);
							}
						}
					}
					else
					{
						this.SendBuffs(component);
					}
					this.damagers.Add(instanceID);
				}
			}
		}

		// Token: 0x06001E64 RID: 7780 RVA: 0x000603B8 File Offset: 0x0005E5B8
		private void Start()
		{
			BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.buffsGeneratorsAssets;
			generatorsBuilders.Instantiate(out this.buffsGenerators);
			IBuffableObject buffableObject;
			if (this.sendBuffsToThisObject && base.TryGetComponent<IBuffableObject>(out buffableObject))
			{
				this.selfBuffsController = buffableObject.BuffsController;
			}
			if (base.TryGetComponent<IDamageable>(out this.hitPointsController))
			{
				this.hitPointsController.HitPointsChanged += this.OnHitPointsChanged;
			}
		}

		// Token: 0x06001E65 RID: 7781 RVA: 0x0006041B File Offset: 0x0005E61B
		private void OnDestroy()
		{
			if (this.hitPointsController != null)
			{
				this.hitPointsController.HitPointsChanged -= this.OnHitPointsChanged;
			}
		}

		// Token: 0x0400111B RID: 4379
		[SerializeField]
		private BuffsGeneratorBuilderAsset.Reference[] buffsGeneratorsAssets;

		// Token: 0x0400111C RID: 4380
		public EventBasedBuffsSender.Event eventType;

		// Token: 0x0400111D RID: 4381
		public bool sendBuffsToThisObject;

		// Token: 0x0400111E RID: 4382
		public GameMobDescription buffsTargetsDescription = GameMobDescription.BlankDescription;

		// Token: 0x0400111F RID: 4383
		private readonly HashSet<int> damagers = new HashSet<int>();

		// Token: 0x04001120 RID: 4384
		private IBuffsController selfBuffsController;

		// Token: 0x04001121 RID: 4385
		private IBuffsGenerator[] buffsGenerators;

		// Token: 0x04001122 RID: 4386
		private IDamageable hitPointsController;

		// Token: 0x02000578 RID: 1400
		public enum Event
		{
			// Token: 0x04001C65 RID: 7269
			None,
			// Token: 0x04001C66 RID: 7270
			AnyDamage,
			// Token: 0x04001C67 RID: 7271
			FirstDamage
		}
	}
}
