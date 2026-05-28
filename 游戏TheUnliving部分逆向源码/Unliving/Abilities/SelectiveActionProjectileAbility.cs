using System;
using System.Collections.Generic;
using Common;
using Game.Abilities;
using Game.Damage.Projectiles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Abilities
{
	// Token: 0x020003B5 RID: 949
	[CreateAssetMenu(fileName = "SelectiveActionProjectileAbility", menuName = "Abilities/Selective Action Projectile Ability")]
	public sealed class SelectiveActionProjectileAbility : ProjectileAbility, ICompositeAbility, IAbility, IDestroyable
	{
		// Token: 0x17000663 RID: 1635
		// (get) Token: 0x06001FA9 RID: 8105 RVA: 0x00063898 File Offset: 0x00061A98
		// (set) Token: 0x06001FAA RID: 8106 RVA: 0x0006389F File Offset: 0x00061A9F
		public override float UsingDuration
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		// Token: 0x17000664 RID: 1636
		// (get) Token: 0x06001FAB RID: 8107 RVA: 0x000638A1 File Offset: 0x00061AA1
		// (set) Token: 0x06001FAC RID: 8108 RVA: 0x000638A9 File Offset: 0x00061AA9
		public SelectiveActionProjectileAbility.SpecialAbilityInfo[] SpecialCasesAbilities
		{
			get
			{
				return this.specialCasesAbilities;
			}
			set
			{
				this.specialCasesAbilities = value;
			}
		}

		// Token: 0x17000665 RID: 1637
		// (get) Token: 0x06001FAD RID: 8109 RVA: 0x000638B4 File Offset: 0x00061AB4
		public override bool InUse
		{
			get
			{
				if (this.specialCasesAbilities != null)
				{
					for (int i = 0; i < this.specialCasesAbilities.Length; i++)
					{
						if (this.specialCasesAbilities[i].InProgress())
						{
							return true;
						}
					}
				}
				return base.InUse;
			}
		}

		// Token: 0x17000666 RID: 1638
		// (get) Token: 0x06001FAE RID: 8110 RVA: 0x000638F7 File Offset: 0x00061AF7
		IList<IAbility> ICompositeAbility.ChildAbilities
		{
			get
			{
				return this.childAbilitiesImpl ?? this.specialCasesAbilities.InitializeChildAbilitiesAsPrototypes(ref this.childAbilitiesImpl);
			}
		}

		// Token: 0x17000667 RID: 1639
		// (get) Token: 0x06001FAF RID: 8111 RVA: 0x00063914 File Offset: 0x00061B14
		bool ICompositeAbility.WillUseChildAbilitiesSequentially
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x00063918 File Offset: 0x00061B18
		protected override bool PrepareProjectileEffectUsing(ProjectileHitInfo projectileHitInfo, BaseAbility.UsingArgs projectileEffectUsingArgs, out bool sendBuffs)
		{
			if (this.specialCasesAbilities != null)
			{
				BaseProjectile.HitInfo hitInfo = projectileHitInfo as BaseProjectile.HitInfo;
				if (hitInfo != null && hitInfo.hitReceiver != null)
				{
					int targetLayerMask = 1 << hitInfo.hitReceiver.gameObject.layer;
					for (int i = 0; i < this.specialCasesAbilities.Length; i++)
					{
						SelectiveActionProjectileAbility.SpecialAbilityInfo specialAbilityInfo = this.specialCasesAbilities[i];
						if (specialAbilityInfo.HasTargetLayers(targetLayerMask))
						{
							sendBuffs = false;
							this.selectedSpecialAbility = specialAbilityInfo.AbilityInstance;
							this.selectedSpecialAbility.Owner = base.Owner;
							this.specialAbilityUsingArgs.targetObject = hitInfo.hitReceiver;
							this.specialAbilityUsingArgs.targetPosition = hitInfo.point;
							this.OnProjectileUsingPrepared(this.specialAbilityUsingArgs, projectileHitInfo);
							return false;
						}
					}
				}
			}
			return base.PrepareProjectileEffectUsing(projectileHitInfo, projectileEffectUsingArgs, out sendBuffs);
		}

		// Token: 0x06001FB1 RID: 8113 RVA: 0x000639EC File Offset: 0x00061BEC
		protected override void OnProjectileHit(ProjectileHitInfo hitArgs)
		{
			if (this.selectedSpecialAbility != null)
			{
				this.selectedSpecialAbility.UsingDuration = 0f;
				this.selectedSpecialAbility.PrepTime = 0f;
				this.selectedSpecialAbility.MaxUsingCount = 0;
				this.selectedSpecialAbility.HasInfiniteUsingDuration = false;
				this.selectedSpecialAbility.UsingDelay = 0f;
				this.selectedSpecialAbility.Activate(this.specialAbilityUsingArgs);
				this.selectedSpecialAbility.UpdateAbility(Time.deltaTime);
				this.selectedSpecialAbility.ForceReload();
				this.selectedSpecialAbility = null;
				this.specialAbilityUsingArgs.Reset();
				return;
			}
			base.OnProjectileHit(hitArgs);
		}

		// Token: 0x06001FB2 RID: 8114 RVA: 0x00063A98 File Offset: 0x00061C98
		protected override void OnInitialize(object context)
		{
			if (this.specialCasesAbilities != null && this.specialCasesAbilities.Length != 0)
			{
				if (SelectiveActionProjectileAbility.abilitiesFactory == null)
				{
					SelectiveActionProjectileAbility.abilitiesFactory = (context as IGameAbilitiesFactory);
				}
				if (this.childAbilitiesImpl == null)
				{
					this.childAbilitiesImpl = new IAbility[this.specialCasesAbilities.Length];
				}
				AbilityFactoryArgs abilitiesFactoryArgs = new AbilityFactoryArgs
				{
					abilityID = (AbilityID)this.ID,
					reloadingTimeOverride = new float?(0f),
					canGenerateBuffs = true,
					abilityOwner = base.Owner,
					parentAbility = this
				};
				for (int i = 0; i < this.specialCasesAbilities.Length; i++)
				{
					ref SelectiveActionProjectileAbility.SpecialAbilityInfo ptr = ref this.specialCasesAbilities[i];
					ptr.Initialize(this, abilitiesFactoryArgs);
					this.childAbilitiesImpl[i] = ptr.AbilityInstance;
					if (ptr.IsValid)
					{
						this._validObjectLayers |= ptr.TargetsLayers;
					}
				}
			}
			else
			{
				this.childAbilitiesImpl = Array.Empty<IAbility>();
			}
			base.OnInitialize(context);
		}

		// Token: 0x06001FB3 RID: 8115 RVA: 0x00063B98 File Offset: 0x00061D98
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.specialCasesAbilities != null)
			{
				for (int i = 0; i < this.specialCasesAbilities.Length; i++)
				{
					this.specialCasesAbilities[i].Dispose();
				}
			}
		}

		// Token: 0x040013F9 RID: 5113
		private static IGameAbilitiesFactory abilitiesFactory;

		// Token: 0x040013FA RID: 5114
		[SerializeField]
		[FormerlySerializedAs("_specialCasesAbilities")]
		private SelectiveActionProjectileAbility.SpecialAbilityInfo[] specialCasesAbilities;

		// Token: 0x040013FB RID: 5115
		private readonly BaseAbility.UsingArgs specialAbilityUsingArgs = new BaseAbility.UsingArgs();

		// Token: 0x040013FC RID: 5116
		private BaseAbility selectedSpecialAbility;

		// Token: 0x040013FD RID: 5117
		private IAbility[] childAbilitiesImpl;

		// Token: 0x0200057E RID: 1406
		[Serializable]
		public struct SpecialAbilityInfo : IAbilityDecorator, IDisposable
		{
			// Token: 0x06002748 RID: 10056 RVA: 0x0007AADB File Offset: 0x00078CDB
			public static BaseAbility ToGameAbility(SelectiveActionProjectileAbility.SpecialAbilityInfo childAbility)
			{
				return childAbility.AbilityInstance;
			}

			// Token: 0x170007FA RID: 2042
			// (get) Token: 0x06002749 RID: 10057 RVA: 0x0007AAE4 File Offset: 0x00078CE4
			public int TargetsLayers
			{
				get
				{
					return this._targetsLayers;
				}
			}

			// Token: 0x170007FB RID: 2043
			// (get) Token: 0x0600274A RID: 10058 RVA: 0x0007AAF1 File Offset: 0x00078CF1
			public BaseAbility AbilityInstance
			{
				get
				{
					return this.abilityInstance;
				}
			}

			// Token: 0x170007FC RID: 2044
			// (get) Token: 0x0600274B RID: 10059 RVA: 0x0007AAF9 File Offset: 0x00078CF9
			public bool IsValid
			{
				get
				{
					return this.isValid;
				}
			}

			// Token: 0x170007FD RID: 2045
			// (get) Token: 0x0600274C RID: 10060 RVA: 0x0007AB01 File Offset: 0x00078D01
			BaseAbility IAbilityDecorator.AbilityPrototype
			{
				get
				{
					return this.abilityPrototype;
				}
			}

			// Token: 0x170007FE RID: 2046
			// (get) Token: 0x0600274D RID: 10061 RVA: 0x0007AB09 File Offset: 0x00078D09
			BaseAbility IAbilityDecorator.AbilityInstance
			{
				get
				{
					return this.abilityInstance;
				}
			}

			// Token: 0x0600274E RID: 10062 RVA: 0x0007AB14 File Offset: 0x00078D14
			public void Initialize(SelectiveActionProjectileAbility parentAbility, AbilityFactoryArgs abilitiesFactoryArgs)
			{
				if (this.abilityPrototype == null)
				{
					return;
				}
				if (this.abilityPrototype.IsProjectileAbility(false))
				{
					return;
				}
				abilitiesFactoryArgs.abilityPrototype = this.abilityPrototype;
				this.abilityInstance = (BaseAbility)SelectiveActionProjectileAbility.abilitiesFactory.Create(abilitiesFactoryArgs);
				BaseAbility baseAbility = this.abilityInstance;
				baseAbility.ValidObjectLayers |= this._targetsLayers.value;
				this.abilityInstance.IsAutoUseAbility = false;
				Ability ability = this.abilityInstance as Ability;
				if (ability != null)
				{
					ability.UsingTarget = Ability.Target.Object;
				}
				this.isValid = true;
			}

			// Token: 0x0600274F RID: 10063 RVA: 0x0007ABB1 File Offset: 0x00078DB1
			public bool HasTargetLayers(int targetLayerMask)
			{
				return this.isValid && (targetLayerMask & this._targetsLayers.value) != 0;
			}

			// Token: 0x06002750 RID: 10064 RVA: 0x0007ABCD File Offset: 0x00078DCD
			public bool InProgress()
			{
				return this.isValid && this.abilityInstance.IsBusy();
			}

			// Token: 0x06002751 RID: 10065 RVA: 0x0007ABE4 File Offset: 0x00078DE4
			public void Dispose()
			{
				if (this.abilityInstance == null)
				{
					return;
				}
				this.abilityInstance.Destroy();
				this.abilityInstance = null;
				this.isValid = false;
			}

			// Token: 0x04001C7E RID: 7294
			[SerializeField]
			private BaseAbility abilityPrototype;

			// Token: 0x04001C7F RID: 7295
			[SerializeField]
			private LayerMask _targetsLayers;

			// Token: 0x04001C80 RID: 7296
			private BaseAbility abilityInstance;

			// Token: 0x04001C81 RID: 7297
			private bool isValid;
		}
	}
}
