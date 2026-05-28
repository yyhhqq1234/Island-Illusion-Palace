using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.Editor;
using Common.PivotGroup;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Core;
using Game.Damage.Projectiles;
using Game.ObjectPool;
using Game.Utility;
using Game.VFX;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Player;

namespace Unliving.Abilities.VFX
{
	// Token: 0x020003B8 RID: 952
	[CreateAssetMenu(fileName = "AbilityVFXController", menuName = "Abilities/Controllers/Ability VFX Controller")]
	public sealed class AbilityVFXController : AbilityExtensionAssetBase, IAbilityUpdateNotifiable
	{
		// Token: 0x06001FFA RID: 8186 RVA: 0x00064B0F File Offset: 0x00062D0F
		private static void ModifyAbilityUsingDelay(BaseAbility ability, float usingDelay)
		{
			if (usingDelay > 0f && ability.UsingDelay < usingDelay)
			{
				ability.UsingDelay = usingDelay;
			}
		}

		// Token: 0x17000685 RID: 1669
		// (get) Token: 0x06001FFB RID: 8187 RVA: 0x00064B29 File Offset: 0x00062D29
		// (set) Token: 0x06001FFC RID: 8188 RVA: 0x00064B31 File Offset: 0x00062D31
		public AbilityVFXController.VisualEffect[] AbilityPrepEffects
		{
			get
			{
				return this._abilityPrepEffects;
			}
			set
			{
				this._abilityPrepEffects = value;
			}
		}

		// Token: 0x17000686 RID: 1670
		// (get) Token: 0x06001FFD RID: 8189 RVA: 0x00064B3A File Offset: 0x00062D3A
		// (set) Token: 0x06001FFE RID: 8190 RVA: 0x00064B42 File Offset: 0x00062D42
		public AbilityVFXController.VisualEffect[] AbilityActivationEffects
		{
			get
			{
				return this._abilityActivationEffects;
			}
			set
			{
				this._abilityActivationEffects = value;
			}
		}

		// Token: 0x17000687 RID: 1671
		// (get) Token: 0x06001FFF RID: 8191 RVA: 0x00064B4B File Offset: 0x00062D4B
		// (set) Token: 0x06002000 RID: 8192 RVA: 0x00064B53 File Offset: 0x00062D53
		public AbilityVFXController.VisualEffect[] AbilityUsingEffects
		{
			get
			{
				return this._abilityUsingEffects;
			}
			set
			{
				this._abilityUsingEffects = value;
			}
		}

		// Token: 0x17000688 RID: 1672
		// (get) Token: 0x06002001 RID: 8193 RVA: 0x00064B5C File Offset: 0x00062D5C
		// (set) Token: 0x06002002 RID: 8194 RVA: 0x00064B64 File Offset: 0x00062D64
		[Obsolete]
		public GameObject ObjectEffectPrefab
		{
			get
			{
				return this._objectEffectPrefab;
			}
			set
			{
				this._objectEffectPrefab = value;
			}
		}

		// Token: 0x17000689 RID: 1673
		// (get) Token: 0x06002003 RID: 8195 RVA: 0x00064B6D File Offset: 0x00062D6D
		// (set) Token: 0x06002004 RID: 8196 RVA: 0x00064B75 File Offset: 0x00062D75
		public AbilityVFXController.ObjectEffectInfo[] AttachableObjectEffects
		{
			get
			{
				return this._attachableObjectEffects;
			}
			set
			{
				this._attachableObjectEffects = value;
			}
		}

		// Token: 0x1700068A RID: 1674
		// (get) Token: 0x06002005 RID: 8197 RVA: 0x00064B7E File Offset: 0x00062D7E
		// (set) Token: 0x06002006 RID: 8198 RVA: 0x00064B86 File Offset: 0x00062D86
		public GameObject SummoningAbilityEffectPrefab
		{
			get
			{
				return this._summoningAbilityEffectPrefab;
			}
			set
			{
				this._summoningAbilityEffectPrefab = value;
			}
		}

		// Token: 0x1700068B RID: 1675
		// (get) Token: 0x06002007 RID: 8199 RVA: 0x00064B8F File Offset: 0x00062D8F
		// (set) Token: 0x06002008 RID: 8200 RVA: 0x00064B97 File Offset: 0x00062D97
		public GameObject ZoneEffectPrefab
		{
			get
			{
				return this._zoneEffectPrefab;
			}
			set
			{
				this._zoneEffectPrefab = value;
			}
		}

		// Token: 0x1700068C RID: 1676
		// (get) Token: 0x06002009 RID: 8201 RVA: 0x00064BA0 File Offset: 0x00062DA0
		// (set) Token: 0x0600200A RID: 8202 RVA: 0x00064BA8 File Offset: 0x00062DA8
		public GameObject RayAbilityRendererPrefab
		{
			get
			{
				return this._rayAbilityRendererPrefab;
			}
			set
			{
				this._rayAbilityRendererPrefab = value;
			}
		}

		// Token: 0x1700068D RID: 1677
		// (get) Token: 0x0600200B RID: 8203 RVA: 0x00064BB1 File Offset: 0x00062DB1
		// (set) Token: 0x0600200C RID: 8204 RVA: 0x00064BB9 File Offset: 0x00062DB9
		public PlayerCameraFollow.ShakeImpulse AbilityUsingCameraShake
		{
			get
			{
				return this._abilityUsingCameraShake;
			}
			set
			{
				this._abilityUsingCameraShake = value;
			}
		}

		// Token: 0x1700068E RID: 1678
		// (get) Token: 0x0600200D RID: 8205 RVA: 0x00064BC2 File Offset: 0x00062DC2
		// (set) Token: 0x0600200E RID: 8206 RVA: 0x00064BCA File Offset: 0x00062DCA
		public GameObject ProjectileLaunchEffectPrefab
		{
			get
			{
				return this._projectileLaunchEffectPrefab;
			}
			set
			{
				this._projectileLaunchEffectPrefab = value;
			}
		}

		// Token: 0x1700068F RID: 1679
		// (get) Token: 0x0600200F RID: 8207 RVA: 0x00064BD3 File Offset: 0x00062DD3
		// (set) Token: 0x06002010 RID: 8208 RVA: 0x00064BDB File Offset: 0x00062DDB
		public RandomObjectSpawner ProjectileHitEffectSpawner
		{
			get
			{
				return this._projectileHitEffectSpawner;
			}
			set
			{
				this._projectileHitEffectSpawner = value;
			}
		}

		// Token: 0x17000690 RID: 1680
		// (get) Token: 0x06002011 RID: 8209 RVA: 0x00064BE4 File Offset: 0x00062DE4
		// (set) Token: 0x06002012 RID: 8210 RVA: 0x00064BEC File Offset: 0x00062DEC
		public RandomObjectSpawner ProjectileDestructionEffectSpawner
		{
			get
			{
				return this._projectileDestructionEffectSpawner;
			}
			set
			{
				this._projectileDestructionEffectSpawner = value;
			}
		}

