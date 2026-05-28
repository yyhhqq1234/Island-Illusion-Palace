using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200038C RID: 908
	[Serializable]
	public sealed class MobMadnessAbilityEffect : StateBasedAbilityEffect, GameAbilityExtensions.IExplicitUsingContextAbility
	{
		// Token: 0x1700061F RID: 1567
		// (get) Token: 0x06001DF4 RID: 7668 RVA: 0x0005EEB7 File Offset: 0x0005D0B7
		GameAbilityUsingContext GameAbilityExtensions.IExplicitUsingContextAbility.UsingContext
		{
			get
			{
				return GameAbilityUsingContext.BattleAbility;
			}
		}

		// Token: 0x06001DF5 RID: 7669 RVA: 0x0005EEBA File Offset: 0x0005D0BA
		public MobMadnessAbilityEffect()
		{
		}

		// Token: 0x06001DF6 RID: 7670 RVA: 0x0005EECF File Offset: 0x0005D0CF
		public MobMadnessAbilityEffect(MobMadnessAbilityEffect effectPrototype)
		{
			this.additionalAttackLayers = effectPrototype.additionalAttackLayers;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DF7 RID: 7671 RVA: 0x0005EEF8 File Offset: 0x0005D0F8
		protected override void SetEffectActive(Component effectTarget, bool isActive)
		{
			if (isActive)
			{
				BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
				GameMobAIController gameMobAIController = (baseGameMob != null) ? baseGameMob.AIController : null;
				if (gameMobAIController != null)
				{
					MobMadnessAbilityEffect.<>c__DisplayClass7_0 CS$<>8__locals1 = new MobMadnessAbilityEffect.<>c__DisplayClass7_0();
					CS$<>8__locals1.alliesAttackLayers = (gameMobAIController.ControllerOwner.LayerMask | this.additionalAttackLayers);
					this.storedAIStates.Add(new MobMadnessAbilityEffect.StoredAIControllerState(gameMobAIController));
					GameMobAIControllerParams gameMobAIControllerParams = gameMobAIController.CurrentParams.Clone();
					gameMobAIControllerParams.isAggressiveByDefault = true;
					gameMobAIControllerParams.shareAggression = false;
					gameMobAIControllerParams.hasResponseAggression = false;
					gameMobAIControllerParams.canBeScared = false;
					gameMobAIControllerParams.attackLayers = CS$<>8__locals1.alliesAttackLayers;
					gameMobAIControllerParams.canUseSupportAbilities = false;
					gameMobAIController.CurrentParams = gameMobAIControllerParams;
					gameMobAIController.ControllerOwner.AbilitiesController.ForAll(new Action<IAbility>(CS$<>8__locals1.<SetEffectActive>g__PassAttackLayerToAbility|0));
					return;
				}
			}
			else
			{
				for (int i = this.storedAIStates.Count - 1; i >= 0; i--)
				{
					if (this.storedAIStates[i].RestoreControllerState(effectTarget.gameObject))
					{
						this.storedAIStates.RemoveBySwap(i);
						return;
					}
				}
			}
		}

		// Token: 0x06001DF8 RID: 7672 RVA: 0x0005EFFE File Offset: 0x0005D1FE
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001DF9 RID: 7673 RVA: 0x0005F005 File Offset: 0x0005D205
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001DFA RID: 7674 RVA: 0x0005F007 File Offset: 0x0005D207
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new MobMadnessAbilityEffect((MobMadnessAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001DFB RID: 7675 RVA: 0x0005F014 File Offset: 0x0005D214
		public override void Reset()
		{
			this.storedAIStates.Clear();
			base.Reset();
		}

		// Token: 0x040010E0 RID: 4320
		public LayerMask additionalAttackLayers;

		// Token: 0x040010E1 RID: 4321
		private readonly List<MobMadnessAbilityEffect.StoredAIControllerState> storedAIStates = new List<MobMadnessAbilityEffect.StoredAIControllerState>(50);

		// Token: 0x02000572 RID: 1394
		private struct StoredAIControllerState
		{
			// Token: 0x06002725 RID: 10021 RVA: 0x0007A2C4 File Offset: 0x000784C4
			public StoredAIControllerState(GameMobAIController aiController)
			{
				this.AIController = aiController;
				this.storedParams = aiController.CurrentParams;
				GameAbilitiesController abilitiesController = aiController.ControllerOwner.AbilitiesController;
				if (abilitiesController != null)
				{
					IReadOnlyList<BaseAbility> abilities = abilitiesController.Abilities;
					int usableAbilitiesCount = abilitiesController.UsableAbilitiesCount;
					this.storedAbilityUsingLayers = new int[usableAbilitiesCount];
					for (int i = 0; i < usableAbilitiesCount; i++)
					{
						this.storedAbilityUsingLayers[i] = abilities[i].ValidObjectLayers;
					}
					return;
				}
				this.storedAbilityUsingLayers = null;
			}

			// Token: 0x06002726 RID: 10022 RVA: 0x0007A33C File Offset: 0x0007853C
			public bool RestoreControllerState(GameObject controllerOwnerObject)
			{
				int instanceID = controllerOwnerObject.GetInstanceID();
				GameMobAIController aicontroller = this.AIController;
				int? num;
				if (aicontroller == null)
				{
					num = null;
				}
				else
				{
					BaseGameMob controllerOwner = aicontroller.ControllerOwner;
					num = ((controllerOwner != null) ? new int?(controllerOwner.gameObject.GetInstanceID()) : null);
				}
				int? num2 = num;
				if (instanceID == num2.GetValueOrDefault() & num2 != null)
				{
					if (this.storedAbilityUsingLayers != null)
					{
						IReadOnlyList<BaseAbility> abilities = this.AIController.ControllerOwner.AbilitiesController.Abilities;
						for (int i = 0; i < this.storedAbilityUsingLayers.Length; i++)
						{
							abilities[i].ValidObjectLayers = this.storedAbilityUsingLayers[i];
						}
					}
					this.AIController.CurrentParams = this.storedParams;
					return true;
				}
				return false;
			}

			// Token: 0x04001C4F RID: 7247
			public readonly GameMobAIController AIController;

			// Token: 0x04001C50 RID: 7248
			private readonly GameMobAIControllerParams storedParams;

			// Token: 0x04001C51 RID: 7249
			private readonly int[] storedAbilityUsingLayers;
		}
	}
}
