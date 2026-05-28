using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.VFX;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Mobs;
using Unliving.Mobs.VFX;

namespace Unliving.Player
{
	// Token: 0x02000157 RID: 343
	public class ClosestMobActivationController : PlayerMobsActivationControllerBase<ClosestMobActivationController.Data>
	{
		// Token: 0x1400005A RID: 90
		// (add) Token: 0x06000978 RID: 2424 RVA: 0x0002042C File Offset: 0x0001E62C
		// (remove) Token: 0x06000979 RID: 2425 RVA: 0x00020464 File Offset: 0x0001E664
		public override event Action<bool> ActivationModeStateChanged;

		// Token: 0x1400005B RID: 91
		// (add) Token: 0x0600097A RID: 2426 RVA: 0x0002049C File Offset: 0x0001E69C
		// (remove) Token: 0x0600097B RID: 2427 RVA: 0x000204D4 File Offset: 0x0001E6D4
		public override event Action<BaseGameMob> MobSelected;

		// Token: 0x1400005C RID: 92
		// (add) Token: 0x0600097C RID: 2428 RVA: 0x0002050C File Offset: 0x0001E70C
		// (remove) Token: 0x0600097D RID: 2429 RVA: 0x00020544 File Offset: 0x0001E744
		public override event Action<MobActivationAbilityType> MobActivationFailed;

