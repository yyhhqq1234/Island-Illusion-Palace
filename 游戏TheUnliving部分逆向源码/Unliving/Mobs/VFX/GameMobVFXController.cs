using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.CollectionsExtensions;
using Common.Editor;
using Common.PivotGroup;
using Common.Tiles;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Buffs.VFX;
using Game.Core;
using Game.Damage;
using Game.ObjectPool;
using Game.VFX;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Abilities.VFX;
using Unliving.AbilityResources;
using Unliving.GameSession.PlayerArmySizeLimiting;
using Unliving.LevelGeneration;
using Unliving.Mobs.Animation;
using Unliving.Mobs.Motion;
using Unliving.ObjectPools;
using Unliving.Player;

namespace Unliving.Mobs.VFX
{
	// Token: 0x02000204 RID: 516
	public sealed class GameMobVFXController : GameBehaviourBase, IObjectVFXController, ITileEffectSender
	{
		// Token: 0x0600114C RID: 4428 RVA: 0x00035F65 File Offset: 0x00034165
		private static bool IsEventArg(string arg, string targetArg)
		{
			return string.Equals(arg, targetArg, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x00035F6F File Offset: 0x0003416F
		private static int GetEffectSlotID(string slotTag)
		{
			return TaggedPivotGroup.TagToHash(slotTag);
		}

		// Token: 0x0600114E RID: 4430 RVA: 0x00035F77 File Offset: 0x00034177
		private static int GetEffectSlotID(IAttachableEffectArgs effectArgs)
		{
			return GameMobVFXController.GetEffectSlotID((string)effectArgs.AttachmentPointID);
		}

		// Token: 0x0600114F RID: 4431 RVA: 0x00035F89 File Offset: 0x00034189
		private static bool IsColorSlot(int slotID)
		{
			return slotID == GameMobVFXController.MobColorSlotID;
		}

		// Token: 0x06001150 RID: 4432 RVA: 0x00035F93 File Offset: 0x00034193
		private static bool IsEmptyEffectArgs(int slotID, IAttachableEffectArgs effectArgs)
		{
			return !GameMobVFXController.IsColorSlot(slotID) && effectArgs.EffectPrefab == null;
		}

		// Token: 0x06001151 RID: 4433 RVA: 0x00035FAC File Offset: 0x000341AC
		private static int GetEffectID(int slotID, IAttachableEffectArgs effectArgs)
		{
			return (GameMobVFXController.IsColorSlot(slotID) ? effectArgs.EffectColor.GetHashCode() : effectArgs.EffectPrefab.GetInstanceID()) ^ effectArgs.EffectPriority << 2;
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x00035FEC File Offset: 0x000341EC
		private static int GetEffectIndex(List<GameMobVFXController.AttachableEffect> effects, int effectID)
		{
			for (int i = 0; i < effects.Count; i++)
			{
				if (effects[i].ID == effectID)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x0003601C File Offset: 0x0003421C
		private static void ActivateEffects(GameMobVFXController.VisualEffect[] effects, BaseGameMob currentMob, ref float lastActivationTime, Component sender = null)
		{
			if (effects == null || effects.Length == 0)
			{
				return;
			}
			GameMobVFXController.randomEffectsCount = 0;
			foreach (GameMobVFXController.VisualEffect visualEffect in effects)
			{
				if (visualEffect.senderLayerMask == 0 || visualEffect.senderLayerMask == -1 || sender.IsNull() || visualEffect.senderLayerMask == (visualEffect.senderLayerMask | 1 << sender.gameObject.layer))
				{
					if (visualEffect.forceActivation)
					{
						visualEffect.Activate(currentMob);
					}
					else
					{
						GameMobVFXController.RandomEffectsBuffer[GameMobVFXController.randomEffectsCount++] = visualEffect;
					}
				}
			}
			if (GameMobVFXController.randomEffectsCount > 0)
			{
				GameMobVFXController.VisualEffect visualEffect2;
				GameMobVFXController.RandomEffectsBuffer.GetRandomWeightedItem(out visualEffect2, 0, GameMobVFXController.randomEffectsCount, null);
				visualEffect2.Activate(currentMob, ref lastActivationTime);
			}
		}

		// Token: 0x06001154 RID: 4436 RVA: 0x000360FC File Offset: 0x000342FC
		private static void DestroyEffects(GameMobVFXController.VisualEffect[] effects, BaseGameMob currentMob)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Destroy(currentMob);
			}
		}

		// Token: 0x06001155 RID: 4437 RVA: 0x00036124 File Offset: 0x00034324
		public static void UpdateShadowTransform(Transform shadowTransform, float groundDistance, float shadowOffset, bool keepOrientation)
		{
			Vector3 vector = new Vector3
			{
				y = shadowOffset - groundDistance
			};
			Transform parent = shadowTransform.parent;
			if (parent != null)
			{
				if (keepOrientation)
				{
					Quaternion quaternion = Quaternion.Inverse(parent.rotation);
					vector = quaternion * vector;
					shadowTransform.localRotation = quaternion;
				}
				float x = parent.localScale.x;
				Vector3 localScale = shadowTransform.localScale;
				if (x * localScale.x < 0f)
				{
					localScale.x = Mathf.Sign(x);
					shadowTransform.localScale = localScale;
				}
			}
			shadowTransform.localPosition = vector;
		}

		// Token: 0x06001156 RID: 4438 RVA: 0x000361B6 File Offset: 0x000343B6
		public static void UpdateShadowTransform(Transform shadowTransform, GameMobMotionControllerBase motionController, float shadowOffset, bool keepOrientation)
		{
			GameMobVFXController.UpdateShadowTransform(shadowTransform, motionController.CurrentGroundDistance, shadowOffset, keepOrientation);
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06001157 RID: 4439 RVA: 0x000361C6 File Offset: 0x000343C6
		// (set) Token: 0x06001158 RID: 4440 RVA: 0x000361CE File Offset: 0x000343CE
		public GameMobVFXController.ResourcesCollectionEffect[] ResourcesCollectionEffects
		{
			get
			{
				return this._resourcesCollectionEffects;
			}
			set
			{
				this._resourcesCollectionEffects = value;
			}
		}

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x06001159 RID: 4441 RVA: 0x000361D7 File Offset: 0x000343D7
		// (set) Token: 0x0600115A RID: 4442 RVA: 0x000361DF File Offset: 0x000343DF
		public PlayerCameraFollow.ShakeImpulse KillCameraShakeImpulse
		{
			get
			{
				return this._killCameraShakeImpulse;
			}
			set
			{
				this._killCameraShakeImpulse = value;
			}
		}

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x0600115B RID: 4443 RVA: 0x000361E8 File Offset: 0x000343E8
		// (set) Token: 0x0600115C RID: 4444 RVA: 0x000361F0 File Offset: 0x000343F0
		public IUnityObjectPool<ParticleSystem> AttachableEffectsPool { get; set; }

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x0600115D RID: 4445 RVA: 0x000361F9 File Offset: 0x000343F9
		public BaseGameMob TargetMob
		{
			get
			{
				return this.targetMob;
			}
		}

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x0600115E RID: 4446 RVA: 0x00036201 File Offset: 0x00034401
		public GameMobVFXController.VisualEffect[] DamageReactionEffects
		{
			get
			{
				return this._damageReactionEffects;
			}
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x0600115F RID: 4447 RVA: 0x00036209 File Offset: 0x00034409
		public GameMobVFXController.VisualEffect[] AttackEffects
		{
			get
			{
				return this._attackEffects;
			}
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x06001160 RID: 4448 RVA: 0x00036211 File Offset: 0x00034411
		public GameMobVFXController.VisualEffect[] DeathEffects
		{
			get
			{
				return this._deathEffects;
			}
		}

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x06001161 RID: 4449 RVA: 0x00036219 File Offset: 0x00034419
		public GameMobVFXController.VisualEffect[] AbilityActivationInterruptionEffects
		{
			get
			{
				return this._abilityActivationInterruptionEffects;
			}
		}

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x06001162 RID: 4450 RVA: 0x00036221 File Offset: 0x00034421
		public Vector3Int CurrentTileCell
		{
			get
			{
				return this.currentGroundCell.GetValueOrDefault();
			}
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x06001163 RID: 4451 RVA: 0x0003622E File Offset: 0x0003442E
		public IMaterialUsingTile CurrentTile
		{
			get
			{
				return this.currentGroundTile;
			}
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x00036238 File Offset: 0x00034438
		private bool DetachAttachedEffect(int effectSlotID, int effectID, object effectOwner)
		{
			GameMobVFXController.AttachableEffectsController attachableEffectsController;
			return this.attachedEffectsControllers.TryGetValue(effectSlotID, out attachableEffectsController) && attachableEffectsController.UnregisterEffect(effectID, effectOwner);
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x0003625F File Offset: 0x0003445F
		private void TryRegisterBuffVisualEffect(int effectSlotID, int effectID, IBuff buff)
		{
			if (buff == null)
			{
				return;
			}
			buff.Completed += this.OnBuffWithVisualsCompleted;
			this.attachedBuffEffects.Add(new GameMobVFXController.BuffEffectInfo(buff, effectSlotID, effectID));
		}

		// Token: 0x06001166 RID: 4454 RVA: 0x0003628C File Offset: 0x0003448C
		private void SetMobColor(Color newColor)
		{
			SpriteRenderer renderer = this.targetMob.Renderer;
			if (renderer != null)
			{
				renderer.color = newColor;
			}
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x000362B5 File Offset: 0x000344B5
		private void ResetMobColor()
		{
			this.SetMobColor(this.initialMobColor);
		}

		// Token: 0x06001168 RID: 4456 RVA: 0x000362C3 File Offset: 0x000344C3
		private bool CanUseGroundEffects()
		{
			return this.hasAnimationController && this.targetMob.IsRendererVisible;
		}

		// Token: 0x06001169 RID: 4457 RVA: 0x000362DC File Offset: 0x000344DC
		private bool IsMobMoving()
		{
			return this.hasAnimationController && this.mobAnimationController.SmoothedVelocity.sqrMagnitude > 0.01f;
		}

		// Token: 0x0600116A RID: 4458 RVA: 0x0003630D File Offset: 0x0003450D
		private Vector3 GetMobFootPosition()
		{
			if (this.mobFootPivot == null)
			{
				return base.transform.position;
			}
			return base.transform.position + this.mobFootPivot.LocalPosition;
		}

		// Token: 0x0600116B RID: 4459 RVA: 0x00036340 File Offset: 0x00034540
		private void UpdateCustomBuffEffectColor()
		{
			if (this.mobColorEffectsController == null || !this.mobColorEffectsController.IsActive)
			{
				return;
			}
			GameMobVFXController.AttachableEffect highestPriorityEffect = this.mobColorEffectsController.GetHighestPriorityEffect();
			if (highestPriorityEffect != null && !highestPriorityEffect.IsDestroyed)
			{
				int id = highestPriorityEffect.ID;
				for (int i = 0; i < this.attachedBuffEffects.Count; i++)
				{
					GameMobVFXController.BuffEffectInfo buffEffectInfo = this.attachedBuffEffects[i];
					if (buffEffectInfo.EffectID == id)
					{
						IBuffWithColorProperty buffWithColorProperty = buffEffectInfo.Buff as IBuffWithColorProperty;
						if (buffWithColorProperty != null)
						{
							Color mobColor;
							if (buffWithColorProperty.GetValue(out mobColor))
							{
								this.SetMobColor(mobColor);
								return;
							}
							break;
						}
					}
				}
			}
		}

		// Token: 0x0600116C RID: 4460 RVA: 0x000363D4 File Offset: 0x000345D4
		private void DestroyAllAttachedEffects()
		{
			foreach (GameMobVFXController.BuffEffectInfo buffEffectInfo in this.attachedBuffEffects)
			{
				if (buffEffectInfo.Buff != null)
				{
					buffEffectInfo.Buff.Completed -= this.OnBuffWithVisualsCompleted;
				}
			}
			if (!GameApplication.IsGameStateChanging)
			{
				foreach (KeyValuePair<int, GameMobVFXController.AttachableEffectsController> keyValuePair in this.attachedEffectsControllers)
				{
					keyValuePair.Value.DestroyAllEffects();
				}
			}
			this.attachedEffectsControllers.Clear();
			this.attachedBuffEffects.Clear();
		}

		// Token: 0x0600116D RID: 4461 RVA: 0x000364A4 File Offset: 0x000346A4
		private GameMobVFXController.AttachableEffect AddAttachableEffect(IAttachableEffectArgs effectArgs, object effectOwner)
		{
			if (effectOwner == null)
			{
				throw new ArgumentNullException("effectOwner");
			}
			int effectSlotID = GameMobVFXController.GetEffectSlotID(effectArgs);
			if (GameMobVFXController.IsEmptyEffectArgs(effectSlotID, effectArgs))
			{
				return null;
			}
			GameMobVFXController.AttachableEffectsController attachableEffectsController;
			if (!this.attachedEffectsControllers.TryGetValue(effectSlotID, out attachableEffectsController))
			{
				TaggedPivot pivot = this.targetMob.TaggedPivotsGroup.GetPivot(effectSlotID);
				if (pivot != null)
				{
					attachableEffectsController = new GameMobVFXController.AttachableEffectsController(this, effectSlotID, pivot.CurrentGroup.GroupTransform, pivot.LocalPosition);
				}
				else
				{
					attachableEffectsController = new GameMobVFXController.AttachableEffectsController(this, effectSlotID, base.transform, default(Vector3));
				}
				attachableEffectsController.EffectsPool = this.AttachableEffectsPool;
				this.attachedEffectsControllers.Add(effectSlotID, attachableEffectsController);
				if (GameMobVFXController.IsColorSlot(effectSlotID))
				{
					this.mobColorEffectsController = attachableEffectsController;
				}
			}
			GameMobVFXController.AttachableEffect attachableEffect = attachableEffectsController.RegisterEffect(effectArgs, effectOwner);
			if (attachableEffect != null)
			{
				this.TryRegisterBuffVisualEffect(effectSlotID, attachableEffect.ID, effectOwner as IBuff);
			}
			return attachableEffect;
		}

		// Token: 0x0600116E RID: 4462 RVA: 0x00036571 File Offset: 0x00034771
		public bool AttachEffect(IAttachableEffectArgs effectArgs, object effectOwner)
		{
			return this.AddAttachableEffect(effectArgs, effectOwner) != null;
		}

		// Token: 0x0600116F RID: 4463 RVA: 0x00036580 File Offset: 0x00034780
		public GameMobVFXController.AttachableEffect GetAttachedEffect(IAttachableEffectArgs effectArgs, object effectOwner)
		{
			int effectSlotID = GameMobVFXController.GetEffectSlotID(effectArgs);
			GameMobVFXController.AttachableEffectsController attachableEffectsController;
			if (this.attachedEffectsControllers.TryGetValue(effectSlotID, out attachableEffectsController))
			{
				return attachableEffectsController.GetEffect(GameMobVFXController.GetEffectID(effectSlotID, effectArgs), effectOwner);
			}
			return null;
		}

		// Token: 0x06001170 RID: 4464 RVA: 0x000365B4 File Offset: 0x000347B4
		public bool DetachAttachedEffect(IAttachableEffectArgs effectArgs, object effectOwner)
		{
			int effectSlotID = GameMobVFXController.GetEffectSlotID(effectArgs);
			return this.DetachAttachedEffect(effectSlotID, GameMobVFXController.GetEffectID(effectSlotID, effectArgs), effectOwner);
		}

		// Token: 0x06001171 RID: 4465 RVA: 0x000365D8 File Offset: 0x000347D8
		public void SetEffectsGroupActive(string groupTag, bool isActive)
		{
			if (string.IsNullOrEmpty(groupTag))
			{
				return;
			}
			int effectSlotID = GameMobVFXController.GetEffectSlotID(groupTag);
			GameMobVFXController.AttachableEffectsController attachableEffectsController;
			if (this.attachedEffectsControllers.TryGetValue(effectSlotID, out attachableEffectsController))
			{
				attachableEffectsController.IsActive = isActive;
				return;
			}
			if (isActive)
			{
				GameMobVFXController.AttachableEffectsController.MarkAsEnabled(this, effectSlotID);
				return;
			}
			GameMobVFXController.AttachableEffectsController.MarkAsDisabled(this, effectSlotID);
		}

		// Token: 0x06001172 RID: 4466 RVA: 0x00036621 File Offset: 0x00034821
		void IObjectVFXController.SetEffectsGroupActive(object groupID, bool isActive)
		{
			this.SetEffectsGroupActive((string)groupID, isActive);
		}

		// Token: 0x06001173 RID: 4467 RVA: 0x00036630 File Offset: 0x00034830
		public void ActivateFootstepEffect()
		{
			if (!this.hasGroundEffectsEmitter || !this.CanUseGroundEffects() || !this.IsMobMoving())
			{
				return;
			}
			if (this.particlesSystemsManager.SpawnFootstepParticle(this.GetMobFootPosition(), this.lastFootstepEffectEmitTime, this.targetMob.Radius))
			{
				this.lastFootstepEffectEmitTime = Time.time;
			}
		}

		// Token: 0x06001174 RID: 4468 RVA: 0x00036688 File Offset: 0x00034888
		public void ActivateTileEffect()
		{
			if (!this.CanUseGroundEffects())
			{
				return;
			}
			LocationChunk locationChunk = this.targetMob.CurrentLocationChunk as LocationChunk;
			if (locationChunk != null)
			{
				Vector3 mobFootPosition = this.GetMobFootPosition();
				Vector3Int vector3Int;
				if (locationChunk.TryGetGridCellPosition(mobFootPosition, out vector3Int))
				{
					if (this.IsMobMoving())
					{
						if (vector3Int == this.currentGroundCell)
						{
							return;
						}
						this.currentGroundTile = locationChunk.GetGroundTileAtCellPosition(vector3Int);
						this.currentGroundCell = new Vector3Int?(vector3Int);
						if (this.<ActivateTileEffect>g__CanEmitTileParticles|103_0())
						{
							this.particlesSystemsManager.SpawnDynamicParticle((int)this.currentGroundTile.TileMaterial, mobFootPosition);
							return;
						}
					}
					else if (vector3Int == this.currentGroundCell && this.<ActivateTileEffect>g__CanEmitTileParticles|103_0() && this.particlesSystemsManager.SpawnStaticParticle((int)this.currentGroundTile.TileMaterial, mobFootPosition, this.lastTileEffectTime))
					{
						this.lastTileEffectTime = Time.time;
						return;
					}
				}
			}
			else
			{
				this.currentGroundCell = null;
				this.currentGroundTile = null;
			}
		}

		// Token: 0x06001175 RID: 4469 RVA: 0x000367A0 File Offset: 0x000349A0
		private void OnMobBuffActivated(IBuff buff)
		{
			IList<IAttachableEffectArgs> list = buff.VFXData as IList<IAttachableEffectArgs>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					this.AddAttachableEffect(list[i], buff);
				}
				return;
			}
			IAttachableEffectArgs attachableEffectArgs = buff.VFXData as IAttachableEffectArgs;
			if (attachableEffectArgs != null)
			{
				this.AddAttachableEffect(attachableEffectArgs, buff);
			}
		}

		// Token: 0x06001176 RID: 4470 RVA: 0x000367F8 File Offset: 0x000349F8
		private void OnBuffWithVisualsCompleted(IBuff completedBuff)
		{
			for (int i = 0; i < this.attachedBuffEffects.Count; i++)
			{
				GameMobVFXController.BuffEffectInfo buffEffectInfo = this.attachedBuffEffects[i];
				if (buffEffectInfo.Buff == completedBuff)
				{
					this.DetachAttachedEffect(buffEffectInfo.EffectSlotID, buffEffectInfo.EffectID, completedBuff);
					this.attachedBuffEffects.RemoveBySwap(i);
					break;
				}
			}
			completedBuff.Completed -= this.OnBuffWithVisualsCompleted;
		}

		// Token: 0x06001177 RID: 4471 RVA: 0x00036865 File Offset: 0x00034A65
		private void OnDamageResistActivated()
		{
			this.damageResistEffect.HasDuration = true;
			this.AddAttachableEffect(this.damageResistEffect, this.targetMob.HitPointsController);
		}

		// Token: 0x06001178 RID: 4472 RVA: 0x0003688B File Offset: 0x00034A8B
		private void OnDamageResistDeactivated()
		{
			this.DetachAttachedEffect(this.damageResistEffect, this.targetMob.HitPointsController);
		}

		// Token: 0x06001179 RID: 4473 RVA: 0x000368A8 File Offset: 0x00034AA8
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.targetMob = base.GetComponent<BaseGameMob>();
			BaseGameMob baseGameMob = this.targetMob;
			if (((baseGameMob != null) ? baseGameMob.Renderer : null) != null)
			{
				this.initialMobColor = this.targetMob.Renderer.color;
			}
			this.hasGroundEffectsEmitter = base.CurrentGame.Services.TryGet<ParticlesSystemsManager>(out this.particlesSystemsManager);
		}

		// Token: 0x0600117A RID: 4474 RVA: 0x00036914 File Offset: 0x00034B14
		private void Start()
		{
			if (GameMobVFXController.currentCamera == null)
			{
				GameMobVFXController.currentCamera = base.CurrentGame.Services.Get<PlayerCameraFollow>();
			}
			if (this.targetMob != null)
			{
				if (this.shadowRenderer != null)
				{
					this.initialShadowOffset = this.shadowRenderer.transform.localPosition.y;
				}
				this.mobMotionController = this.targetMob.MotionController;
				this.mobAnimationController = (this.targetMob.AnimationController as GameMobAnimationController);
				this.targetMob.AbilityPrepInterrupted += this.OnMobAbilityActivationInterrupted;
				this.targetMob.AnimationEventFired += this.OnAnimationEventFired;
				this.targetMob.Killed += this.OnMobKilled;
				if (this.targetMob.HitPointsController != null)
				{
					this.targetMob.HitPointsController.HitReceived += this.OnHitReceivedByMob;
					IContainerBasedHPController containerBasedHPController = this.targetMob.HitPointsController as IContainerBasedHPController;
					if (containerBasedHPController != null)
					{
						containerBasedHPController.DamageResistActivated += this.OnDamageResistActivated;
						containerBasedHPController.DamageResistDeactivated += this.OnDamageResistDeactivated;
					}
				}
				if (this.targetMob.BuffsController != null)
				{
					this.targetMob.BuffsController.BuffActivated += this.OnMobBuffActivated;
				}
				if (this._resourcesCollectionEffects != null && this._resourcesCollectionEffects.Length != 0)
				{
					for (int i = 0; i < this._resourcesCollectionEffects.Length; i++)
					{
						this._resourcesCollectionEffects[i].Initialize(this);
					}
					this.targetMob.CollectingAbilityResources += this.OnCollectingAbilityResourcesByMob;
				}
				this.spriteOverlayController.Initialize(this.targetMob, this.targetMob.Renderer);
				this.hasAnimationController = (this.mobAnimationController != null);
				this.mobFootPivot = this.targetMob.TaggedPivotsGroup.GetPivot(GameMobVFXController.MobFootTagHash);
			}
		}

		// Token: 0x0600117B RID: 4475 RVA: 0x00036B0C File Offset: 0x00034D0C
		private void LateUpdate()
		{
			if (this.shadowRenderer != null && this.mobMotionController != null)
			{
				GameMobVFXController.UpdateShadowTransform(this.shadowRenderer.transform, this.mobMotionController, this.initialShadowOffset, false);
			}
			if (this._animationDependentEffects != null && this._animationDependentEffects.Length != 0)
			{
				for (int i = 0; i < this._animationDependentEffects.Length; i++)
				{
					this._animationDependentEffects[i].SyncEffect(this);
				}
			}
			ShakeImpulse shakeImpulse = this.currentShakeImpulse;
			if (shakeImpulse != null)
			{
				shakeImpulse.Update();
			}
			this.UpdateCustomBuffEffectColor();
			this.ActivateTileEffect();
			this.ActivateFootstepEffect();
		}

		// Token: 0x0600117C RID: 4476 RVA: 0x00036BA4 File Offset: 0x00034DA4
		private void OnHitReceivedByMob(IDamageable hitPointsController, object sender, IHitPointsChangingArgs args)
		{
			DamageGenerator.DamageSendingArgs damageSendingArgs = args as DamageGenerator.DamageSendingArgs;
			if (damageSendingArgs == null || sender as BaseGameMob == this.targetMob)
			{
				return;
			}
			if (!damageSendingArgs.disableTargetReaction)
			{
				if (this.targetMob.IsAlive() && args.Amount > 0f)
				{
					GameMobVFXController.ActivateEffects(this._damageReactionEffects, this.targetMob, ref this.damageReactionEffectsLastTime, sender as Component);
				}
				ShakeImpulse shakeImpulse = this.currentShakeImpulse;
				if (shakeImpulse != null)
				{
					shakeImpulse.ResetShakeImpulse();
				}
				this.currentShakeImpulse = ((sender is PlayerBehaviour) ? this.playerShakeImpulse : this.mobShakeImpulse);
				this.currentShakeImpulse.AddShakeImpulse(this.ShakeImpulseTransform);
			}
		}

		// Token: 0x0600117D RID: 4477 RVA: 0x00036C4C File Offset: 0x00034E4C
		private void OnMobAbilityActivationInterrupted(IAbility interruptedAbility)
		{
			GameMobVFXController.ActivateEffects(this._abilityActivationInterruptionEffects, this.targetMob, ref this.activationInterruptionEffectsLastTime, null);
		}

		// Token: 0x0600117E RID: 4478 RVA: 0x00036C68 File Offset: 0x00034E68
		private void OnCollectingAbilityResourcesByMob(BaseGameMob mob, AbilityResourcesCollector resourcesCollector, float collectionDuration)
		{
			foreach (AbilityResourcesCollector.RequiredResourceInfo requiredResourceInfo in resourcesCollector.LastCollectedResourcesInfo.RequiredResourcesInfo)
			{
				int num = 0;
				while (num < this._resourcesCollectionEffects.Length && !this._resourcesCollectionEffects[num].TryActivate(requiredResourceInfo.resourceType, collectionDuration))
				{
					num++;
				}
			}
		}

		// Token: 0x0600117F RID: 4479 RVA: 0x00036CC4 File Offset: 0x00034EC4
		private void OnAnimationEventFired(string arg)
		{
			if (GameMobVFXController.IsEventArg(arg, "attack"))
			{
				GameMobVFXController.ActivateEffects(this._attackEffects, this.targetMob, ref this.attackEffectsLastTime, null);
				return;
			}
			if (GameMobVFXController.IsEventArg(arg, "SecondaryAttack"))
			{
				GameMobVFXController.ActivateEffects(this._secondaryAttackEffects, this.targetMob, ref this.secondaryAttackEffectsLastTime, null);
				return;
			}
			if (GameMobVFXController.IsEventArg(arg, "death") && !this.deathEffectActivated)
			{
				this.deathEffectActivated = true;
				GameMobVFXController.ActivateEffects(this._deathEffects, this.targetMob, ref this.deathEffectsLastTime, null);
			}
		}

		// Token: 0x06001180 RID: 4480 RVA: 0x00036D54 File Offset: 0x00034F54
		private void OnMobKilled(IGameMob mob)
		{
			this.DestroyAllAttachedEffects();
			GameMobVFXController.DestroyEffects(this._damageReactionEffects, this.targetMob);
			GameMobVFXController.DestroyEffects(this._attackEffects, this.targetMob);
			GameMobVFXController.DestroyEffects(this._abilityActivationInterruptionEffects, this.targetMob);
			TaggedPivotGroup taggedPivotsGroup = this.targetMob.TaggedPivotsGroup;
			if (mob.IsSacrificed)
			{
				this.sacrificationIndicationEffect.CreateEffect(taggedPivotsGroup);
				return;
			}
			if (mob.HitPointsController.LastDamageSender is PlayerArmySizeLimitManager)
			{
				this.armyLimitDeathIndicationEffect.CreateEffect(taggedPivotsGroup);
				return;
			}
			this.deathIndicationEffect.CreateEffect(taggedPivotsGroup);
		}

		// Token: 0x06001181 RID: 4481 RVA: 0x00036DEC File Offset: 0x00034FEC
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.DestroyAllAttachedEffects();
			if (this.targetMob != null)
			{
				this.targetMob.AnimationEventFired -= this.OnAnimationEventFired;
				this.targetMob.AbilityPrepInterrupted -= this.OnMobAbilityActivationInterrupted;
				this.targetMob.CollectingAbilityResources -= this.OnCollectingAbilityResourcesByMob;
				this.targetMob.Killed -= this.OnMobKilled;
				if (this.targetMob.HitPointsController != null)
				{
					this.targetMob.HitPointsController.HitReceived -= this.OnHitReceivedByMob;
					IContainerBasedHPController containerBasedHPController = this.targetMob.HitPointsController as IContainerBasedHPController;
					if (containerBasedHPController != null)
					{
						containerBasedHPController.DamageResistActivated -= this.OnDamageResistActivated;
						containerBasedHPController.DamageResistDeactivated -= this.OnDamageResistDeactivated;
					}
				}
				if (this.targetMob.BuffsController != null)
				{
					this.targetMob.BuffsController.BuffActivated -= this.OnMobBuffActivated;
				}
			}
		}

		// Token: 0x06001184 RID: 4484 RVA: 0x00036FA5 File Offset: 0x000351A5
		[CompilerGenerated]
		private bool <ActivateTileEffect>g__CanEmitTileParticles|103_0()
		{
			return this.hasGroundEffectsEmitter && this.currentGroundTile != null;
		}

		// Token: 0x040009C8 RID: 2504
		private static readonly int MobColorSlotID = TaggedPivotGroup.TagToHash("MobColor");

		// Token: 0x040009C9 RID: 2505
		private static readonly int MobFootTagHash = TaggedPivotGroup.TagToHash("MobFoot");

		// Token: 0x040009CA RID: 2506
		private static readonly GameMobVFXController.VisualEffect[] RandomEffectsBuffer = new GameMobVFXController.VisualEffect[100];

		// Token: 0x040009CB RID: 2507
		private static PlayerCameraFollow currentCamera;

		// Token: 0x040009CC RID: 2508
		private static int randomEffectsCount = 0;

		// Token: 0x040009CE RID: 2510
		public SpriteRenderer shadowRenderer;

		// Token: 0x040009CF RID: 2511
		public AbilityVFXController.ObjectEffectInfo damageResistEffect;

		// Token: 0x040009D0 RID: 2512
		[SerializeField]
		private GameMobVFXController.VisualEffect[] _damageReactionEffects;

		// Token: 0x040009D1 RID: 2513
		[SerializeField]
		private GameMobVFXController.VisualEffect[] _deathEffects;

		// Token: 0x040009D2 RID: 2514
		[SerializeField]
		private GameMobVFXController.VisualEffect[] _attackEffects;

		// Token: 0x040009D3 RID: 2515
		[SerializeField]
		private GameMobVFXController.VisualEffect[] _secondaryAttackEffects;

		// Token: 0x040009D4 RID: 2516
		[SerializeField]
		private GameMobVFXController.VisualEffect[] _abilityActivationInterruptionEffects;

		// Token: 0x040009D5 RID: 2517
		public AttachableVisualEffectSpawner deathIndicationEffect;

		// Token: 0x040009D6 RID: 2518
		public AttachableVisualEffectSpawner sacrificationIndicationEffect;

		// Token: 0x040009D7 RID: 2519
		public AttachableVisualEffectSpawner armyLimitDeathIndicationEffect;

		// Token: 0x040009D8 RID: 2520
		[Space(5f)]
		[SerializeField]
		private GameMobVFXController.AnimationEffect[] _animationDependentEffects;

		// Token: 0x040009D9 RID: 2521
		[SerializeField]
		private PlayerCameraFollow.ShakeImpulse _killCameraShakeImpulse;

		// Token: 0x040009DA RID: 2522
		[Tooltip("Объект, на который воздействует импульс (если пусто - Transform моба)")]
		public Transform ShakeImpulseTransform;

		// Token: 0x040009DB RID: 2523
		[SerializeField]
		[FormerlySerializedAs("hitObjectShakeImpulse")]
		private ShakeImpulse mobShakeImpulse;

		// Token: 0x040009DC RID: 2524
		[SerializeField]
		private ShakeImpulse playerShakeImpulse;

		// Token: 0x040009DD RID: 2525
		[SerializeField]
		private OverlayEffect spriteOverlayController;

		// Token: 0x040009DE RID: 2526
		[SerializeField]
		private GameMobVFXController.ResourcesCollectionEffect[] _resourcesCollectionEffects;

		// Token: 0x040009DF RID: 2527
		private readonly Dictionary<int, GameMobVFXController.AttachableEffectsController> attachedEffectsControllers = new Dictionary<int, GameMobVFXController.AttachableEffectsController>();

		// Token: 0x040009E0 RID: 2528
		private readonly List<GameMobVFXController.BuffEffectInfo> attachedBuffEffects = new List<GameMobVFXController.BuffEffectInfo>(8);

		// Token: 0x040009E1 RID: 2529
		private BaseGameMob targetMob;

		// Token: 0x040009E2 RID: 2530
		private GameMobMotionControllerBase mobMotionController;

		// Token: 0x040009E3 RID: 2531
		private GameMobAnimationController mobAnimationController;

		// Token: 0x040009E4 RID: 2532
		private GameMobVFXController.AttachableEffectsController mobColorEffectsController;

		// Token: 0x040009E5 RID: 2533
		private float initialShadowOffset;

		// Token: 0x040009E6 RID: 2534
		private Color initialMobColor;

		// Token: 0x040009E7 RID: 2535
		private float damageReactionEffectsLastTime = float.MinValue;

		// Token: 0x040009E8 RID: 2536
		private float activationInterruptionEffectsLastTime = float.MinValue;

		// Token: 0x040009E9 RID: 2537
		private float secondaryAttackEffectsLastTime = float.MinValue;

		// Token: 0x040009EA RID: 2538
		private float attackEffectsLastTime = float.MinValue;

		// Token: 0x040009EB RID: 2539
		private float deathEffectsLastTime = float.MinValue;

		// Token: 0x040009EC RID: 2540
		private ShakeImpulse currentShakeImpulse;

		// Token: 0x040009ED RID: 2541
		private bool deathEffectActivated;

		// Token: 0x040009EE RID: 2542
		private Vector3Int? currentGroundCell;

		// Token: 0x040009EF RID: 2543
		private IMaterialUsingTile currentGroundTile;

		// Token: 0x040009F0 RID: 2544
		private float lastTileEffectTime = -1f;

		// Token: 0x040009F1 RID: 2545
		private float lastFootstepEffectEmitTime = -1f;

		// Token: 0x040009F2 RID: 2546
		private TaggedPivot mobFootPivot;

		// Token: 0x040009F3 RID: 2547
		private ParticlesSystemsManager particlesSystemsManager;

		// Token: 0x040009F4 RID: 2548
		private bool hasGroundEffectsEmitter;

		// Token: 0x040009F5 RID: 2549
		private bool hasAnimationController;

		// Token: 0x020004AC RID: 1196
		public sealed class AttachableEffect
		{
			// Token: 0x17000770 RID: 1904
			// (get) Token: 0x060024A7 RID: 9383 RVA: 0x00071B6D File Offset: 0x0006FD6D
			public GameMobVFXController.AttachableEffectsController Controller
			{
				get
				{
					return this.controller;
				}
			}

			// Token: 0x17000771 RID: 1905
			// (get) Token: 0x060024A8 RID: 9384 RVA: 0x00071B75 File Offset: 0x0006FD75
			public bool HasOwners
			{
				get
				{
					return this.effectOwners != null && this.effectOwners.Count != 0;
				}
			}

			// Token: 0x17000772 RID: 1906
			// (get) Token: 0x060024A9 RID: 9385 RVA: 0x00071B8F File Offset: 0x0006FD8F
			public int OwnersCount
			{
				get
				{
					if (this.effectOwners != null)
					{
						return this.effectOwners.Count;
					}
					return 0;
				}
			}

			// Token: 0x17000773 RID: 1907
			// (get) Token: 0x060024AA RID: 9386 RVA: 0x00071BA6 File Offset: 0x0006FDA6
			// (set) Token: 0x060024AB RID: 9387 RVA: 0x00071BAE File Offset: 0x0006FDAE
			public bool IsOneShotEffect { get; private set; }

			// Token: 0x17000774 RID: 1908
			// (get) Token: 0x060024AC RID: 9388 RVA: 0x00071BB7 File Offset: 0x0006FDB7
			public bool IsDestroyed
			{
				get
				{
					return this.isDestroyed;
				}
			}

			// Token: 0x060024AD RID: 9389 RVA: 0x00071BC0 File Offset: 0x0006FDC0
			private bool IsNewEffectOwner(object effectOwner)
			{
				if (effectOwner == null)
				{
					return false;
				}
				IBuff buff = effectOwner as IBuff;
				for (int i = 0; i < this.effectOwners.Count; i++)
				{
					object obj = this.effectOwners[i];
					if (GameMobVFXController.AttachableEffect.<IsNewEffectOwner>g__AreEqualBuffs|21_0(buff, obj as IBuff) || obj == effectOwner)
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x060024AE RID: 9390 RVA: 0x00071C14 File Offset: 0x0006FE14
			public AttachableEffect(GameMobVFXController.AttachableEffectsController controller, int effectID, IAttachableEffectArgs effectArgs, object effectOwner, bool isColorEffect)
			{
				this.ID = effectID;
				this.Priority = effectArgs.EffectPriority;
				this.mobVFXController = controller.MobVFXController;
				if (isColorEffect)
				{
					IEffectGradientData effectGradientData = effectArgs as IEffectGradientData;
					Gradient gradient = (effectGradientData != null) ? effectGradientData.EffectGradient : null;
					if (gradient != null)
					{
						this.EffectColor = new Color?(gradient.Evaluate(0f));
					}
					else
					{
						this.EffectColor = new Color?(effectArgs.EffectColor);
					}
				}
				else if ((this.particleSystem = controller.InstantiateParticleSystemEffect(effectArgs)) != null)
				{
					this.EffectObject = this.particleSystem.gameObject;
				}
				else
				{
					this.EffectObject = controller.InstantiateEffectObject(effectArgs);
				}
				this.effectOwners = new List<object>
				{
					effectOwner
				};
				this.controller = controller;
			}

			// Token: 0x060024AF RID: 9391 RVA: 0x00071CDD File Offset: 0x0006FEDD
			public bool IsValid()
			{
				return this.EffectObject != null || this.EffectColor != null;
			}

			// Token: 0x060024B0 RID: 9392 RVA: 0x00071CFA File Offset: 0x0006FEFA
			public bool HasOwner(object effectOwner)
			{
				return this.effectOwners.Contains(effectOwner);
			}

			// Token: 0x060024B1 RID: 9393 RVA: 0x00071D08 File Offset: 0x0006FF08
			public UnityEngine.Object GetEffectHolder()
			{
				if (this.particleSystem != null)
				{
					return this.particleSystem;
				}
				return this.EffectObject;
			}

			// Token: 0x060024B2 RID: 9394 RVA: 0x00071D25 File Offset: 0x0006FF25
			public bool AddOwner(object effectOwner)
			{
				if (this.IsNewEffectOwner(effectOwner))
				{
					this.effectOwners.Add(effectOwner);
					return true;
				}
				return false;
			}

			// Token: 0x060024B3 RID: 9395 RVA: 0x00071D3F File Offset: 0x0006FF3F
			public bool RemoveOwner(object effectOwner)
			{
				return this.effectOwners.Remove(effectOwner);
			}

			// Token: 0x060024B4 RID: 9396 RVA: 0x00071D50 File Offset: 0x0006FF50
			public void SetActive(bool isActive)
			{
				if (this.EffectColor != null)
				{
					this.mobVFXController.SetMobColor(isActive ? this.EffectColor.Value : this.mobVFXController.initialMobColor);
					return;
				}
				if (this.EffectObject != null)
				{
					this.EffectObject.SetActive(isActive);
				}
				if (isActive && this.particleSystem != null)
				{
					this.particleSystem.Play(true);
				}
			}

			// Token: 0x060024B5 RID: 9397 RVA: 0x00071DC8 File Offset: 0x0006FFC8
			public void ActivateAsOneShotEffect()
			{
				if (this.particleSystem == null)
				{
					return;
				}
				this.IsOneShotEffect = true;
				this.controller.DestroyParticleSystemEffect(this.particleSystem, false, true);
				this.SetActive(true);
			}

			// Token: 0x060024B6 RID: 9398 RVA: 0x00071DFC File Offset: 0x0006FFFC
			public void Destroy(bool detachFromParent = false)
			{
				if (this.isDestroyed)
				{
					return;
				}
				this.isDestroyed = true;
				GameMobVFXController.AttachableEffectsController attachableEffectsController = this.controller;
				this.controller.UnregisterEffect(this);
				this.controller = null;
				if (this.EffectColor != null)
				{
					GameMobVFXController gameMobVFXController = this.mobVFXController;
					if (gameMobVFXController != null)
					{
						gameMobVFXController.ResetMobColor();
					}
				}
				else if (this.particleSystem != null)
				{
					GameMobVFXController gameMobVFXController2 = this.mobVFXController;
					BaseGameMob baseGameMob = (gameMobVFXController2 != null) ? gameMobVFXController2.TargetMob : null;
					if (baseGameMob != null && baseGameMob.IsKilled)
					{
						ParticleSystem.ShapeModule shape = this.particleSystem.shape;
						if (shape.shapeType == ParticleSystemShapeType.SpriteRenderer)
						{
							shape.shapeType = ParticleSystemShapeType.Circle;
						}
					}
					attachableEffectsController.DestroyParticleSystemEffect(this.particleSystem, detachFromParent, false);
				}
				else if (this.EffectObject != null)
				{
					attachableEffectsController.DestroyEffectObject(this.EffectObject);
				}
				this.effectOwners.Clear();
			}

			// Token: 0x060024B7 RID: 9399 RVA: 0x00071EDE File Offset: 0x000700DE
			[CompilerGenerated]
			internal static bool <IsNewEffectOwner>g__AreEqualBuffs|21_0(IBuff buff0, IBuff buff1)
			{
				return buff0 != null && buff1 != null && buff0.ID == buff1.ID && buff0.IsConstant == buff1.IsConstant;
			}

			// Token: 0x04001932 RID: 6450
			public readonly int ID;

			// Token: 0x04001933 RID: 6451
			public readonly int Priority;

			// Token: 0x04001934 RID: 6452
			public readonly GameObject EffectObject;

			// Token: 0x04001935 RID: 6453
			public readonly Color? EffectColor;

			// Token: 0x04001936 RID: 6454
			private readonly GameMobVFXController mobVFXController;

			// Token: 0x04001937 RID: 6455
			private readonly ParticleSystem particleSystem;

			// Token: 0x04001938 RID: 6456
			private readonly List<object> effectOwners;

			// Token: 0x04001939 RID: 6457
			private GameMobVFXController.AttachableEffectsController controller;

			// Token: 0x0400193A RID: 6458
			private bool isDestroyed;
		}

		// Token: 0x020004AD RID: 1197
		public sealed class AttachableEffectsController
		{
			// Token: 0x060024B8 RID: 9400 RVA: 0x00071F04 File Offset: 0x00070104
			private static int GetControllerID(GameMobVFXController mobVFXController, int controllerSlotID)
			{
				return (17 * 23 + mobVFXController.GetInstanceID()) * 23 + controllerSlotID;
			}

			// Token: 0x060024B9 RID: 9401 RVA: 0x00071F17 File Offset: 0x00070117
			private static bool IsAlwaysVisibleEffectPriority(int effectPriority)
			{
				return effectPriority < 0;
			}

			// Token: 0x060024BA RID: 9402 RVA: 0x00071F1D File Offset: 0x0007011D
			private static bool TryActivateAsAlwaysVisibleEffect(GameMobVFXController.AttachableEffect effect)
			{
				if (GameMobVFXController.AttachableEffectsController.IsAlwaysVisibleEffectPriority(effect.Priority))
				{
					effect.SetActive(true);
					return true;
				}
				return false;
			}

			// Token: 0x060024BB RID: 9403 RVA: 0x00071F36 File Offset: 0x00070136
			public static bool MarkAsDisabled(GameMobVFXController mobVFXController, int controllerSlotID)
			{
				return GameMobVFXController.AttachableEffectsController.InitiallyDisabledControllers.Add(GameMobVFXController.AttachableEffectsController.GetControllerID(mobVFXController, controllerSlotID));
			}

			// Token: 0x060024BC RID: 9404 RVA: 0x00071F49 File Offset: 0x00070149
			public static bool MarkAsEnabled(GameMobVFXController mobVFXController, int controllerSlotID)
			{
				return GameMobVFXController.AttachableEffectsController.InitiallyDisabledControllers.Remove(GameMobVFXController.AttachableEffectsController.GetControllerID(mobVFXController, controllerSlotID));
			}

			// Token: 0x17000775 RID: 1909
			// (get) Token: 0x060024BD RID: 9405 RVA: 0x00071F5C File Offset: 0x0007015C
			// (set) Token: 0x060024BE RID: 9406 RVA: 0x00071F64 File Offset: 0x00070164
			public IUnityObjectPool<ParticleSystem> EffectsPool
			{
				get
				{
					return this.effectsPool;
				}
				set
				{
					this.effectsPool = value;
				}
			}

			// Token: 0x17000776 RID: 1910
			// (get) Token: 0x060024BF RID: 9407 RVA: 0x00071F6D File Offset: 0x0007016D
			// (set) Token: 0x060024C0 RID: 9408 RVA: 0x00071F75 File Offset: 0x00070175
			public bool IsActive
			{
				get
				{
					return this.isActive;
				}
				set
				{
					if (this.isActive == value)
					{
						return;
					}
					this.isActive = value;
					if (this.isActive)
					{
						this.UpdateActiveEffect(null);
						return;
					}
					this.SetInactive();
				}
			}

			// Token: 0x17000777 RID: 1911
			// (get) Token: 0x060024C1 RID: 9409 RVA: 0x00071F9E File Offset: 0x0007019E
			public GameMobVFXController.AttachableEffect ActiveEffect
			{
				get
				{
					return this.activeEffect;
				}
			}

			// Token: 0x060024C2 RID: 9410 RVA: 0x00071FA8 File Offset: 0x000701A8
			public AttachableEffectsController(GameMobVFXController mobVFXController, int effectsSlotID, Transform effectsParent, Vector3 effectsLocalPosition)
			{
				this.SlotID = effectsSlotID;
				this.EffectsList = new List<GameMobVFXController.AttachableEffect>(8);
				this.MobVFXController = mobVFXController;
				this.effectsParent = effectsParent;
				this.effectsLocalPosition = effectsLocalPosition;
				this.isActive = !GameMobVFXController.AttachableEffectsController.MarkAsEnabled(mobVFXController, effectsSlotID);
			}

			// Token: 0x060024C3 RID: 9411 RVA: 0x00071FF4 File Offset: 0x000701F4
			private void SetEffectParent(GameMobVFXController.AttachableEffect effect)
			{
				if (effect.EffectObject == null)
				{
					return;
				}
				Transform transform = effect.EffectObject.transform;
				transform.parent = this.effectsParent;
				transform.localPosition = this.effectsLocalPosition;
			}

			// Token: 0x060024C4 RID: 9412 RVA: 0x00072027 File Offset: 0x00070227
			private void SetActiveEffect(GameMobVFXController.AttachableEffect newActiveEffect)
			{
				if (this.activeEffect == newActiveEffect)
				{
					return;
				}
				if (this.activeEffect != null)
				{
					this.activeEffect.SetActive(false);
				}
				if (newActiveEffect != null)
				{
					newActiveEffect.SetActive(true);
				}
				this.activeEffect = newActiveEffect;
			}

			// Token: 0x060024C5 RID: 9413 RVA: 0x00072058 File Offset: 0x00070258
			private void UpdateActiveEffect(GameMobVFXController.AttachableEffect removedEffect = null)
			{
				if (removedEffect != null && this.activeEffect != removedEffect)
				{
					return;
				}
				float num = float.NegativeInfinity;
				GameMobVFXController.AttachableEffect attachableEffect = null;
				for (int i = 0; i < this.EffectsList.Count; i++)
				{
					GameMobVFXController.AttachableEffect attachableEffect2 = this.EffectsList[i];
					if (!GameMobVFXController.AttachableEffectsController.TryActivateAsAlwaysVisibleEffect(attachableEffect2))
					{
						int priority = attachableEffect2.Priority;
						if ((float)priority > num)
						{
							attachableEffect = attachableEffect2;
							num = (float)priority;
						}
					}
				}
				this.SetActiveEffect(attachableEffect);
			}

			// Token: 0x060024C6 RID: 9414 RVA: 0x000720C0 File Offset: 0x000702C0
			private void SetInactive()
			{
				this.SetActiveEffect(null);
				for (int i = 0; i < this.EffectsList.Count; i++)
				{
					GameMobVFXController.AttachableEffect attachableEffect = this.EffectsList[i];
					if (GameMobVFXController.AttachableEffectsController.IsAlwaysVisibleEffectPriority(attachableEffect.Priority))
					{
						attachableEffect.SetActive(false);
					}
				}
			}

			// Token: 0x060024C7 RID: 9415 RVA: 0x0007210B File Offset: 0x0007030B
			private void DestroyEffect(GameMobVFXController.AttachableEffect effect)
			{
				effect.Destroy(!this.MobVFXController.TargetMob.IsAlive());
			}

			// Token: 0x060024C8 RID: 9416 RVA: 0x00072128 File Offset: 0x00070328
			private bool RemoveEffect(GameMobVFXController.AttachableEffect effect = null, int effectIndex = -1)
			{
				if (effect == null)
				{
					if (effectIndex >= 0)
					{
						effect = this.EffectsList[effectIndex];
						this.EffectsList.RemoveAt(effectIndex);
						this.UpdateActiveEffect(effect);
						this.DestroyEffect(effect);
						return true;
					}
				}
				else if (this.EffectsList.Remove(effect))
				{
					this.UpdateActiveEffect(effect);
					this.DestroyEffect(effect);
					return true;
				}
				return false;
			}

			// Token: 0x060024C9 RID: 9417 RVA: 0x00072185 File Offset: 0x00070385
			private bool IsInactiveOneShotEffect(GameObject effectObject)
			{
				return effectObject == null || !effectObject.activeSelf || effectObject.transform.parent != this.effectsParent;
			}

			// Token: 0x060024CA RID: 9418 RVA: 0x000721B0 File Offset: 0x000703B0
			private int GetNewOneShotEffectIndex()
			{
				if (this.oneShotEffects == null || this.oneShotEffects.Count == 0)
				{
					return 0;
				}
				for (int i = 0; i < this.oneShotEffects.Count; i++)
				{
					if (this.IsInactiveOneShotEffect(this.oneShotEffects[i]))
					{
						return i;
					}
				}
				if (this.oneShotEffects.Count >= 8)
				{
					return -1;
				}
				return this.oneShotEffects.Count;
			}

			// Token: 0x060024CB RID: 9419 RVA: 0x0007221C File Offset: 0x0007041C
			private void AddOneShotEffect(GameMobVFXController.AttachableEffect effect, int effectIndex)
			{
				if (this.oneShotEffects == null)
				{
					this.oneShotEffects = new List<GameObject>(8);
				}
				GameObject effectObject = effect.EffectObject;
				if (effectIndex < this.oneShotEffects.Count)
				{
					this.oneShotEffects[effectIndex] = effectObject;
					return;
				}
				this.oneShotEffects.Add(effectObject);
			}

			// Token: 0x060024CC RID: 9420 RVA: 0x0007226C File Offset: 0x0007046C
			internal GameObject InstantiateEffectObject(IAttachableEffectArgs effectArgs)
			{
				if (!(effectArgs.EffectPrefab != null))
				{
					return null;
				}
				return UnityEngine.Object.Instantiate<GameObject>(effectArgs.EffectPrefab);
			}

			// Token: 0x060024CD RID: 9421 RVA: 0x0007228C File Offset: 0x0007048C
			internal ParticleSystem InstantiateParticleSystemEffect(IAttachableEffectArgs effectArgs)
			{
				GameObject effectPrefab = effectArgs.EffectPrefab;
				ParticleSystem particleSystem = null;
				ParticleSystem particleSystem2;
				if (effectPrefab != null && effectPrefab.TryGetComponent<ParticleSystem>(out particleSystem2))
				{
					if (this.effectsPool != null)
					{
						GameMobVFXController.AttachableEffectsController.EffectsPoolArgs.unityObjectPrototype = effectPrefab;
						particleSystem = this.effectsPool.TakeObject(GameMobVFXController.AttachableEffectsController.EffectsPoolArgs);
					}
					else
					{
						particleSystem = effectPrefab.InstantiateParticleSystem();
					}
					if (particleSystem != null)
					{
						GameMobVFXController.AttachableEffectsController.<InstantiateParticleSystemEffect>g__SetEffectParams|36_0(particleSystem);
						ParticleSystem.ShapeModule shape = particleSystem.shape;
						GameMobVFXController mobVFXController = this.MobVFXController;
						SpriteRenderer spriteRenderer = (mobVFXController != null) ? mobVFXController.TargetMob.Renderer : null;
						if (shape.shapeType == ParticleSystemShapeType.SpriteRenderer && spriteRenderer != null)
						{
							shape.spriteRenderer = spriteRenderer;
						}
					}
				}
				return particleSystem;
			}

			// Token: 0x060024CE RID: 9422 RVA: 0x00072333 File Offset: 0x00070533
			internal void DestroyEffectObject(GameObject effectObject)
			{
				UnityEngine.Object.Destroy(effectObject);
			}

			// Token: 0x060024CF RID: 9423 RVA: 0x0007233B File Offset: 0x0007053B
			internal void DestroyParticleSystemEffect(ParticleSystem effect, bool detachFromParent = true, bool forcePlay = false)
			{
				if (this.effectsPool != null)
				{
					if (detachFromParent)
					{
						effect.transform.parent = null;
					}
					if (forcePlay)
					{
						effect.Play(true);
					}
					this.effectsPool.ReturnObject(effect);
					return;
				}
				effect.DestroyAfterEmission(detachFromParent, forcePlay);
			}

			// Token: 0x060024D0 RID: 9424 RVA: 0x00072374 File Offset: 0x00070574
			public GameMobVFXController.AttachableEffect GetHighestPriorityEffect()
			{
				for (int i = 0; i < this.EffectsList.Count; i++)
				{
					GameMobVFXController.AttachableEffect attachableEffect = this.EffectsList[i];
					if (GameMobVFXController.AttachableEffectsController.IsAlwaysVisibleEffectPriority(attachableEffect.Priority))
					{
						return attachableEffect;
					}
				}
				return this.activeEffect;
			}

			// Token: 0x060024D1 RID: 9425 RVA: 0x000723BC File Offset: 0x000705BC
			public GameMobVFXController.AttachableEffect RegisterEffect(IAttachableEffectArgs effectArgs, object effectOwner)
			{
				int effectID = GameMobVFXController.GetEffectID(this.SlotID, effectArgs);
				int effectIndex = GameMobVFXController.GetEffectIndex(this.EffectsList, effectID);
				GameMobVFXController.AttachableEffect result = null;
				if (effectIndex == -1)
				{
					bool flag = GameMobVFXController.IsColorSlot(this.SlotID);
					if (flag)
					{
						effectArgs.EffectPrefab != null;
					}
					if (effectArgs.HasDuration)
					{
						GameMobVFXController.AttachableEffect attachableEffect = new GameMobVFXController.AttachableEffect(this, effectID, effectArgs, effectOwner, flag);
						if (attachableEffect.IsValid())
						{
							this.SetEffectParent(attachableEffect);
							if (!this.isActive)
							{
								attachableEffect.SetActive(false);
							}
							else if (!GameMobVFXController.AttachableEffectsController.TryActivateAsAlwaysVisibleEffect(attachableEffect))
							{
								if (this.activeEffect == null || attachableEffect.Priority > this.activeEffect.Priority)
								{
									this.SetActiveEffect(attachableEffect);
								}
								else
								{
									attachableEffect.SetActive(false);
								}
							}
							this.EffectsList.Add(attachableEffect);
							result = attachableEffect;
						}
					}
					else
					{
						int newOneShotEffectIndex = this.GetNewOneShotEffectIndex();
						if (newOneShotEffectIndex >= 0)
						{
							GameMobVFXController.AttachableEffect attachableEffect2 = new GameMobVFXController.AttachableEffect(this, effectID, effectArgs, effectOwner, flag);
							if (attachableEffect2.IsValid())
							{
								if (this.MobVFXController.TargetMob.IsAlive())
								{
									this.SetEffectParent(attachableEffect2);
								}
								else
								{
									attachableEffect2.EffectObject.transform.position = this.effectsParent.TransformPoint(this.effectsLocalPosition);
								}
								attachableEffect2.ActivateAsOneShotEffect();
								this.AddOneShotEffect(attachableEffect2, newOneShotEffectIndex);
								result = attachableEffect2;
							}
						}
					}
				}
				else if (this.EffectsList[effectIndex].AddOwner(effectOwner))
				{
					result = this.EffectsList[effectIndex];
				}
				return result;
			}

			// Token: 0x060024D2 RID: 9426 RVA: 0x0007252C File Offset: 0x0007072C
			public GameMobVFXController.AttachableEffect GetEffect(int effectID, object effectOwner)
			{
				int effectIndex = GameMobVFXController.GetEffectIndex(this.EffectsList, effectID);
				if (effectIndex >= 0)
				{
					GameMobVFXController.AttachableEffect attachableEffect = this.EffectsList[effectIndex];
					if (effectOwner == null || attachableEffect.HasOwner(effectOwner))
					{
						return attachableEffect;
					}
				}
				return null;
			}

			// Token: 0x060024D3 RID: 9427 RVA: 0x00072568 File Offset: 0x00070768
			public GameMobVFXController.AttachableEffect GetEffect(object effectOwner)
			{
				for (int i = 0; i < this.EffectsList.Count; i++)
				{
					GameMobVFXController.AttachableEffect attachableEffect = this.EffectsList[i];
					if (attachableEffect.HasOwner(effectOwner))
					{
						return attachableEffect;
					}
				}
				return null;
			}

			// Token: 0x060024D4 RID: 9428 RVA: 0x000725A4 File Offset: 0x000707A4
			public bool UnregisterEffect(GameMobVFXController.AttachableEffect effectToRemove)
			{
				return effectToRemove != null && this.RemoveEffect(effectToRemove, -1);
			}

			// Token: 0x060024D5 RID: 9429 RVA: 0x000725B4 File Offset: 0x000707B4
			public bool UnregisterEffect(int effectID, object effectOwner)
			{
				int effectIndex = GameMobVFXController.GetEffectIndex(this.EffectsList, effectID);
				GameMobVFXController.AttachableEffect attachableEffect = (effectIndex != -1) ? this.EffectsList[effectIndex] : null;
				if (attachableEffect != null && attachableEffect.RemoveOwner(effectOwner))
				{
					if (!attachableEffect.HasOwners)
					{
						this.RemoveEffect(null, effectIndex);
					}
					return true;
				}
				return false;
			}

			// Token: 0x060024D6 RID: 9430 RVA: 0x00072604 File Offset: 0x00070804
			public void DestroyAllEffects()
			{
				this.SetActiveEffect(null);
				if (this.oneShotEffects != null)
				{
					for (int i = 0; i < this.oneShotEffects.Count; i++)
					{
						GameObject gameObject = this.oneShotEffects[i];
						if (!this.IsInactiveOneShotEffect(gameObject))
						{
							gameObject.transform.parent = null;
						}
					}
					this.oneShotEffects.Clear();
				}
				for (int j = this.EffectsList.Count - 1; j >= 0; j--)
				{
					GameMobVFXController.AttachableEffect attachableEffect = this.EffectsList[j];
					attachableEffect.EffectObject.transform.parent = null;
					this.DestroyEffect(attachableEffect);
				}
				IUnityObjectPool<ParticleSystem> unityObjectPool = this.effectsPool;
				if (unityObjectPool != null)
				{
					unityObjectPool.NotifyPoolableObjectsParentDestroyed(this.effectsParent);
				}
				this.EffectsList.Clear();
			}

			// Token: 0x060024D8 RID: 9432 RVA: 0x000726E0 File Offset: 0x000708E0
			[CompilerGenerated]
			internal static void <InstantiateParticleSystemEffect>g__SetEffectParams|36_0(ParticleSystem particleSystem)
			{
				ParticleSystem.MainModule main = particleSystem.main;
				particleSystem.Stop();
				main.playOnAwake = false;
				main.loop = true;
				main.stopAction = ParticleSystemStopAction.None;
			}

			// Token: 0x0400193B RID: 6459
			private const int MaxOneShotEffectsCount = 8;

			// Token: 0x0400193C RID: 6460
			private static readonly HashSet<int> InitiallyDisabledControllers = new HashSet<int>();

			// Token: 0x0400193D RID: 6461
			private static readonly ParticleSystemEffectsPoolArgs EffectsPoolArgs = new ParticleSystemEffectsPoolArgs
			{
				effectWillBeReturnedManually = true
			};

			// Token: 0x0400193E RID: 6462
			public readonly int SlotID;

			// Token: 0x0400193F RID: 6463
			public readonly List<GameMobVFXController.AttachableEffect> EffectsList;

			// Token: 0x04001940 RID: 6464
			public readonly GameMobVFXController MobVFXController;

			// Token: 0x04001941 RID: 6465
			private readonly Transform effectsParent;

			// Token: 0x04001942 RID: 6466
			private readonly Vector3 effectsLocalPosition;

			// Token: 0x04001943 RID: 6467
			private List<GameObject> oneShotEffects;

			// Token: 0x04001944 RID: 6468
			private IUnityObjectPool<ParticleSystem> effectsPool;

			// Token: 0x04001945 RID: 6469
			private GameMobVFXController.AttachableEffect activeEffect;

			// Token: 0x04001946 RID: 6470
			private bool isActive;
		}

		// Token: 0x020004AE RID: 1198
		[Serializable]
		public struct VisualEffect : IWeighted
		{
			// Token: 0x17000778 RID: 1912
			// (get) Token: 0x060024D9 RID: 9433 RVA: 0x00072712 File Offset: 0x00070912
			// (set) Token: 0x060024DA RID: 9434 RVA: 0x0007271A File Offset: 0x0007091A
			float IWeighted.Weight
			{
				get
				{
					return this.activationWeight;
				}
				set
				{
					this.activationWeight = value;
				}
			}

			// Token: 0x060024DB RID: 9435 RVA: 0x00072723 File Offset: 0x00070923
			public bool Activate(BaseGameMob currentMob, ref float lastActivationTime)
			{
				if (Time.time - lastActivationTime < this.activationTimeout)
				{
					return false;
				}
				lastActivationTime = Time.time;
				return this.Activate(currentMob);
			}

			// Token: 0x060024DC RID: 9436 RVA: 0x00072748 File Offset: 0x00070948
			public bool Activate(BaseGameMob targetMob)
			{
				if (this.particleSystemPrefab != null)
				{
					if (this.particleSystem == null)
					{
						this.particleSystem = UnityEngine.Object.Instantiate<GameObject>(this.particleSystemPrefab).GetComponentOrDestroy<ParticleSystem>();
						if (this.fxTransform.IsNull())
						{
							this.particleSystem.transform.position = targetMob.HitColliderCenter;
							this.particleSystem.transform.SetParent(targetMob.transform);
						}
						else
						{
							this.particleSystem.transform.SetParent(this.fxTransform);
							this.particleSystem.transform.localPosition = default(Vector3);
							this.particleSystem.transform.localRotation = Quaternion.identity;
						}
						if (this.spawnOnChunk)
						{
							Transform transform = this.particleSystem.transform;
							Component component = targetMob.CurrentLocationChunk as Component;
							transform.SetParent((component != null) ? component.transform : null);
						}
						this.particleSystem.transform.localScale = Vector3.one;
						this.particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
						this.mainParticlesModule = this.particleSystem.main;
						this.mainParticlesModule.loop = false;
					}
					int num = (this.maxParticles > this.minParticles) ? UnityEngine.Random.Range(this.minParticles, this.maxParticles) : this.minParticles;
					if (!this.particleSystem.isPlaying && num > 0)
					{
						this.particleSystem.Emit(num);
					}
					if (this.spawnOnChunk)
					{
						this.particleSystem = null;
					}
				}
				if (this.animatorSource != null)
				{
					if (!this.animatorSource.IsPrefab())
					{
						this.animator = this.animatorSource.GetComponentOrDestroy<Animator>();
					}
					if (this.animator == null)
					{
						this.animator = UnityEngine.Object.Instantiate<GameObject>(this.animatorSource).GetComponentOrDestroy<Animator>();
						if (this.animatorTransform.IsNull())
						{
							this.animator.transform.parent = targetMob.transform;
						}
						else
						{
							this.animator.transform.parent = this.animatorTransform;
						}
						this.animator.transform.localPosition = Vector3.zero;
						this.animator.transform.localRotation = Quaternion.identity;
					}
					if (!string.IsNullOrWhiteSpace(this.activationTrigger))
					{
						this.animator.ResetTrigger(this.activationTrigger);
						this.animator.SetTrigger(this.activationTrigger);
					}
					if (this.spawnOnChunk)
					{
						Transform transform2 = this.animator.transform;
						Component component2 = targetMob.CurrentLocationChunk as Component;
						transform2.SetParent((component2 != null) ? component2.transform : null);
						this.animator = null;
					}
				}
				return true;
			}

			// Token: 0x060024DD RID: 9437 RVA: 0x000729F4 File Offset: 0x00070BF4
			public void Destroy(BaseGameMob currentMob)
			{
				if (GameApplication.IsGameStateChanging)
				{
					return;
				}
				this.particleSystem.DestroyAfterEmission(true, false);
				if (this.animatorSource.IsPrefab() && this.animator)
				{
					UnityEngine.Object.Destroy(this.animator.gameObject, this.animator.GetCurrentAnimatorStateInfo(0).length);
				}
			}

			// Token: 0x04001947 RID: 6471
			public LayerMask senderLayerMask;

			// Token: 0x04001948 RID: 6472
			[Tooltip("Спаун эффекта непосредственно на землю")]
			public bool spawnOnChunk;

			// Token: 0x04001949 RID: 6473
			[Tooltip("Принудительная активация эффекта. Если галочка не стоит, то будет выбран один случайный эффект из всех.")]
			public bool forceActivation;

			// Token: 0x0400194A RID: 6474
			public Transform fxTransform;

			// Token: 0x0400194B RID: 6475
			public GameObject particleSystemPrefab;

			// Token: 0x0400194C RID: 6476
			public float activationTimeout;

			// Token: 0x0400194D RID: 6477
			public int minParticles;

			// Token: 0x0400194E RID: 6478
			public int maxParticles;

			// Token: 0x0400194F RID: 6479
			[Range(0f, 1f)]
			[FormerlySerializedAs("activationProbability")]
			public float activationWeight;

			// Token: 0x04001950 RID: 6480
			public Transform animatorTransform;

			// Token: 0x04001951 RID: 6481
			[FormerlySerializedAs("animatorPrefab")]
			public GameObject animatorSource;

			// Token: 0x04001952 RID: 6482
			public string activationTrigger;

			// Token: 0x04001953 RID: 6483
			private ParticleSystem particleSystem;

			// Token: 0x04001954 RID: 6484
			private ParticleSystem.MainModule mainParticlesModule;

			// Token: 0x04001955 RID: 6485
			private Animator animator;
		}

		// Token: 0x020004AF RID: 1199
		[Serializable]
		public sealed class AnimationEffect
		{
			// Token: 0x060024DE RID: 9438 RVA: 0x00072A54 File Offset: 0x00070C54
			private bool IsAnimationStateReached(int currentStateHash)
			{
				return this.animationStateHash == null || currentStateHash == this.animationStateHash.Value;
			}

			// Token: 0x060024DF RID: 9439 RVA: 0x00072A73 File Offset: 0x00070C73
			private bool IsAnimationTriggerActive()
			{
				return this.animationTriggerHash == null || this.targetAnimator.GetBool(this.animationTriggerHash.Value);
			}

			// Token: 0x060024E0 RID: 9440 RVA: 0x00072A9C File Offset: 0x00070C9C
			private void UpdateEffectPosition(BaseGameMob targetMob)
			{
				Renderer renderer = targetMob.Renderer;
				if (renderer == null)
				{
					this.effectObject.transform.localPosition = default(Vector3);
					return;
				}
				this.effectObject.transform.localPosition = targetMob.transform.InverseTransformPoint(renderer.bounds.center);
			}

			// Token: 0x060024E1 RID: 9441 RVA: 0x00072AFC File Offset: 0x00070CFC
			private void Initialize(BaseGameMob targetMob)
			{
				this.effectObject = UnityEngine.Object.Instantiate<GameObject>(this.effectPrefab);
				this.effectInstance = this.effectObject.GetComponentOrDestroy<IAlterableProgressAction>();
				if (!this.effectObject.IsNull())
				{
					this.effectObject.transform.parent = targetMob.transform;
					this.effectObject.transform.localScale = Vector3.one;
					this.UpdateEffectPosition(targetMob);
					this.effectObject.SetActive(false);
				}
				if (!string.IsNullOrEmpty(this.animationStateName))
				{
					this.animationStateHash = new int?(Animator.StringToHash(this.animationStateName));
				}
				if (!string.IsNullOrEmpty(this.animationTriggerName))
				{
					this.animationTriggerHash = new int?(Animator.StringToHash(this.animationTriggerName));
				}
				this.targetAnimator = targetMob.Animator;
				this.isInitialized = true;
			}

			// Token: 0x060024E2 RID: 9442 RVA: 0x00072BCF File Offset: 0x00070DCF
			private void SetEffectActive(bool newActivityState, BaseGameMob targetMob)
			{
				if (this.isActive == newActivityState)
				{
					return;
				}
				this.effectObject.SetActive(newActivityState);
				this.UpdateEffectPosition(targetMob);
				this.isActive = newActivityState;
			}

			// Token: 0x060024E3 RID: 9443 RVA: 0x00072BF8 File Offset: 0x00070DF8
			public void SyncEffect(GameMobVFXController vfxController)
			{
				BaseGameMob targetMob = vfxController.TargetMob;
				if (!this.isInitialized)
				{
					this.Initialize(targetMob);
				}
				AnimatorStateInfo currentAnimatorStateInfo = this.targetAnimator.GetCurrentAnimatorStateInfo(0);
				if (this.IsAnimationStateReached(currentAnimatorStateInfo.shortNameHash) && this.IsAnimationTriggerActive())
				{
					this.SetEffectActive(true, targetMob);
					this.effectInstance.CurrentProgress = currentAnimatorStateInfo.normalizedTime % 1f;
					return;
				}
				if (this.isActive)
				{
					this.SetEffectActive(false, targetMob);
				}
			}

			// Token: 0x04001956 RID: 6486
			public string animationStateName;

			// Token: 0x04001957 RID: 6487
			public string animationTriggerName;

			// Token: 0x04001958 RID: 6488
			public GameObject effectPrefab;

			// Token: 0x04001959 RID: 6489
			private int? animationStateHash;

			// Token: 0x0400195A RID: 6490
			private int? animationTriggerHash;

			// Token: 0x0400195B RID: 6491
			private Animator targetAnimator;

			// Token: 0x0400195C RID: 6492
			private GameObject effectObject;

			// Token: 0x0400195D RID: 6493
			private IAlterableProgressAction effectInstance;

			// Token: 0x0400195E RID: 6494
			private bool isActive;

			// Token: 0x0400195F RID: 6495
			private bool isInitialized;
		}

		// Token: 0x020004B0 RID: 1200
		private readonly struct BuffEffectInfo
		{
			// Token: 0x060024E5 RID: 9445 RVA: 0x00072C78 File Offset: 0x00070E78
			public BuffEffectInfo(IBuff buff, int effectSlotID, int effectID)
			{
				this.EffectSlotID = effectSlotID;
				this.EffectID = effectID;
				this.Buff = buff;
			}

			// Token: 0x04001960 RID: 6496
			public readonly int EffectSlotID;

			// Token: 0x04001961 RID: 6497
			public readonly int EffectID;

			// Token: 0x04001962 RID: 6498
			public readonly IBuff Buff;
		}

		// Token: 0x020004B1 RID: 1201
		[Serializable]
		public sealed class GenericAttachableEffectInfo : IAttachableEffectArgs, IEffectColorData
		{
			// Token: 0x17000779 RID: 1913
			// (get) Token: 0x060024E6 RID: 9446 RVA: 0x00072C8F File Offset: 0x00070E8F
			public string EffectSlotTag
			{
				get
				{
					return this._effectSlotTag;
				}
			}

			// Token: 0x1700077A RID: 1914
			// (get) Token: 0x060024E7 RID: 9447 RVA: 0x00072C97 File Offset: 0x00070E97
			public GameObject EffectPrefab
			{
				get
				{
					return this._effectPrefab;
				}
			}

			// Token: 0x1700077B RID: 1915
			// (get) Token: 0x060024E8 RID: 9448 RVA: 0x00072C9F File Offset: 0x00070E9F
			public int EffectPriority
			{
				get
				{
					return (int)this._effectPriority;
				}
			}

			// Token: 0x1700077C RID: 1916
			// (get) Token: 0x060024E9 RID: 9449 RVA: 0x00072CA7 File Offset: 0x00070EA7
			object IAttachableEffectArgs.AttachmentPointID
			{
				get
				{
					return this._effectSlotTag;
				}
			}

			// Token: 0x1700077D RID: 1917
			// (get) Token: 0x060024EA RID: 9450 RVA: 0x00072CAF File Offset: 0x00070EAF
			Color IEffectColorData.EffectColor
			{
				get
				{
					return Color.clear;
				}
			}

			// Token: 0x1700077E RID: 1918
			// (get) Token: 0x060024EB RID: 9451 RVA: 0x00072CB6 File Offset: 0x00070EB6
			// (set) Token: 0x060024EC RID: 9452 RVA: 0x00072CC8 File Offset: 0x00070EC8
			public float EffectDuration
			{
				get
				{
					return Mathf.Max(this._effectDuration, 0.5f);
				}
				set
				{
					this._effectDuration = value;
				}
			}

			// Token: 0x1700077F RID: 1919
			// (get) Token: 0x060024ED RID: 9453 RVA: 0x00072CD1 File Offset: 0x00070ED1
			// (set) Token: 0x060024EE RID: 9454 RVA: 0x00072CDC File Offset: 0x00070EDC
			bool IAttachableEffectArgs.HasDuration
			{
				get
				{
					return !this.isOneShotEffect;
				}
				set
				{
				}
			}

			// Token: 0x04001963 RID: 6499
			[SerializeField]
			[Tag]
			private string _effectSlotTag = "MobHead";

			// Token: 0x04001964 RID: 6500
			[SerializeField]
			private GameObject _effectPrefab;

			// Token: 0x04001965 RID: 6501
			[SerializeField]
			private AbilityVFXController.ObjectEffectPriority _effectPriority;

			// Token: 0x04001966 RID: 6502
			[SerializeField]
			private float _effectDuration = 3f;

			// Token: 0x04001967 RID: 6503
			public bool isOneShotEffect;
		}

		// Token: 0x020004B2 RID: 1202
		[Serializable]
		public struct ResourcesCollectionEffect
		{
			// Token: 0x060024F0 RID: 9456 RVA: 0x00072CFC File Offset: 0x00070EFC
			public ResourcesCollectionEffect(AbilityResourceType targetResourceType, GameObject effectPrefab, string effectSlotTag)
			{
				this = default(GameMobVFXController.ResourcesCollectionEffect);
				this.targetResourceType = targetResourceType;
				this.effectPrefab = effectPrefab;
				this.effectSlotTag = effectSlotTag;
			}

			// Token: 0x060024F1 RID: 9457 RVA: 0x00072D1C File Offset: 0x00070F1C
			public void Initialize(GameMobVFXController controller)
			{
				if ((this.effectEmitter = this.effectPrefab.InstantiateParticleSystem()) != null)
				{
					TaggedPivot pivot = controller.TargetMob.TaggedPivotsGroup.GetPivot(this.effectSlotTag);
					ParticleSystem.MainModule main = this.effectEmitter.main;
					this.effectEmitter.Stop();
					main.simulationSpace = ParticleSystemSimulationSpace.Local;
					main.playOnAwake = false;
					main.startDelayMultiplier = 1f;
					main.loop = false;
					main.stopAction = ParticleSystemStopAction.Disable;
					if (pivot != null)
					{
						this.effectEmitter.transform.parent = pivot.CurrentGroup.GroupTransform;
						this.effectEmitter.transform.localPosition = pivot.LocalPosition;
					}
					else
					{
						this.effectEmitter.transform.parent = controller.TargetMob.transform;
						this.effectEmitter.transform.localPosition = default(Vector3);
					}
					this.effectEmitter.gameObject.SetActive(false);
				}
			}

			// Token: 0x060024F2 RID: 9458 RVA: 0x00072E20 File Offset: 0x00071020
			public void Activate(float startDelay)
			{
				this.effectEmitter.gameObject.SetActive(true);
				this.effectEmitter.main.startDelay = (this.ignoreCollectionDuration ? 0f : startDelay);
				this.effectEmitter.Play();
			}

			// Token: 0x060024F3 RID: 9459 RVA: 0x00072E71 File Offset: 0x00071071
			public bool TryActivate(AbilityResourceType collectedResoureType, float startDelay)
			{
				if (this.targetResourceType != collectedResoureType)
				{
					return false;
				}
				this.Activate(startDelay);
				return true;
			}

			// Token: 0x04001968 RID: 6504
			[SerializeField]
			private AbilityResourceType targetResourceType;

			// Token: 0x04001969 RID: 6505
			[SerializeField]
			private GameObject effectPrefab;

			// Token: 0x0400196A RID: 6506
			[SerializeField]
			[Tag]
			private string effectSlotTag;

			// Token: 0x0400196B RID: 6507
			public bool ignoreCollectionDuration;

			// Token: 0x0400196C RID: 6508
			private ParticleSystem effectEmitter;
		}
	}
}