		// Token: 0x17000691 RID: 1681
		// (get) Token: 0x06002013 RID: 8211 RVA: 0x00064BF5 File Offset: 0x00062DF5
		// (set) Token: 0x06002014 RID: 8212 RVA: 0x00064BFD File Offset: 0x00062DFD
		public bool AlignProjectileHitEffectByFlightDirection
		{
			get
			{
				return this._alignProjectileHitEffectByFlightDirection;
			}
			set
			{
				this._alignProjectileHitEffectByFlightDirection = value;
			}
		}

		// Token: 0x17000692 RID: 1682
		// (get) Token: 0x06002015 RID: 8213 RVA: 0x00064C06 File Offset: 0x00062E06
		// (set) Token: 0x06002016 RID: 8214 RVA: 0x00064C0E File Offset: 0x00062E0E
		public bool AlignProjectileHitEffectByHitNormal
		{
			get
			{
				return this._alignProjectileHitEffectByHitNormal;
			}
			set
			{
				this._alignProjectileHitEffectByHitNormal = value;
			}
		}

		// Token: 0x17000693 RID: 1683
		// (get) Token: 0x06002017 RID: 8215 RVA: 0x00064C17 File Offset: 0x00062E17
		// (set) Token: 0x06002018 RID: 8216 RVA: 0x00064C20 File Offset: 0x00062E20
		public IUnityObjectPool<ParticleSystem> EffectsPool
		{
			get
			{
				return this.effectsPool;
			}
			set
			{
				if (this.effectsPool == value)
				{
					return;
				}
				this.effectsPool = value;
				this._projectileHitEffectSpawner.CustomInstantiator = null;
				this._projectileDestructionEffectSpawner.CustomInstantiator = null;
				if (this.effectsPool != null)
				{
					this._projectileHitEffectSpawner.CustomInstantiator = new Func<GameObject, GameObject>(this.PooledEffectInstantiator);
					this._projectileDestructionEffectSpawner.CustomInstantiator = new Func<GameObject, GameObject>(this.PooledEffectInstantiator);
				}
			}
		}

		// Token: 0x06002019 RID: 8217 RVA: 0x00064C8C File Offset: 0x00062E8C
		private GameObject PooledEffectInstantiator(GameObject effectPrefab)
		{
			AbilityVFXController.EffectsPoolArgs.unityObjectPrototype = effectPrefab;
			ParticleSystem particleSystem = this.effectsPool.TakeObject(AbilityVFXController.EffectsPoolArgs);
			if (particleSystem == null)
			{
				return null;
			}
			return particleSystem.gameObject;
		}

		// Token: 0x0600201A RID: 8218 RVA: 0x00064CB4 File Offset: 0x00062EB4
		private void UpdateAbilityOwnerInfo(object newOwner)
		{
			if (this.notifiableAbilityOwner != null)
			{
				this.notifiableAbilityOwner.AbilityUsedOnTarget -= this.OnAbilityUsedOnTarget;
			}
			this.notifiableAbilityOwner = (newOwner as INotifyAbilityUsedOnTarget);
			if (this.notifiableAbilityOwner != null)
			{
				this.notifiableAbilityOwner.AbilityUsedOnTarget += this.OnAbilityUsedOnTarget;
			}
		}

		// Token: 0x0600201B RID: 8219 RVA: 0x00064D0C File Offset: 0x00062F0C
		private void InitializeProjectileLaunchEffects(BaseAbility ability)
		{
			if (this._projectileLaunchEffectPrefab != null && this.projectileLaunchEffects.Count == 0)
			{
				ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
				if (projectileAbilityBase != null && projectileAbilityBase.HasProjectileLaunchPoints)
				{
					IList<ProjectileAbilityBase.ProjectileLaunchPoint> projectileLaunchPoints = projectileAbilityBase.ProjectileLaunchPoints;
					for (int i = 0; i < projectileLaunchPoints.Count; i++)
					{
						ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint = projectileLaunchPoints[i];
						ParticleSystem particleSystem = AbilityVFXController.VisualEffect.CreateOwnerEffect(this._projectileLaunchEffectPrefab, projectileLaunchPoint.Transform, projectileLaunchPoint.LocalPosition, new bool?(false), 1f);
						if (particleSystem != null)
						{
							particleSystem.name = string.Format("_projectileEffect{0}", i);
							this.projectileLaunchEffects.Add(particleSystem);
						}
					}
				}
			}
		}