		// Token: 0x0600097E RID: 2430 RVA: 0x0002057C File Offset: 0x0001E77C
		public ClosestMobActivationController(PlayerBehaviour targetMob) : base(targetMob)
		{
			this.currentGame.Services.TryGet<IMobsActivationAssistanceManager>(out this.activationAssistanceManager);
			GameMobsFactory gameMobsFactory;
			if (this.currentGame.Services.TryGet<GameMobsFactory>(out gameMobsFactory))
			{
				GameMobFactionInfo factionInfo = gameMobsFactory.GetFactionInfo(this.data.mobsFaction);
				if (factionInfo.IsValid())
				{
					this.playerMobsLayerMask = 1 << factionInfo.mobsLayer;
				}
			}
			this.buffGenerators = new Dictionary<MobActivationAbilityType, IBuffsGenerator>();
			for (int i = 0; i < this.data.abilityTypesData.Length; i++)
			{
				ClosestMobActivationController.ActivationAbilityTypeData activationAbilityTypeData = this.data.abilityTypesData[i];
				IBuffsGenerator buffsGenerator = activationAbilityTypeData.buffGeneratorAsset.InstantiateBuffsGenerator();
				if (buffsGenerator != null)
				{
					IInitializable initializable = buffsGenerator as IInitializable;
					if (initializable != null)
					{
						initializable.Initialize();
					}
					this.buffGenerators.Add(activationAbilityTypeData.abilityType, buffsGenerator);
				}
			}
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x00020662 File Offset: 0x0001E862
		public override void SetSilentState(bool isActive)
		{
			if (this.isSilentModeActive == isActive)
			{
				return;
			}
			this.isSilentModeActive = isActive;
			if (isActive)
			{
				this.CompleteCurrentBuff();
			}
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x00020680 File Offset: 0x0001E880
		public override void OnUpdate()
		{
			if (this.targetMobActivationType == MobActivationAbilityType.None)
			{
				BaseGameMob baseGameMob;
				if (!this.playerInput.CurrentPointerOverTransform.IsNull() && this.playerInput.CurrentPointerOverTransform.TryGetComponent<BaseGameMob>(out baseGameMob))
				{
					if (!baseGameMob.IsPlayerMob || this.selectedMob == baseGameMob)
					{
						return;
					}
					this.selectedMob = baseGameMob;
					BaseAbility baseAbility;
					MobActivationAbilityType activationAbilityType;
					if (baseGameMob.TryGetMobActivationAbility(out baseAbility, out activationAbilityType))
					{
						this.SetMobBuffIndicatorVisibility(this.selectedMob, false);
						this.AddBuffOnMob(this.selectedMob, activationAbilityType);
					}
				}
				else
				{
					this.ResetSelectedMob();
				}
			}
			if (this.targetMobActivationType != MobActivationAbilityType.None && Time.unscaledTime > this.nextUpdateTime)
			{
				this.nextUpdateTime = Time.unscaledTime + this.data.updateStep;
				BaseGameMob x;
				if (this.TryGetClosestMobTransform(this.targetMobActivationType, out x))
				{
					if (x == this.selectedMob)
					{
						return;
					}
					this.selectedMob = x;
					this.SetMobBuffIndicatorVisibility(this.selectedMob, false);
					this.AddBuffOnMob(this.selectedMob, this.targetMobActivationType);
					Action<BaseGameMob> mobSelected = this.MobSelected;
					if (mobSelected == null)
					{
						return;
					}
					mobSelected(this.selectedMob);
					return;
				}
				else
				{
					Action<MobActivationAbilityType> mobActivationFailed = this.MobActivationFailed;
					if (mobActivationFailed != null)
					{
						mobActivationFailed(this.targetMobActivationType);
					}
					this.SetTargetMobActivationType(MobActivationAbilityType.None);
					this.InterruptInputActions();
				}
			}
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x000207BC File Offset: 0x0001E9BC
		protected override void OnPlayerActionPerformed(PlayerInputController.ActionArgs args)
		{
			if (!args.HasActionFlag(PlayerAction.PLAYER_USE_NATIVE_ABILITY_4))
			{
				if (!args.usedKeysInfo.Any((PlayerInputController.UsedKeyInfo i) => ClosestMobActivationController.<OnPlayerActionPerformed>g__IsDashActionUsed|34_0(i)))
				{
					if (this.targetMobActivationType != MobActivationAbilityType.None)
					{
						if (args.HasActionFlag(PlayerAction.ACTIVATE_CLOSEST_MOB))
						{
							this.ActivateClosestMob(MobActivationAbilityType.None);
						}
						else if (args.HasActionFlag(PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_01))
						{
							this.ActivateClosestMob(MobActivationAbilityType.Fighters);
						}
						else if (args.HasActionFlag(PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_02))
						{
							this.ActivateClosestMob(MobActivationAbilityType.Ranged);
						}
						else if (args.HasActionFlag(PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_03))
						{
							this.ActivateClosestMob(MobActivationAbilityType.Giants);
						}
						else if (args.HasActionFlag(PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_04))
						{
							this.ActivateClosestMob(MobActivationAbilityType.Unholy);
						}
					}
					if (args.HasActionFlag(PlayerAction.FIND_CLOSEST_MOB_TYPE_01))
					{
						this.SetTargetMobActivationType(MobActivationAbilityType.Fighters);
						return;
					}
					if (args.HasActionFlag(PlayerAction.FIND_CLOSEST_MOB_TYPE_02))
					{
						this.SetTargetMobActivationType(MobActivationAbilityType.Ranged);
						return;
					}
					if (args.HasActionFlag(PlayerAction.FIND_CLOSEST_MOB_TYPE_03))
					{
						this.SetTargetMobActivationType(MobActivationAbilityType.Giants);
						return;
					}
					if (args.HasActionFlag(PlayerAction.FIND_CLOSEST_MOB_TYPE_04))
					{
						this.SetTargetMobActivationType(MobActivationAbilityType.Unholy);
						return;
					}
					this.SetTargetMobActivationType(MobActivationAbilityType.None);
					return;
				}
			}
			this.SetTargetMobActivationType(MobActivationAbilityType.None);
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x000208BC File Offset: 0x0001EABC
		private void SetTargetMobActivationType(MobActivationAbilityType activationAbilityType)
		{
			if (this.targetMobActivationType == activationAbilityType)
			{
				return;
			}
			if (activationAbilityType == MobActivationAbilityType.None)
			{
				this.targetMobActivationType = MobActivationAbilityType.None;
				this.ResetSelectedMob();
				Action<bool> activationModeStateChanged = this.ActivationModeStateChanged;
				if (activationModeStateChanged == null)
				{
					return;
				}
				activationModeStateChanged(false);
				return;
			}
			else
			{
				this.targetMobActivationType = activationAbilityType;
				Action<bool> activationModeStateChanged2 = this.ActivationModeStateChanged;
				if (activationModeStateChanged2 == null)
				{
					return;
				}
				activationModeStateChanged2(true);
				return;
			}
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x0002090D File Offset: 0x0001EB0D
		private void ResetSelectedMob()
		{
			if (this.selectedMob.IsNull())
			{
				return;
			}
			this.SetMobBuffIndicatorVisibility(this.selectedMob, true);
			this.CompleteCurrentBuff();
			this.InterruptInputActions();
			this.selectedMob = null;
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x00020940 File Offset: 0x0001EB40
		private void ActivateClosestMob(MobActivationAbilityType activationType)
		{
			BaseGameMob baseGameMob;
			if (this.TryGetClosestMobTransform(activationType, out baseGameMob))
			{
				this.CompleteCurrentBuff();
				this.playerInput.SetPointerOverTransformOverride(baseGameMob.transform);
				this.playerInput.PerformExternalAction(this.data.mobActivationAction, false);
			}
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x00020988 File Offset: 0x0001EB88
		private void AddBuffOnMob(BaseGameMob mob, MobActivationAbilityType activationAbilityType)
		{
			IBuffsController buffsController = mob.BuffsController;
			if (buffsController == null)
			{
				return;
			}
			this.CompleteCurrentBuff();
			if (this.isSilentModeActive)
			{
				return;
			}
			IBuffsGenerator buffsGenerator;
			if (this.buffGenerators.TryGetValue(activationAbilityType, out buffsGenerator))
			{
				IBuff buff = buffsGenerator.GenerateBuff(this.ControllerOwner, true);
				if (buffsController.AddBuff(buff))
				{
					this.currentBuff = buff;
				}
			}
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x000209DD File Offset: 0x0001EBDD
		private void CompleteCurrentBuff()
		{
			IBuff buff = this.currentBuff;
			if (buff != null)
			{
				buff.Complete();
			}
			this.currentBuff = null;
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x000209F8 File Offset: 0x0001EBF8
		private bool TryGetClosestMobTransform(MobActivationAbilityType activationType, out BaseGameMob closestMob)
		{
			ClosestMobActivationController.<>c__DisplayClass40_0 CS$<>8__locals1;
			CS$<>8__locals1.position = this.playerInput.CurrentWorldCursorPosition;
			closestMob = this.activationAssistanceManager.GetClosestHighlightedMob(CS$<>8__locals1.position, activationType);
			if (closestMob != null)
			{
				return true;
			}
			Array.Clear(this.mobsBuffer, 0, this.nearestMobsCount);
			this.nearestMobsCount = Physics2D.OverlapCircleNonAlloc(CS$<>8__locals1.position, this.data.maxActivationRadius, this.mobsBuffer, this.playerMobsLayerMask);
			ClosestMobActivationController.<>c__DisplayClass40_1 CS$<>8__locals2;
			CS$<>8__locals2.minDistance = float.MaxValue;
			Transform transform = null;
			for (int i = 0; i < this.nearestMobsCount; i++)
			{
				Collider2D collider2D = this.mobsBuffer[i];
				if (!(collider2D == null) && this.IsValidMob(collider2D, activationType))
				{
					ClosestMobActivationController.<TryGetClosestMobTransform>g__CheckDistance|40_0(collider2D.transform, ref transform, ref CS$<>8__locals1, ref CS$<>8__locals2);
				}
			}
			return transform != null && transform.TryGetComponent<BaseGameMob>(out closestMob);
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x00020AD8 File Offset: 0x0001ECD8
		private void SetMobBuffIndicatorVisibility(BaseGameMob mob, bool isVisible)
		{
			if (mob.IsNull())
			{
				return;
			}
			IBuffsGenerator buffsGenerator;
			IBuff assistanceBuff = this.activationAssistanceManager.GetAssistanceBuff(mob, out buffsGenerator);
			GameMobVFXController gameMobVFXController;
			if (assistanceBuff != null && mob.TryGetComponent<GameMobVFXController>(out gameMobVFXController))
			{
				IList list = assistanceBuff.VFXData as IList;
				IAttachableEffectArgs effectArgs = (IAttachableEffectArgs)(((list != null) ? list[0] : null) ?? assistanceBuff.VFXData);
				GameMobVFXController.AttachableEffect attachedEffect = gameMobVFXController.GetAttachedEffect(effectArgs, assistanceBuff);
				if (attachedEffect != null && !attachedEffect.EffectObject.IsNull())
				{
					attachedEffect.EffectObject.SetActive(isVisible);
				}
			}
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x00020B5C File Offset: 0x0001ED5C
		private bool IsValidMob(Collider2D mobCollider, MobActivationAbilityType activationType)
		{
			BaseGameMob mob;
			BaseAbility baseAbility;
			MobActivationAbilityType mobActivationAbilityType;
			return activationType == MobActivationAbilityType.None || (mobCollider.TryGetComponent<BaseGameMob>(out mob) && mob.TryGetMobActivationAbility(out baseAbility, out mobActivationAbilityType) && mobActivationAbilityType == activationType);
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x00020B8A File Offset: 0x0001ED8A
		private void InterruptInputActions()
		{
			this.playerInput.SetPlayerActionInterrupted(PlayerAction.FIND_CLOSEST_MOB_TYPE_01);
			this.playerInput.SetPlayerActionInterrupted(PlayerAction.FIND_CLOSEST_MOB_TYPE_02);
			this.playerInput.SetPlayerActionInterrupted(PlayerAction.FIND_CLOSEST_MOB_TYPE_03);
			this.playerInput.SetPlayerActionInterrupted(PlayerAction.FIND_CLOSEST_MOB_TYPE_04);
		}

		// Token: 0x0600098B RID: 2443 RVA: 0x00020BC0 File Offset: 0x0001EDC0
		[CompilerGenerated]
		internal static bool <OnPlayerActionPerformed>g__IsDashActionUsed|34_0(PlayerInputController.UsedKeyInfo keyInfo)
		{
			return keyInfo.Action == PlayerAction.PLAYER_USE_NATIVE_ABILITY_4;
		}

		// Token: 0x0600098C RID: 2444 RVA: 0x00020BCC File Offset: 0x0001EDCC
		[CompilerGenerated]
		internal static void <TryGetClosestMobTransform>g__CheckDistance|40_0(Transform transform, ref Transform closestMobTransform, ref ClosestMobActivationController.<>c__DisplayClass40_0 A_2, ref ClosestMobActivationController.<>c__DisplayClass40_1 A_3)
		{
			float num = (A_2.position - transform.position).SqrMagnitude();
			if (num < A_3.minDistance)
			{
				A_3.minDistance = num;
				closestMobTransform = transform;
			}
		}

		// Token: 0x04000587 RID: 1415
		private const PlayerAction ClosestMobActivationAction = PlayerAction.ACTIVATE_CLOSEST_MOB;

		// Token: 0x04000588 RID: 1416
		private const PlayerAction ActivationActionType01 = PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_01;

		// Token: 0x04000589 RID: 1417
		private const PlayerAction ActivationActionType02 = PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_02;

		// Token: 0x0400058A RID: 1418
		private const PlayerAction ActivationActionType03 = PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_03;

		// Token: 0x0400058B RID: 1419
		private const PlayerAction ActivationActionType04 = PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_04;

		// Token: 0x0400058C RID: 1420
		private const PlayerAction FindMobActionType01 = PlayerAction.FIND_CLOSEST_MOB_TYPE_01;

		// Token: 0x0400058D RID: 1421
		private const PlayerAction FindMobActionType02 = PlayerAction.FIND_CLOSEST_MOB_TYPE_02;

		// Token: 0x0400058E RID: 1422
		private const PlayerAction FindMobActionType03 = PlayerAction.FIND_CLOSEST_MOB_TYPE_03;

		// Token: 0x0400058F RID: 1423
		private const PlayerAction FindMobActionType04 = PlayerAction.FIND_CLOSEST_MOB_TYPE_04;

		// Token: 0x04000590 RID: 1424
		private const PlayerAction DashAction = PlayerAction.PLAYER_USE_NATIVE_ABILITY_4;

		// Token: 0x04000594 RID: 1428
		private int nearestMobsCount;

		// Token: 0x04000595 RID: 1429
		private int playerMobsLayerMask;

		// Token: 0x04000596 RID: 1430
		private Collider2D[] mobsBuffer = new Collider2D[512];

		// Token: 0x04000597 RID: 1431
		private IMobsActivationAssistanceManager activationAssistanceManager;

		// Token: 0x04000598 RID: 1432
		private MobActivationAbilityType targetMobActivationType;

		// Token: 0x04000599 RID: 1433
		private BaseGameMob selectedMob;

		// Token: 0x0400059A RID: 1434
		private IBuff currentBuff;

		// Token: 0x0400059B RID: 1435
		private float nextUpdateTime;

		// Token: 0x0400059C RID: 1436
		private readonly Dictionary<MobActivationAbilityType, IBuffsGenerator> buffGenerators;

		// Token: 0x0400059D RID: 1437
		private bool isSilentModeActive;

		// Token: 0x0200045F RID: 1119
		[Serializable]
		public struct ActivationAbilityTypeData
		{
			// Token: 0x0400171F RID: 5919
			public MobActivationAbilityType abilityType;

			// Token: 0x04001720 RID: 5920
			public BuffsGeneratorBuilderAsset.Reference buffGeneratorAsset;
		}

		// Token: 0x02000460 RID: 1120
		[Serializable]
		public struct Data
		{
			// Token: 0x04001721 RID: 5921
			public float maxActivationRadius;

			// Token: 0x04001722 RID: 5922
			public float updateStep;

			// Token: 0x04001723 RID: 5923
			public GameMobFactions mobsFaction;

			// Token: 0x04001724 RID: 5924
			public PlayerAction mobActivationAction;

			// Token: 0x04001725 RID: 5925
			public ClosestMobActivationController.ActivationAbilityTypeData[] abilityTypesData;
		}
	}
}