		// Token: 0x0600201C RID: 8220 RVA: 0x00064DC4 File Offset: 0x00062FC4
		private void InitializeEffects(BaseAbility ability)
		{
			if (ability == null || ability.Owner == null)
			{
				return;
			}
			AbilityVFXController.VisualEffect[] array = this._abilityPrepEffects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Initialize(ability, AbilityEvent.Activating);
			}
			array = this._abilityActivationEffects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Initialize(ability, AbilityEvent.Activated);
			}
			array = this._abilityUsingEffects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Initialize(ability, AbilityEvent.Used);
			}
		}

		// Token: 0x0600201D RID: 8221 RVA: 0x00064E3C File Offset: 0x0006303C
		private void ActivateVisualEffects(AbilityVFXController.VisualEffect[] effects, BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs, AbilityEvent usingStage)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Activate(ability, abilityUsingArgs, usingStage);
			}
		}

		// Token: 0x0600201E RID: 8222 RVA: 0x00064E64 File Offset: 0x00063064
		private void CompleteVisualEffects(AbilityVFXController.VisualEffect[] effects)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Complete(false);
			}
		}

		// Token: 0x0600201F RID: 8223 RVA: 0x00064E88 File Offset: 0x00063088
		private void HideAbilityPrepEffects(IAbility ability)
		{
			if (ability.HasPrepTime())
			{
				this.CompleteVisualEffects(this._abilityPrepEffects);
			}
		}

		// Token: 0x06002020 RID: 8224 RVA: 0x00064EA0 File Offset: 0x000630A0
		private bool HandleAttachableObjectEffect(IObjectVFXController objectVFXController, object effectOwner, bool effectWithDuration, bool attach)
		{
			if (objectVFXController == null)
			{
				return false;
			}
			if (this._attachableObjectEffects != null && this._attachableObjectEffects.Length != 0)
			{
				bool flag = false;
				for (int i = 0; i < this._attachableObjectEffects.Length; i++)
				{
					if (attach)
					{
						AbilityVFXController.ObjectEffectInfo objectEffectInfo = this._attachableObjectEffects[i];
						objectEffectInfo.HasDuration = effectWithDuration;
						flag |= objectVFXController.AttachEffect(objectEffectInfo, effectOwner);
					}
					else
					{
						objectVFXController.DetachAttachedEffect(this._attachableObjectEffects[i], effectOwner);
					}
				}
				return flag;
			}
			if (this._objectEffectPrefab != null)
			{
				AbilityVFXController.ObjectEffectArgs.EffectPrefab = this._objectEffectPrefab;
				AbilityVFXController.ObjectEffectArgs.HasDuration = effectWithDuration;
				if (attach)
				{
					return objectVFXController.AttachEffect(AbilityVFXController.ObjectEffectArgs, effectOwner);
				}
				objectVFXController.DetachAttachedEffect(AbilityVFXController.ObjectEffectArgs, effectOwner);
			}
			return false;
		}

		// Token: 0x06002021 RID: 8225 RVA: 0x00064F53 File Offset: 0x00063153
		private void DetachObjectEffect(IObjectVFXController objectVFXController)
		{
			this.HandleAttachableObjectEffect(objectVFXController, this.currentAbility, false, false);
		}

		// Token: 0x06002022 RID: 8226 RVA: 0x00064F68 File Offset: 0x00063168
		private void CreateObjectEffect(Component targetBehaviour)
		{
			if (targetBehaviour.IsNull())
			{
				return;
			}
			IObjectVFXController objectVFXController;
			if (!targetBehaviour.TryGetComponent<IObjectVFXController>(out objectVFXController))
			{
				return;
			}
			int instanceID = targetBehaviour.gameObject.GetInstanceID();
			object currentAbility = this.currentAbility;
			if (this.currentAbility.IsContinuous && !this.currentAbility.HasWaveEffect())
			{
				if (this.HandleAttachableObjectEffect(objectVFXController, currentAbility, true, true))
				{
					this.objectEffects.Add(new KeyValuePair<int, IObjectVFXController>(instanceID, objectVFXController));
					return;
				}
			}
			else
			{
				this.HandleAttachableObjectEffect(objectVFXController, currentAbility, false, true);
			}
		}

		// Token: 0x06002023 RID: 8227 RVA: 0x00064FE0 File Offset: 0x000631E0
		private void DestroyObjectEffects()
		{
			if (this.objectEffects.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<int, IObjectVFXController> keyValuePair in this.objectEffects)
			{
				this.DetachObjectEffect(keyValuePair.Value);
			}
			this.affectedTargetsID.Clear();
		}

		// Token: 0x06002024 RID: 8228 RVA: 0x00065054 File Offset: 0x00063254
		private AbilityVFXController.RayAbilityRenderer GetRayAbilityRenderer()
		{
			if (this._rayAbilityRendererPrefab != null)
			{
				RayAbility rayAbility = this.currentAbility as RayAbility;
				if (rayAbility != null && this.currentRayAbilityRenderer == null)
				{
					LineRenderer componentOrDestroy = UnityEngine.Object.Instantiate<GameObject>(this._rayAbilityRendererPrefab).GetComponentOrDestroy<LineRenderer>();
					if (!componentOrDestroy.IsNull())
					{
						componentOrDestroy.name = this.currentAbility.OwnerBehaviour.name + "_" + this.currentAbility.name + "_linePointer";
						this.currentRayAbilityRenderer = new AbilityVFXController.RayAbilityRenderer(rayAbility, componentOrDestroy);
					}
				}
			}
			return this.currentRayAbilityRenderer;
		}

		// Token: 0x06002025 RID: 8229 RVA: 0x000650E2 File Offset: 0x000632E2
		private void UpdateRayAbilityRenderer(BaseAbility.UsingArgs abilityUsingArgs)
		{
			if (abilityUsingArgs == null)
			{
				if (this.currentRayAbilityRenderer != null)
				{
					this.currentRayAbilityRenderer.Destroy();
					this.currentRayAbilityRenderer = null;
				}
				return;
			}
			AbilityVFXController.RayAbilityRenderer rayAbilityRenderer = this.GetRayAbilityRenderer();
			if (rayAbilityRenderer == null)
			{
				return;
			}
			rayAbilityRenderer.Update(abilityUsingArgs);
		}

		// Token: 0x06002026 RID: 8230 RVA: 0x00065114 File Offset: 0x00063314
		private void CreateSummoningAbilityEffect(Component summonedMob)
		{
			if (this._summoningAbilityEffectPrefab != null && summonedMob != null)
			{
				Vector3 position = summonedMob.transform.position;
				if (this.effectsPool != null)
				{
					AbilityVFXController.EffectsPoolArgs.unityObjectPrototype = this._summoningAbilityEffectPrefab;
					this.effectsPool.TakeObject(AbilityVFXController.EffectsPoolArgs).transform.position = position;
					return;
				}
				ParticleSystem particleSystem = this._summoningAbilityEffectPrefab.InstantiateParticleSystem();
				if (particleSystem != null)
				{
					ParticleSystem.MainModule main = particleSystem.main;
					main.loop = false;
					main.stopAction = ParticleSystemStopAction.Destroy;
					particleSystem.transform.position = position;
					particleSystem.Play();
				}
			}
		}

		// Token: 0x06002027 RID: 8231 RVA: 0x000651BC File Offset: 0x000633BC
		private void SpawnProjectileHitEffect(RandomObjectSpawner effectSpawner, ProjectileHitInfo hitInfo)
		{
			if (effectSpawner == null)
			{
				return;
			}
			Quaternion? rotationOverride = null;
			if (this._alignProjectileHitEffectByFlightDirection)
			{
				rotationOverride = new Quaternion?(QuaternionExtensions.Get2DRotation(-hitInfo.projectile.CurrentVelocity, 0f));
			}
			else if (this._alignProjectileHitEffectByHitNormal)
			{
				rotationOverride = new Quaternion?(QuaternionExtensions.Get2DRotation(hitInfo.normal, 0f));
			}
			effectSpawner.Spawn(this, hitInfo.point, rotationOverride);
		}

		// Token: 0x06002028 RID: 8232 RVA: 0x00065234 File Offset: 0x00063434
		private void Cleanup(AbilityVFXController.VisualEffect[] effects)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Cleanup();
			}
		}

		// Token: 0x06002029 RID: 8233 RVA: 0x00065258 File Offset: 0x00063458
		private void Cleanup()
		{
			this.Cleanup(this._abilityActivationEffects);
			this.Cleanup(this._abilityUsingEffects);
			for (int i = 0; i < this.projectileLaunchEffects.Count; i++)
			{
				this.projectileLaunchEffects[i].DestroyAfterEmission(true, false);
			}
			this.UpdateRayAbilityRenderer(null);
			this.DestroyObjectEffects();
			this.projectileLaunchEffects.Clear();
		}

		// Token: 0x0600202A RID: 8234 RVA: 0x000652C0 File Offset: 0x000634C0
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.InitializeEffects(ability);
			ability.AddUpdateListener(this);
			ability.Activating += this.OnAbilityPreparation;
			ability.Activated += this.OnAbilityActivated;
			ability.Used += this.OnAbilityUsed;
			ability.Completed += this.OnAbilityCompleted;
			ability.AbilityEffectZoneCreated += this.OnAbilityEffectZoneCreated;
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectileLaunched += this.OnAbilityProjectileLaunched;
			}
		}

		// Token: 0x0600202B RID: 8235 RVA: 0x00065358 File Offset: 0x00063558
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Activating -= this.OnAbilityPreparation;
			ability.Activated -= this.OnAbilityActivated;
			ability.Used -= this.OnAbilityUsed;
			ability.Completed -= this.OnAbilityCompleted;
			ability.AbilityEffectZoneCreated -= this.OnAbilityEffectZoneCreated;
			ability.RemoveUpdateListener(this);
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectileLaunched -= this.OnAbilityProjectileLaunched;
			}
			this.UpdateAbilityOwnerInfo(null);
			this.Cleanup();
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x0600202C RID: 8236 RVA: 0x000653F8 File Offset: 0x000635F8
		protected override void OnAbilityOwnerChanged(BaseAbility currentAbility, object lastOwner, object newOwner)
		{
			if (this.currentAbility.IsPostMortemActivationInProgress)
			{
				return;
			}
			this.UpdateAbilityOwnerInfo(newOwner);
			this.InitializeEffects(currentAbility);
			if (this._abilityUsingCameraShake.HasValue && this.mainCamera == null)
			{
				IGameBehaviour gameBehaviour = newOwner as IGameBehaviour;
				this.mainCamera = ((gameBehaviour != null) ? gameBehaviour.CurrentGame.Services.Get<PlayerCameraFollow>() : null);
			}
		}

		// Token: 0x0600202D RID: 8237 RVA: 0x0006545E File Offset: 0x0006365E
		private void OnAbilityPreparation(IAbility ability, object usingArgs)
		{
			if (ability.HasPrepTime())
			{
				this.ActivateVisualEffects(this._abilityPrepEffects, (BaseAbility)ability, (BaseAbility.UsingArgs)usingArgs, AbilityEvent.Activating);
			}
		}

		// Token: 0x0600202E RID: 8238 RVA: 0x00065484 File Offset: 0x00063684
		private void OnAbilityActivated(IAbility ability, object usingArgs)
		{
			BaseAbility ability2 = (BaseAbility)ability;
			BaseAbility.UsingArgs abilityUsingArgs = usingArgs as BaseAbility.UsingArgs;
			this.HideAbilityPrepEffects(ability);
			AbilityVFXController.ModifyAbilityUsingDelay(ability2, this.vfxWaitingTime);
			this.ActivateVisualEffects(this._abilityActivationEffects, ability2, abilityUsingArgs, AbilityEvent.Activated);
		}

		// Token: 0x0600202F RID: 8239 RVA: 0x000654C4 File Offset: 0x000636C4
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			BaseAbility.UsingArgs abilityUsingArgs = usingArgs as BaseAbility.UsingArgs;
			if (!this.currentAbility.WasUsed)
			{
				this.UpdateRayAbilityRenderer(abilityUsingArgs);
				if (this.mainCamera != null)
				{
					this.mainCamera.AddShakeImpulse(this._abilityUsingCameraShake);
				}
			}
			this.ActivateVisualEffects(this._abilityUsingEffects, (BaseAbility)ability, abilityUsingArgs, AbilityEvent.Used);
		}

		// Token: 0x06002030 RID: 8240 RVA: 0x00065520 File Offset: 0x00063720
		private void OnAbilityUsedOnTarget(IAbility usedAbility, object abilityTarget, object args)
		{
			if (this.currentAbility == usedAbility as BaseAbility)
			{
				if (this.currentAbility is SummoningAbility)
				{
					this.CreateSummoningAbilityEffect(abilityTarget as Component);
					return;
				}
				BaseAbility.UsingArgs usingArgs = args as BaseAbility.UsingArgs;
				if (!(((usingArgs != null) ? usingArgs.additionalContext : null) is IBuff))
				{
					this.CreateObjectEffect(abilityTarget as Component);
				}
			}
		}

		// Token: 0x06002031 RID: 8241 RVA: 0x00065580 File Offset: 0x00063780
		private void OnAbilityProjectileLaunched(ProjectileAbilityBase.LaunchEventArgs eventArgs)
		{
			this.InitializeProjectileLaunchEffects(this.currentAbility);
			if (this.projectileLaunchEffects.Count != 0)
			{
				ParticleSystem particleSystem = this.projectileLaunchEffects[eventArgs.launchPointIndex];
				if (!particleSystem.isPlaying)
				{
					particleSystem.gameObject.SetActive(true);
					particleSystem.Play();
				}
			}
			eventArgs.launchedProjectile.Hit += this.OnAbilityProjectileHit;
			eventArgs.launchedProjectile.Destroyed += this.<OnAbilityProjectileLaunched>g__OnAbilityProjectileDestroyed|99_0;
		}

		// Token: 0x06002032 RID: 8242 RVA: 0x00065600 File Offset: 0x00063800
		private void OnAbilityProjectileHit(ProjectileHitInfo hitArgs)
		{
			if (!GameApplication.IsGameStateChanging)
			{
				if (hitArgs.projectile.IsDestroyed)
				{
					this.SpawnProjectileHitEffect(this._projectileDestructionEffectSpawner, hitArgs);
					return;
				}
				if (hitArgs.isEffectiveHit)
				{
					if (this.projectileHitEffectsLayers != 0 && !hitArgs.HitReceiverHasLayer(this.projectileHitEffectsLayers))
					{
						return;
					}
					this.SpawnProjectileHitEffect(this._projectileHitEffectSpawner, hitArgs);
				}
			}
		}

		// Token: 0x06002033 RID: 8243 RVA: 0x00065668 File Offset: 0x00063868
		private void OnAbilityEffectZoneCreated(BaseAbility ability, object effectZone)
		{
			AbilityEffectZone abilityEffectZone = effectZone as AbilityEffectZone;
			if (abilityEffectZone != null)
			{
				abilityEffectZone.visualEffectPrefab = this._zoneEffectPrefab;
			}
		}

		// Token: 0x06002034 RID: 8244 RVA: 0x0006568B File Offset: 0x0006388B
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.HideAbilityPrepEffects(ability);
			if (!this.currentAbility.WasUsed)
			{
				return;
			}
			this.CompleteVisualEffects(this._abilityActivationEffects);
			this.CompleteVisualEffects(this._abilityUsingEffects);
			this.UpdateRayAbilityRenderer(null);
		}

		// Token: 0x06002035 RID: 8245 RVA: 0x000656C4 File Offset: 0x000638C4
		void IAbilityUpdateNotifiable.OnAbilityUpdated(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			if (this.affectedTargetsID.Count != 0)
			{
				this.affectedTargetsID.Clear();
			}
			if (this.currentAbility.InUse)
			{
				if (abilityUsingArgs.HasTargetsList)
				{
					int targetsCount = abilityUsingArgs.TargetsCount;
					for (int i = 0; i < targetsCount; i++)
					{
						Component component = abilityUsingArgs.targetsList[i];
						if (!component.IsNull())
						{
							AbilityVFXController.<Game.Abilities.IAbilityUpdateNotifiable.OnAbilityUpdated>g__RegisterAffectedTarget|103_0(component, this.affectedTargetsID);
						}
					}
				}
				else if (abilityUsingArgs.HasTargetObject)
				{
					AbilityVFXController.<Game.Abilities.IAbilityUpdateNotifiable.OnAbilityUpdated>g__RegisterAffectedTarget|103_0(abilityUsingArgs.targetObject, this.affectedTargetsID);
				}
				this.UpdateRayAbilityRenderer(abilityUsingArgs);
			}
			if (this.objectEffects.Count != 0)
			{
				for (int j = this.objectEffects.Count - 1; j >= 0; j--)
				{
					KeyValuePair<int, IObjectVFXController> keyValuePair = this.objectEffects[j];
					if (keyValuePair.Value.IsNull() || !this.affectedTargetsID.Contains(keyValuePair.Key))
					{
						this.DetachObjectEffect(keyValuePair.Value);
						this.objectEffects.RemoveAt(j);
					}
				}
			}
		}

		// Token: 0x06002038 RID: 8248 RVA: 0x0006580F File Offset: 0x00063A0F
		[CompilerGenerated]
		private void <OnAbilityProjectileLaunched>g__OnAbilityProjectileDestroyed|99_0(IProjectile destroyedProjectile)
		{
			destroyedProjectile.Hit -= this.OnAbilityProjectileHit;
			destroyedProjectile.Destroyed -= this.<OnAbilityProjectileLaunched>g__OnAbilityProjectileDestroyed|99_0;
		}

		// Token: 0x06002039 RID: 8249 RVA: 0x00065835 File Offset: 0x00063A35
		[CompilerGenerated]
		internal static void <Game.Abilities.IAbilityUpdateNotifiable.OnAbilityUpdated>g__RegisterAffectedTarget|103_0(Component target, HashSet<int> affectedTargets)
		{
			affectedTargets.Add(target.gameObject.GetInstanceID());
		}

		// Token: 0x04001431 RID: 5169
		private static readonly AbilityVFXController.ObjectEffectInfo ObjectEffectArgs = new AbilityVFXController.ObjectEffectInfo(null);

		// Token: 0x04001432 RID: 5170
		private static readonly UnityObjectPoolArgs EffectsPoolArgs = new UnityObjectPoolArgs();

		// Token: 0x04001433 RID: 5171
		[SerializeField]
		private AbilityVFXController.VisualEffect[] _abilityPrepEffects;

		// Token: 0x04001434 RID: 5172
		[SerializeField]
		private AbilityVFXController.VisualEffect[] _abilityActivationEffects;

		// Token: 0x04001435 RID: 5173
		[SerializeField]
		private AbilityVFXController.VisualEffect[] _abilityUsingEffects;

		// Token: 0x04001436 RID: 5174
		[Tooltip("В течение этого времени абилити не будет использоваться. Можно применять для ожидания vfx.")]
		public float vfxWaitingTime;

		// Token: 0x04001437 RID: 5175
		[Space(5f)]
		[Obsolete]
		[SerializeField]
		[Tooltip("Эффект, который будет отспавнен на объекте (или объектах), к которому была применена способность.")]
		private GameObject _objectEffectPrefab;

		// Token: 0x04001438 RID: 5176
		[SerializeField]
		private AbilityVFXController.ObjectEffectInfo[] _attachableObjectEffects;

		// Token: 0x04001439 RID: 5177
		[SerializeField]
		private GameObject _summoningAbilityEffectPrefab;

		// Token: 0x0400143A RID: 5178
		[SerializeField]
		[Tooltip("Эффект для способности-лужи.")]
		private GameObject _zoneEffectPrefab;

		// Token: 0x0400143B RID: 5179
		[SerializeField]
		[Tooltip("Объект для отрисовки способности-луча. Должен иметь компонент LineRenderer.")]
		private GameObject _rayAbilityRendererPrefab;

		// Token: 0x0400143C RID: 5180
		[SerializeField]
		[Tooltip("Тряска камеры при использовании абилити.")]
		private PlayerCameraFollow.ShakeImpulse _abilityUsingCameraShake;

		// Token: 0x0400143D RID: 5181
		[Space(5f)]
		[SerializeField]
		private GameObject _projectileLaunchEffectPrefab;

		// Token: 0x0400143E RID: 5182
		[SerializeField]
		[Tooltip("Спавнер хит-эффектов для проджектайлов.")]
		private RandomObjectSpawner _projectileHitEffectSpawner;

		// Token: 0x0400143F RID: 5183
		[SerializeField]
		private RandomObjectSpawner _projectileDestructionEffectSpawner;

		// Token: 0x04001440 RID: 5184
		[SerializeField]
		[Tooltip("Эффект будет повернут по направлению движения снаряда.")]
		private bool _alignProjectileHitEffectByFlightDirection;

		// Token: 0x04001441 RID: 5185
		[SerializeField]
		[Tooltip("Эффект будет повернут по нормали точки столкновения снаряда.")]
		private bool _alignProjectileHitEffectByHitNormal;

		// Token: 0x04001442 RID: 5186
		[Tooltip("Слои объектов для которых будет создаваться эффект столкновения снаряда. Если не заданы, то эффект будет отспавнен для всех.")]
		public LayerMask projectileHitEffectsLayers = 0;

		// Token: 0x04001443 RID: 5187
		private readonly List<ParticleSystem> projectileLaunchEffects = new List<ParticleSystem>(6);

		// Token: 0x04001444 RID: 5188
		private readonly List<KeyValuePair<int, IObjectVFXController>> objectEffects = new List<KeyValuePair<int, IObjectVFXController>>();

		// Token: 0x04001445 RID: 5189
		private readonly HashSet<int> affectedTargetsID = new HashSet<int>();

		// Token: 0x04001446 RID: 5190
		private INotifyAbilityUsedOnTarget notifiableAbilityOwner;

		// Token: 0x04001447 RID: 5191
		private IUnityObjectPool<ParticleSystem> effectsPool;

		// Token: 0x04001448 RID: 5192
		private AbilityVFXController.RayAbilityRenderer currentRayAbilityRenderer;

		// Token: 0x04001449 RID: 5193
		private PlayerCameraFollow mainCamera;

		// Token: 0x02000582 RID: 1410
		[Serializable]
		public sealed class VisualEffect
		{
			// Token: 0x06002755 RID: 10069 RVA: 0x0007AD28 File Offset: 0x00078F28
			internal static ParticleSystem CreateOwnerEffect(GameObject prefab, Transform parent, Vector3 localPosition, bool? isLoopedEffect, float simulationSpeedScale = 1f)
			{
				AbilityVFXController.VisualEffect.<>c__DisplayClass0_0 CS$<>8__locals1 = new AbilityVFXController.VisualEffect.<>c__DisplayClass0_0();
				CS$<>8__locals1.isLoopedEffect = isLoopedEffect;
				CS$<>8__locals1.simulationSpeedScale = simulationSpeedScale;
				ParticleSystem particleSystem = prefab.InstantiateParticleSystem();
				if (!particleSystem.IsNull())
				{
					particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
					ParticleSystem.MainModule main = particleSystem.main;
					main.playOnAwake = false;
					if (main.stopAction != ParticleSystemStopAction.Callback)
					{
						main.stopAction = ParticleSystemStopAction.None;
					}
					particleSystem.ProcessParticleSystems(new Action<ParticleSystem>(CS$<>8__locals1.<CreateOwnerEffect>g__PrepareEmitter|0), false);
					if (parent != null)
					{
						particleSystem.transform.parent = parent;
						particleSystem.transform.localPosition = localPosition;
					}
					particleSystem.gameObject.SetActive(false);
					return particleSystem;
				}
				return null;
			}

			// Token: 0x170007FF RID: 2047
			// (get) Token: 0x06002756 RID: 10070 RVA: 0x0007ADC5 File Offset: 0x00078FC5
			// (set) Token: 0x06002757 RID: 10071 RVA: 0x0007ADCD File Offset: 0x00078FCD
			public float MinParticleSystemWaitDelay
			{
				get
				{
					return this._minParticleSystemWaitDelay;
				}
				set
				{
					this._minParticleSystemWaitDelay = Mathf.Clamp01(value);
				}
			}

			// Token: 0x06002758 RID: 10072 RVA: 0x0007ADDB File Offset: 0x00078FDB
			private void ScaleToFitRange(ParticleSystem effect, float range)
			{
				if (range > 0f)
				{
					effect.ScaleToFit(range * 2f, false);
				}
			}

			// Token: 0x06002759 RID: 10073 RVA: 0x0007ADF3 File Offset: 0x00078FF3
			private void SetAbilityUsingDelay(BaseAbility ability, ref ParticleSystem.MainModule mainModule)
			{
				AbilityVFXController.ModifyAbilityUsingDelay(ability, mainModule.duration * this._minParticleSystemWaitDelay);
			}

			// Token: 0x0600275A RID: 10074 RVA: 0x0007AE08 File Offset: 0x00079008
			private bool IsWaveVFXStage(BaseAbility ability, AbilityEvent usingStage, out bool isWaveEffectAbility)
			{
				isWaveEffectAbility = ability.HasWaveEffect();
				return isWaveEffectAbility && this.fitOwnerEffectToAbilityRange && usingStage == AbilityEvent.Activated;
			}

			// Token: 0x0600275B RID: 10075 RVA: 0x0007AE24 File Offset: 0x00079024
			private bool? GetEffectLoopState(BaseAbility ability, AbilityEvent usingStage)
			{
				bool flag2;
				bool flag = this.IsWaveVFXStage(ability, usingStage, out flag2);
				if (flag2)
				{
					if (!flag)
					{
						return null;
					}
					return new bool?(true);
				}
				else
				{
					if (usingStage == AbilityEvent.Activating)
					{
						return new bool?(ability.HasPrepTime());
					}
					if (usingStage - AbilityEvent.Using > 1)
					{
						return null;
					}
					return new bool?(ability.IsContinuous);
				}
			}

			// Token: 0x0600275C RID: 10076 RVA: 0x0007AE80 File Offset: 0x00079080
			private void StartWavePropagationAnimation(BaseAbility ability, ParticleSystem effect)
			{
				AbilityVFXController.VisualEffect.<StartWavePropagationAnimation>d__29 <StartWavePropagationAnimation>d__;
				<StartWavePropagationAnimation>d__.ability = ability;
				<StartWavePropagationAnimation>d__.effect = effect;
				<StartWavePropagationAnimation>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
				<StartWavePropagationAnimation>d__.<>1__state = -1;
				AsyncVoidMethodBuilder <>t__builder = <StartWavePropagationAnimation>d__.<>t__builder;
				<>t__builder.Start<AbilityVFXController.VisualEffect.<StartWavePropagationAnimation>d__29>(ref <StartWavePropagationAnimation>d__);
			}

			// Token: 0x0600275D RID: 10077 RVA: 0x0007AEC4 File Offset: 0x000790C4
			public void Initialize(BaseAbility ability, AbilityEvent usingStage)
			{
				if (this.isInitialized)
				{
					return;
				}
				Transform transform = null;
				Vector3 localPosition = default(Vector3);
				this.isOwnerEffectPositionDirty = true;
				if (!string.IsNullOrEmpty(this.ownerEffectAttachmentParentPath))
				{
					transform = ability.OwnerBehaviour.transform.Find(this.ownerEffectAttachmentParentPath);
					this.isOwnerEffectPositionDirty = false;
				}
				else if (AbilityVFXController.ObjectEffectInfo.IsEffectTag(this.ownerEffectAttachmentPointTag))
				{
					IPivotGroupProvider<string> pivotGroupProvider = ability.Owner as IPivotGroupProvider<string>;
					if (pivotGroupProvider != null)
					{
						IPivot pivot = pivotGroupProvider.PivotGroup.GetPivot(this.ownerEffectAttachmentPointTag);
						if (pivot != null)
						{
							transform = pivot.CurrentGroup.GroupTransform;
							localPosition = pivot.LocalPosition;
							this.isOwnerEffectPositionDirty = false;
						}
					}
				}
				if (transform == null)
				{
					transform = ability.OwnerBehaviour.transform;
				}
				bool? effectLoopState = this.GetEffectLoopState(ability, usingStage);
				this.ownerEffectParticleSystem = AbilityVFXController.VisualEffect.CreateOwnerEffect(this.ownerEffectPrefab, transform, localPosition, effectLoopState, this.ownerEffectSimulationSpeedScale);
				if (this.ownerEffectParticleSystem != null)
				{
					this.ownerEffectParticleSystem.name = ability.Name + "_VisualEffect";
					this.ownerEffectParticlesMainModule = this.ownerEffectParticleSystem.main;
					this.ownerEffectParticlesEmissionModule = this.ownerEffectParticleSystem.emission;
					if (this.rotateOwnerEffectToUsingPoint)
					{
						this.ownerEffectParticlesMainModule.scalingMode = ParticleSystemScalingMode.Shape;
					}
					if (this.fitOwnerEffectToAbilityRange)
					{
						this.ownerEffectParticleSystem.shape.radius = 1f;
						this.ScaleToFitRange(this.ownerEffectParticleSystem, ability.Range);
					}
					this.hasLoopedOwnerEffect = this.ownerEffectParticlesMainModule.loop;
				}
				this.currentAbility = ability;
				this.isInitialized = true;
			}

			// Token: 0x0600275E RID: 10078 RVA: 0x0007B054 File Offset: 0x00079254
			private void UpdateUsingPointEffect(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs, AbilityEvent usingStage)
			{
				AbilityVFXController.VisualEffect.<>c__DisplayClass31_0 CS$<>8__locals1 = new AbilityVFXController.VisualEffect.<>c__DisplayClass31_0();
				CS$<>8__locals1.<>4__this = this;
				CS$<>8__locals1.ability = ability;
				CS$<>8__locals1.usingStage = usingStage;
				if (this.pointEffectParticleSystem.IsNull())
				{
					ParticleSystem particleSystem = this.usingPointEffectPrefab.InstantiateParticleSystem();
					if (particleSystem != null)
					{
						if (this.fitPointEffectToAbilityRange)
						{
							this.ScaleToFitRange(particleSystem, this.currentAbility.Range);
						}
						ParticleSystem.MainModule main = particleSystem.main;
						particleSystem.ProcessParticleSystems(new Action<ParticleSystem>(CS$<>8__locals1.<UpdateUsingPointEffect>g__PrepareEmitter|0), false);
						this.pointEffectParticleSystem = particleSystem;
						this.pointEffectParticlesMainModule = particleSystem.main;
						this.hasLoopedPointEffect = this.pointEffectParticlesMainModule.loop;
					}
				}
				if (!this.pointEffectParticleSystem.IsNull())
				{
					this.pointEffectParticleSystem.transform.position = abilityUsingArgs.TryGetTargetPosition();
					this.SetAbilityUsingDelay(CS$<>8__locals1.ability, ref this.pointEffectParticlesMainModule);
					if (!this.pointEffectParticleSystem.isPlaying)
					{
						this.pointEffectParticleSystem.Play();
					}
				}
			}

			// Token: 0x0600275F RID: 10079 RVA: 0x0007B144 File Offset: 0x00079344
			public void Activate(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs, AbilityEvent usingStage)
			{
				if (!this.ownerEffectParticleSystem.IsNull())
				{
					if (this.isOwnerEffectPositionDirty)
					{
						MonoBehaviour ownerBehaviour = ability.OwnerBehaviour;
						Transform transform = (ownerBehaviour != null) ? ownerBehaviour.transform : null;
						if (transform != null)
						{
							this.ownerEffectParticleSystem.transform.localPosition = transform.InverseTransformPoint(ability.OwnerPosition);
						}
						this.isOwnerEffectPositionDirty = false;
					}
					if (this.rotateOwnerEffectToUsingPoint)
					{
						ICustomUsingArgsAbility customUsingArgsAbility = ability as ICustomUsingArgsAbility;
						Vector2 vector = (((customUsingArgsAbility != null) ? customUsingArgsAbility.SourceUsingArgs : null) ?? abilityUsingArgs).TryGetTargetPosition() - this.ownerEffectParticleSystem.transform.position;
						if (vector.SqrMagnitude() > 0.0001f)
						{
							float constant = Mathf.Atan2(-vector.y, vector.x);
							this.ownerEffectParticlesMainModule.startRotation = constant;
						}
					}
					this.ownerEffectParticleSystem.gameObject.SetActive(true);
					bool flag;
					if (this.IsWaveVFXStage(ability, usingStage, out flag))
					{
						if (!ability.WasUsed)
						{
							this.StartWavePropagationAnimation(ability, this.ownerEffectParticleSystem);
						}
					}
					else
					{
						bool flag2 = usingStage == AbilityEvent.Activated;
						if (flag2)
						{
							this.SetAbilityUsingDelay(ability, ref this.ownerEffectParticlesMainModule);
						}
						if (flag2 || !this.ownerEffectParticleSystem.isPlaying || this.ownerEffectParticlesEmissionModule.rateOverDistance.constant > 0f)
						{
							this.ownerEffectParticleSystem.Play();
						}
					}
				}
				if (abilityUsingArgs != null)
				{
					this.UpdateUsingPointEffect(ability, abilityUsingArgs, usingStage);
				}
			}

			// Token: 0x06002760 RID: 10080 RVA: 0x0007B2A4 File Offset: 0x000794A4
			public void Complete(bool force = false)
			{
				force |= this.forceFinalizeEffects;
				if ((force || this.hasLoopedOwnerEffect) && !this.ownerEffectParticleSystem.IsNull())
				{
					this.ownerEffectParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
				}
				if ((force || this.hasLoopedPointEffect) && !this.pointEffectParticleSystem.IsNull())
				{
					this.pointEffectParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
				}
			}

			// Token: 0x06002761 RID: 10081 RVA: 0x0007B305 File Offset: 0x00079505
			public void Cleanup()
			{
				this.ownerEffectParticleSystem.DestroyAfterEmission(true, false);
				this.ownerEffectParticleSystem = null;
				this.Complete(true);
			}

			// Token: 0x04001C8E RID: 7310
			[Tooltip("Эффект, прикрепленный к носителю абилити.")]
			public GameObject ownerEffectPrefab;

			// Token: 0x04001C8F RID: 7311
			[Tooltip("Путь к чайлд трансформу, к которому будет прикреплен эффект.")]
			public string ownerEffectAttachmentParentPath;

			// Token: 0x04001C90 RID: 7312
			[Tag]
			public string ownerEffectAttachmentPointTag;

			// Token: 0x04001C91 RID: 7313
			[FormerlySerializedAs("fitOwnerEffectScaleToAbilityRange")]
			[Tooltip("Если включено, то система частиц эффекта будет отскейлена под радиус способности.")]
			public bool fitOwnerEffectToAbilityRange;

			// Token: 0x04001C92 RID: 7314
			public bool rotateOwnerEffectToUsingPoint;

			// Token: 0x04001C93 RID: 7315
			public float ownerEffectSimulationSpeedScale = 1f;

			// Token: 0x04001C94 RID: 7316
			[Space(5f)]
			[Tooltip("Эффект, который будет отспавнен в точке применения абилити.")]
			public GameObject usingPointEffectPrefab;

			// Token: 0x04001C95 RID: 7317
			[FormerlySerializedAs("fitPointEffectScaleToAbilityRange")]
			[Tooltip("Если включено, то система частиц эффекта будет отскейлена под радиус способности.")]
			public bool fitPointEffectToAbilityRange;

			// Token: 0x04001C96 RID: 7318
			public float pointEffectSimulationSpeedScale = 1f;

			// Token: 0x04001C97 RID: 7319
			[SerializeField]
			[Range(0f, 1f)]
			private float _minParticleSystemWaitDelay;

			// Token: 0x04001C98 RID: 7320
			public bool forceFinalizeEffects;

			// Token: 0x04001C99 RID: 7321
			private BaseAbility currentAbility;

			// Token: 0x04001C9A RID: 7322
			private ParticleSystem ownerEffectParticleSystem;

			// Token: 0x04001C9B RID: 7323
			private ParticleSystem.MainModule ownerEffectParticlesMainModule;

			// Token: 0x04001C9C RID: 7324
			private ParticleSystem.EmissionModule ownerEffectParticlesEmissionModule;

			// Token: 0x04001C9D RID: 7325
			private ParticleSystem pointEffectParticleSystem;

			// Token: 0x04001C9E RID: 7326
			private ParticleSystem.MainModule pointEffectParticlesMainModule;

			// Token: 0x04001C9F RID: 7327
			private bool hasLoopedOwnerEffect;

			// Token: 0x04001CA0 RID: 7328
			private bool hasLoopedPointEffect;

			// Token: 0x04001CA1 RID: 7329
			private bool isOwnerEffectPositionDirty;

			// Token: 0x04001CA2 RID: 7330
			[NonSerialized]
			private bool isInitialized;
		}

		// Token: 0x02000583 RID: 1411
		private sealed class RayAbilityRenderer
		{
			// Token: 0x06002763 RID: 10083 RVA: 0x0007B340 File Offset: 0x00079540
			public RayAbilityRenderer(RayAbility ability, LineRenderer renderer)
			{
				this.ability = ability;
				this.renderer = renderer;
				renderer.useWorldSpace = true;
				renderer.positionCount = 2;
				renderer.startWidth = ability.RayThickness;
				renderer.endWidth = ability.RayThickness;
			}

			// Token: 0x06002764 RID: 10084 RVA: 0x0007B37C File Offset: 0x0007957C
			public void Update(BaseAbility.UsingArgs usingArgs)
			{
				Vector3 ownerPosition = this.ability.OwnerPosition;
				Vector3 b = Vector3.ClampMagnitude(usingArgs.targetPosition - ownerPosition, this.ability.CurrentRayLength);
				this.renderer.SetPosition(0, ownerPosition);
				this.renderer.SetPosition(1, ownerPosition + b);
			}

			// Token: 0x06002765 RID: 10085 RVA: 0x0007B3D2 File Offset: 0x000795D2
			public void Destroy()
			{
				UnityEngine.Object.Destroy(this.renderer.gameObject, this.ability.IsContinuous ? 0f : Mathf.Max(0.2f, Time.deltaTime));
			}

			// Token: 0x04001CA3 RID: 7331
			private readonly RayAbility ability;

			// Token: 0x04001CA4 RID: 7332
			private readonly LineRenderer renderer;
		}

		// Token: 0x02000584 RID: 1412
		public enum ObjectEffectPriority
		{
			// Token: 0x04001CA6 RID: 7334
			AlwaysVisible = -1,
			// Token: 0x04001CA7 RID: 7335
			Low,
			// Token: 0x04001CA8 RID: 7336
			Medium,
			// Token: 0x04001CA9 RID: 7337
			High
		}

		// Token: 0x02000585 RID: 1413
		[Serializable]
		public sealed class ObjectEffectInfo : IAttachableEffectArgs, IEffectColorData, IEffectGradientData, ICloneable<AbilityVFXController.ObjectEffectInfo>
		{
			// Token: 0x06002766 RID: 10086 RVA: 0x0007B407 File Offset: 0x00079607
			public static bool IsEffectTag(string tag)
			{
				return !string.IsNullOrEmpty(tag) && tag != TaggedPivotGroup.UntaggedTagValue;
			}

			// Token: 0x17000800 RID: 2048
			// (get) Token: 0x06002767 RID: 10087 RVA: 0x0007B41E File Offset: 0x0007961E
			public string AttachmentPointTag
			{
				get
				{
					return this._attachmentPointTag;
				}
			}

			// Token: 0x17000801 RID: 2049
			// (get) Token: 0x06002768 RID: 10088 RVA: 0x0007B426 File Offset: 0x00079626
			public AbilityVFXController.ObjectEffectPriority EffectPriority
			{
				get
				{
					return this._effectPriority;
				}
			}

			// Token: 0x17000802 RID: 2050
			// (get) Token: 0x06002769 RID: 10089 RVA: 0x0007B42E File Offset: 0x0007962E
			public Color EffectColor
			{
				get
				{
					return this._effectColor;
				}
			}

			// Token: 0x17000803 RID: 2051
			// (get) Token: 0x0600276A RID: 10090 RVA: 0x0007B436 File Offset: 0x00079636
			public Gradient EffectGradient
			{
				get
				{
					if (!this.useEffectGradient)
					{
						return null;
					}
					return this._effectGradient;
				}
			}

			// Token: 0x17000804 RID: 2052
			// (get) Token: 0x0600276B RID: 10091 RVA: 0x0007B448 File Offset: 0x00079648
			object IAttachableEffectArgs.AttachmentPointID
			{
				get
				{
					return this._attachmentPointTag;
				}
			}

			// Token: 0x17000805 RID: 2053
			// (get) Token: 0x0600276C RID: 10092 RVA: 0x0007B450 File Offset: 0x00079650
			int IAttachableEffectArgs.EffectPriority
			{
				get
				{
					return (int)this._effectPriority;
				}
			}

			// Token: 0x17000806 RID: 2054
			// (get) Token: 0x0600276D RID: 10093 RVA: 0x0007B458 File Offset: 0x00079658
			// (set) Token: 0x0600276E RID: 10094 RVA: 0x0007B460 File Offset: 0x00079660
			public bool HasDuration { get; set; }

			// Token: 0x17000807 RID: 2055
			// (get) Token: 0x0600276F RID: 10095 RVA: 0x0007B469 File Offset: 0x00079669
			// (set) Token: 0x06002770 RID: 10096 RVA: 0x0007B471 File Offset: 0x00079671
			public GameObject EffectPrefab
			{
				get
				{
					return this._effectPrefab;
				}
				internal set
				{
					this._effectPrefab = value;
				}
			}

			// Token: 0x06002771 RID: 10097 RVA: 0x0007B47A File Offset: 0x0007967A
			public ObjectEffectInfo(GameObject effectPrefab)
			{
				this._effectPrefab = effectPrefab;
				this._effectPriority = AbilityVFXController.ObjectEffectPriority.AlwaysVisible;
				this._attachmentPointTag = TaggedPivotGroup.UntaggedTagValue;
			}

			// Token: 0x06002772 RID: 10098 RVA: 0x0007B49B File Offset: 0x0007969B
			public AbilityVFXController.ObjectEffectInfo Clone()
			{
				return (AbilityVFXController.ObjectEffectInfo)base.MemberwiseClone();
			}

			// Token: 0x04001CAB RID: 7339
			[SerializeField]
			[Tag]
			private string _attachmentPointTag;

			// Token: 0x04001CAC RID: 7340
			[SerializeField]
			private GameObject _effectPrefab;

			// Token: 0x04001CAD RID: 7341
			[SerializeField]
			private AbilityVFXController.ObjectEffectPriority _effectPriority;

			// Token: 0x04001CAE RID: 7342
			[SerializeField]
			private Color _effectColor;

			// Token: 0x04001CAF RID: 7343
			[SerializeField]
			private Gradient _effectGradient;

			// Token: 0x04001CB0 RID: 7344
			public bool useEffectGradient;
		}
	}
}
