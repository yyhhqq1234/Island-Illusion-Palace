using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000149 RID: 329
	[Serializable]
	public sealed class PlayerAbilitiesController : GameAbilitiesController, IPlayerAbilitiesController, IGameAbilitiesController, IAbilitiesController, ISlotsBasedAbilitiesController<AbilityInfo>, ISlotsBasedAbilitiesController
	{
		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060008A2 RID: 2210 RVA: 0x0001C99D File Offset: 0x0001AB9D
		public BaseAbility[] AbilitySlots
		{
			get
			{
				return this.abilitySlots;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060008A3 RID: 2211 RVA: 0x0001C9A5 File Offset: 0x0001ABA5
		IReadOnlyList<IAbility> ISlotsBasedAbilitiesController.AbilitySlots
		{
			get
			{
				return this.abilitySlots;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060008A4 RID: 2212 RVA: 0x0001C9AD File Offset: 0x0001ABAD
		public int NonNativeAbilitiesStartSlotIndex
		{
			get
			{
				return this.nonNativeAbilitiesStartSlotIndex;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060008A5 RID: 2213 RVA: 0x0001C9B5 File Offset: 0x0001ABB5
		// (set) Token: 0x060008A6 RID: 2214 RVA: 0x0001C9BD File Offset: 0x0001ABBD
		public int NonNativeAbilitiesCount { get; private set; }

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060008A7 RID: 2215 RVA: 0x0001C9C6 File Offset: 0x0001ABC6
		// (set) Token: 0x060008A8 RID: 2216 RVA: 0x0001C9CE File Offset: 0x0001ABCE
		public int NativeAbilitiesCount { get; private set; }

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060008A9 RID: 2217 RVA: 0x0001C9D7 File Offset: 0x0001ABD7
		// (set) Token: 0x060008AA RID: 2218 RVA: 0x0001C9DF File Offset: 0x0001ABDF
		public int SelectedAbilitySlot { get; private set; }

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060008AB RID: 2219 RVA: 0x0001C9E8 File Offset: 0x0001ABE8
		public int ActiveAbilitySlot
		{
			get
			{
				return this.activeAbilitySlot;
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060008AC RID: 2220 RVA: 0x0001C9F0 File Offset: 0x0001ABF0
		// (set) Token: 0x060008AD RID: 2221 RVA: 0x0001C9F3 File Offset: 0x0001ABF3
		public override int UsableAbilitiesCount
		{
			get
			{
				return 0;
			}
			protected set
			{
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060008AE RID: 2222 RVA: 0x0001C9F5 File Offset: 0x0001ABF5
		public bool IsSuspended
		{
			get
			{
				return this.isSuspended;
			}
		}

		// Token: 0x14000051 RID: 81
		// (add) Token: 0x060008AF RID: 2223 RVA: 0x0001CA00 File Offset: 0x0001AC00
		// (remove) Token: 0x060008B0 RID: 2224 RVA: 0x0001CA38 File Offset: 0x0001AC38
		public event Action<IAbility, int> AbilityAddedToSlot;

		// Token: 0x14000052 RID: 82
		// (add) Token: 0x060008B1 RID: 2225 RVA: 0x0001CA70 File Offset: 0x0001AC70
		// (remove) Token: 0x060008B2 RID: 2226 RVA: 0x0001CAA8 File Offset: 0x0001ACA8
		public event Action<IAbility, int> AbilityRemovedFromSlot;

		// Token: 0x14000053 RID: 83
		// (add) Token: 0x060008B3 RID: 2227 RVA: 0x0001CAE0 File Offset: 0x0001ACE0
		// (remove) Token: 0x060008B4 RID: 2228 RVA: 0x0001CB18 File Offset: 0x0001AD18
		public event Action<int> ActiveNonNativeSlotChanged;

		// Token: 0x14000054 RID: 84
		// (add) Token: 0x060008B5 RID: 2229 RVA: 0x0001CB50 File Offset: 0x0001AD50
		// (remove) Token: 0x060008B6 RID: 2230 RVA: 0x0001CB88 File Offset: 0x0001AD88
		public event Action<IPlayerAbilitiesController, int, BaseAbility.UsingArgs, BaseAbility.ActivationErrorType> ActiveAbilitySlotSelected;

		// Token: 0x14000055 RID: 85
		// (add) Token: 0x060008B7 RID: 2231 RVA: 0x0001CBC0 File Offset: 0x0001ADC0
		// (remove) Token: 0x060008B8 RID: 2232 RVA: 0x0001CBF8 File Offset: 0x0001ADF8
		public event Action<BaseAbility, BaseAbility.ActivationError> AbilityActivationFailed;

		// Token: 0x060008B9 RID: 2233 RVA: 0x0001CC30 File Offset: 0x0001AE30
		public PlayerAbilitiesController(PlayerBehaviour player, IGameAbilitiesFactory abilityFactory, IAbilityActivatedContainersFactory hpContainersAbilityFactory) : base(player, abilityFactory)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.hpContainersAbilityFactory = hpContainersAbilityFactory;
			this.playerInputController = player.PlayerInputController;
			if (this.playerInputController != null)
			{
				this.playerInputController.PlayerActionPerformed += this.OnPlayerActionPerformed;
			}
			player.Destroyed += this.OnPlayerDestroyed;
			this.currentPlayer = player;
		}

		// Token: 0x060008BA RID: 2234 RVA: 0x0001CCEC File Offset: 0x0001AEEC
		private bool AddAbilityToSlot(IAbility ability, int slot, bool checkSlot)
		{
			BaseAbility baseAbility = ability as BaseAbility;
			if (baseAbility != null && (!checkSlot || this.CanBeAddedToSlot(baseAbility, slot)))
			{
				this.abilitySlots[slot] = baseAbility;
				base.AddAbility(baseAbility);
				Action<IAbility, int> abilityAddedToSlot = this.AbilityAddedToSlot;
				if (abilityAddedToSlot != null)
				{
					abilityAddedToSlot(ability, slot);
				}
				return true;
			}
			return false;
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x0001CD37 File Offset: 0x0001AF37
		private bool IsIgnoredAsNonExclusive(BaseAbility ability)
		{
			return this.exclusiveUsingAbilities.Count != 0 && !(ability == null) && !this.exclusiveUsingAbilities.Contains(ability.GetInstanceID());
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x0001CD65 File Offset: 0x0001AF65
		private bool IsValidAbilitySlot(int slotIndex)
		{
			return slotIndex >= 0 && slotIndex < this.abilitySlots.Length;
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x0001CD78 File Offset: 0x0001AF78
		private bool IsValidAbilitySlotType(int slotIndex, int abilityType)
		{
			int abilitySlotType = this.GetAbilitySlotType(slotIndex);
			return abilityType == 0 || abilitySlotType == 0 || abilityType == abilitySlotType;
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x0001CD99 File Offset: 0x0001AF99
		private bool CanBeCleared(int slot)
		{
			return !this.IsNativeAbilitySlot(slot) && this.IsValidAbilitySlot(slot) && !this.abilitySlots[slot].IsNull();
		}

		// Token: 0x060008BF RID: 2239 RVA: 0x0001CDBF File Offset: 0x0001AFBF
		private bool CanBeDropped(int abilitySlot)
		{
			return abilitySlot >= 0 && !this.IsNativeAbilitySlot(abilitySlot);
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x0001CDD1 File Offset: 0x0001AFD1
		private bool CanBeMoved(int slotIndex)
		{
			return this.IsValidAbilitySlot(slotIndex) && (this.abilitySlots[slotIndex].IsNull() || this.CanBeCleared(slotIndex));
		}

		// Token: 0x060008C1 RID: 2241 RVA: 0x0001CDF8 File Offset: 0x0001AFF8
		private int GetActualAbilitySlot(int abilitySlot)
		{
			if (this.IsNativeAbilitySlot(abilitySlot))
			{
				BaseAbility ability = this.GetAbility(abilitySlot);
				if (ability == null || ability.InUse)
				{
					return abilitySlot;
				}
				if (ability != null && this.nativeAbilitySlotsFallback != null && this.nativeAbilitySlotsFallback.Length != 0)
				{
					Vector2 vector = this.abilityUsingCursorPosition - this.currentPlayerPosition;
					float range = ability.Range;
					if (vector.x * vector.x + vector.y * vector.y > range * range)
					{
						for (int i = 0; i < this.nativeAbilitySlotsFallback.Length; i++)
						{
							PlayerAbilitiesController.AbilitySlotFallback abilitySlotFallback = this.nativeAbilitySlotsFallback[i];
							if (abilitySlotFallback.sourceSlotIndex == abilitySlot)
							{
								return abilitySlotFallback.fallbackSlotIndex;
							}
						}
					}
				}
			}
			return abilitySlot;
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x0001CEB3 File Offset: 0x0001B0B3
		private int GetActualAbilitySlot(IAbility ability)
		{
			return this.GetActualAbilitySlot(this.GetAbilitySlot(ability));
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x0001CEC2 File Offset: 0x0001B0C2
		private bool TryQueueActiveAbilitySlot(PlayerInputController.ActionArgs actionArgs, PlayerAction actionID, int slot)
		{
			if (!actionArgs.HasActionFlag(actionID))
			{
				return false;
			}
			this.targetActiveAbilitySlots.Add(slot);
			this.usedNativeAbilityActions.AddActionFlag(actionID);
			return true;
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x0001CEE8 File Offset: 0x0001B0E8
		private BaseAbility.UsingArgs GetAbilityUsingArgs(int abilitySlot)
		{
			return this.abilitiesUsingArgs[abilitySlot];
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x0001CEF4 File Offset: 0x0001B0F4
		private void SetActiveAbility(int newActiveAbilitySlot)
		{
			if (this.IsIgnoredAsNonExclusive(this.GetAbility(newActiveAbilitySlot)))
			{
				newActiveAbilitySlot = -1;
			}
			else
			{
				newActiveAbilitySlot = this.GetActualAbilitySlot(newActiveAbilitySlot);
			}
			if (newActiveAbilitySlot != this.activeAbilitySlot || this.activeAbilitySlot == -1 || newActiveAbilitySlot == -1)
			{
				BaseAbility ability = this.GetAbility(this.activeAbilitySlot);
				BaseAbility.UsingArgs usingArgs = (ability != null) ? this.GetAbilityUsingArgs(this.activeAbilitySlot) : null;
				this.activeAbilitySlot = newActiveAbilitySlot;
				if (ability != null)
				{
					ability.SetAbilityUsingBlocked(false);
					if (ability.PrepProgress < 1f)
					{
						PlayerAbilityParameters playerAbilityParams = ability.GetPlayerAbilityParams();
						if (playerAbilityParams != null && playerAbilityParams.canBePartiallyPrepared)
						{
							ability.ActivateWithCurrentPrepProgress(usingArgs);
						}
						else
						{
							ability.Complete();
						}
					}
					else if (ability.WasUsed && (newActiveAbilitySlot >= 0 || (ability.InUse && PlayerAbilitiesController.<SetActiveAbility>g__TryInterruptAbility|78_0(ability))))
					{
						ability.Complete();
					}
					else
					{
						ability.Activate(usingArgs);
					}
				}
				BaseAbility ability2 = this.GetAbility(newActiveAbilitySlot);
				PlayerAbilityParameters playerAbilityParameters;
				if (ability2 != null && ability2.HasPrepTime() && ability2.TryGetExtension(out playerAbilityParameters) && playerAbilityParameters.preventAutoUsing)
				{
					ability2.SetAbilityUsingBlocked(true);
				}
			}
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x0001D00C File Offset: 0x0001B20C
		private void TryUseInputActionForNativeAbilities(PlayerInputController.ActionArgs inputActionArgs)
		{
			if (this.usedNativeAbilityActions.ActionsCount == 0)
			{
				return;
			}
			for (int i = 0; i < this.targetActiveAbilitySlots.Count; i++)
			{
				int actualAbilitySlot = this.GetActualAbilitySlot(this.targetActiveAbilitySlots[i]);
				if (this.IsNativeAbilitySlot(actualAbilitySlot) && this.IsValidAbilitySlot(actualAbilitySlot))
				{
					BaseAbility baseAbility = this.abilitySlots[actualAbilitySlot];
					BaseAbility.UsingArgs abilityUsingArgs = this.GetAbilityUsingArgs(actualAbilitySlot);
					if (!(baseAbility == null))
					{
						BaseAbility.ActivationError activationError = baseAbility.GetActivationError(abilityUsingArgs);
						if (activationError.type == BaseAbility.ActivationErrorType.None)
						{
							inputActionArgs.Use(PlayerInputController.GetAbilitySlotAction(actualAbilitySlot));
							this.playerInputController.LockInput(inputActionArgs, this.usedNativeAbilityActions);
							break;
						}
						this.OnAbilityActivationFailed(baseAbility, activationError);
					}
				}
			}
			this.usedNativeAbilityActions.ClearFlags();
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x0001D0CC File Offset: 0x0001B2CC
		private void SelectNonNativeActiveSlot(int slotIndex)
		{
			slotIndex += this.nonNativeAbilitiesStartSlotIndex;
			if (this.IsNativeAbilitySlot(slotIndex))
			{
				return;
			}
			if (this.currentPlayer.IsNull() || !this.useActiveAbilitySlot)
			{
				return;
			}
			if (!this.IsBusyAbilitySlot(slotIndex))
			{
				return;
			}
			this.SelectedAbilitySlot = slotIndex;
			Action<int> activeNonNativeSlotChanged = this.ActiveNonNativeSlotChanged;
			if (activeNonNativeSlotChanged == null)
			{
				return;
			}
			activeNonNativeSlotChanged(this.SelectedAbilitySlot);
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x0001D12A File Offset: 0x0001B32A
		private void UseNonNativeSlot(int slotIndex)
		{
			slotIndex += this.nonNativeAbilitiesStartSlotIndex;
			this.targetActiveAbilitySlots.Add(slotIndex);
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x0001D142 File Offset: 0x0001B342
		private void ResetActiveAbility()
		{
			this.SetActiveAbility(-1);
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x0001D14B File Offset: 0x0001B34B
		private bool IsAbilityWithoutUsingTimeout(int abilityID)
		{
			return this.abilitiesWithoutTimeoutSet != null && this.abilitiesWithoutTimeoutSet.Contains(abilityID);
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x0001D164 File Offset: 0x0001B364
		private bool IsExternalAbility(BaseAbility ability, out int abilityIndex)
		{
			return (abilityIndex = this.externalAbilities.IndexOf(ability)) != -1;
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x0001D188 File Offset: 0x0001B388
		private void ActivateExternalAbilities()
		{
			for (int i = this.externalAbilities.Count - 1; i >= 0; i--)
			{
				BaseAbility baseAbility = this.externalAbilities[i];
				if (!baseAbility.IsActivated)
				{
					baseAbility.Activate(this.externalAbilitiesArgs[i]);
				}
			}
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x0001D1D0 File Offset: 0x0001B3D0
		private void TryRemoveForcedUseAbility(BaseAbility ability)
		{
			int actualAbilitySlot = this.GetActualAbilitySlot(ability);
			if (actualAbilitySlot >= 0)
			{
				this.forcedUseAbilitySlots.Remove(actualAbilitySlot);
			}
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x0001D1F8 File Offset: 0x0001B3F8
		private void ApplyAbilityUsingImpulse(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			if (abilityUsingArgs == null)
			{
				return;
			}
			AbilityFactoryPrototype abilityPrototype = base.GetAbilityPrototype(ability.ID);
			float num = (abilityPrototype != null) ? abilityPrototype.playerAbilityUsingImpulse : 0f;
			if (num > 0f)
			{
				Vector2 vector = this.abilityUsingCursorPosition - this.currentPlayerPosition;
				Vector2 vector2 = vector;
				Component component = null;
				if (abilityUsingArgs.HasTargetsList)
				{
					IList<Component> targetsList = abilityUsingArgs.targetsList;
					int targetsCount = abilityUsingArgs.TargetsCount;
					float num2 = float.PositiveInfinity;
					for (int i = 0; i < targetsCount; i++)
					{
						Component component2 = targetsList[i];
						Vector2 vector3 = component2.transform.position - this.currentPlayerPosition;
						float num3 = vector3.x * vector3.x + vector3.y * vector3.y;
						if (num3 < num2)
						{
							component = component2;
							vector2 = vector3;
							num2 = num3;
						}
					}
				}
				else
				{
					if (abilityUsingArgs.HasTargetObject)
					{
						vector = abilityUsingArgs.targetPosition - this.currentPlayerPosition;
					}
					if (vector.SqrMagnitude() > 0.0001f)
					{
						vector2 = vector;
					}
				}
				vector2.Normalize();
				vector2 *= num;
				this.currentPlayer.AddMovementImpulse(vector2, false);
				if (component != null)
				{
					BaseGameMob baseGameMob = component.CastOrGetComponent<BaseGameMob>();
					if (baseGameMob == null)
					{
						return;
					}
					baseGameMob.AddMovementImpulse(vector2, false);
				}
			}
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x0001D340 File Offset: 0x0001B540
		private GameObject DropAbility(int abilitySlot, Vector2? targetPosition, float dropRadius, float maxRandomRotation, bool force)
		{
			if (!force && !this.CanBeDropped(abilitySlot))
			{
				return null;
			}
			BaseAbility ability = this.GetAbility(abilitySlot);
			if (ability == null)
			{
				return null;
			}
			if (this.AbilitiesFactory == null)
			{
				return null;
			}
			AbilityID id = (AbilityID)ability.ID;
			int abilityLevel;
			ability.TryGetAbilityLevel(out abilityLevel, 1);
			IGameAbilitiesFactory abilitiesFactory = this.AbilitiesFactory;
			AbilitySpecialBehaviourDescription specialBehaviourDescription = (abilitiesFactory != null) ? abilitiesFactory.GetAbilitySpecialBehaviourDescription(ability) : null;
			float reloadingProgress = ability.ReloadingProgress;
			if (this.ClearAbilitySlot(abilitySlot))
			{
				AbilityFactoryArgs query = new AbilityFactoryArgs
				{
					abilityID = id,
					abilityLevel = abilityLevel,
					objectType = MultiRepresentationObjectInstantiator.ObjectType.PickableObject,
					specialBehaviourDescription = specialBehaviourDescription,
					reloadingProgressOverride = new float?(reloadingProgress)
				};
				GameObject gameObject = (GameObject)this.AbilitiesFactory.Create(query);
				if (gameObject != null)
				{
					dropRadius = Mathf.Max(this.currentPlayer.Radius, dropRadius);
					gameObject.transform.position = (targetPosition ?? ((MonoBehaviour)this.abilitiesOwner).transform.position);
					if (dropRadius > 0f)
					{
						gameObject.transform.position += UnityEngine.Random.insideUnitCircle.normalized * dropRadius;
					}
					if (maxRandomRotation > 0f)
					{
						gameObject.transform.rotation = QuaternionExtensions.Get2DRotation(UnityEngine.Random.value * maxRandomRotation);
					}
				}
				return gameObject;
			}
			return null;
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x0001D4B4 File Offset: 0x0001B6B4
		protected override void AddAbilityToList(List<BaseAbility> abilities, BaseAbility ability)
		{
			abilities.Add(ability);
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x0001D4BD File Offset: 0x0001B6BD
		public int GetAbilitySlotType(int abilitySlot)
		{
			if (this.abilitySlotsTypes == null || abilitySlot >= this.abilitySlotsTypes.Length)
			{
				return -1;
			}
			return this.abilitySlotsTypes[abilitySlot];
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x0001D4DC File Offset: 0x0001B6DC
		public BaseAbility GetAbility(int abilitySlot)
		{
			if (this.IsValidAbilitySlot(abilitySlot))
			{
				return this.abilitySlots[abilitySlot];
			}
			return null;
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x0001D4F4 File Offset: 0x0001B6F4
		public AbilityID GetAbilityID(int abilitySlot)
		{
			BaseAbility ability = this.GetAbility(abilitySlot);
			if (!(ability == null))
			{
				return (AbilityID)ability.ID;
			}
			return AbilityID.None;
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x0001D51A File Offset: 0x0001B71A
		IAbility ISlotsBasedAbilitiesController.GetAbility(int abilitySlot)
		{
			return this.GetAbility(abilitySlot);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x0001D524 File Offset: 0x0001B724
		public bool IsCurrentlyEquipedMainBattleAbility(int abilityID)
		{
			for (int i = 0; i < this.mainBattleAbilitiesSlots.Count; i++)
			{
				if (abilityID == this.abilitySlots[this.mainBattleAbilitiesSlots[i]].ID)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x0001D568 File Offset: 0x0001B768
		public int GetAbilitySlot(IAbility ability)
		{
			if (ability == null)
			{
				return -1;
			}
			IAbility[] array = this.abilitySlots;
			return Array.IndexOf<IAbility>(array, ability);
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x0001D588 File Offset: 0x0001B788
		public int GetAbilitySlot(AbilityInfo abilityDescription)
		{
			return Array.FindIndex<BaseAbility>(this.abilitySlots, new Predicate<BaseAbility>(abilityDescription.IsMatch));
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x0001D5A8 File Offset: 0x0001B7A8
		public int GetCompatibleAbilitySlot(int abilityType)
		{
			if (this.abilitySlots.Length != 0)
			{
				for (int i = 0; i < this.abilitySlots.Length; i++)
				{
					if (this.GetAbilitySlotType(i) == abilityType)
					{
						return i;
					}
				}
			}
			return -1;
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x0001D5E0 File Offset: 0x0001B7E0
		public IAbility GetLowerLevelAbilityOfType(int abilityType)
		{
			IItemLevelProvider itemLevelProvider = null;
			for (int i = 0; i < this.abilities.Count; i++)
			{
				BaseAbility baseAbility = this.abilities[i];
				IItemLevelProvider itemLevelProvider2 = baseAbility as IItemLevelProvider;
				if (itemLevelProvider2 != null && baseAbility.Type == abilityType && (itemLevelProvider == null || itemLevelProvider.ItemLevel > itemLevelProvider2.ItemLevel))
				{
					itemLevelProvider = itemLevelProvider2;
				}
			}
			return itemLevelProvider as IAbility;
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x0001D63E File Offset: 0x0001B83E
		public bool IsNativeAbilitySlot(int abilitySlot)
		{
			return abilitySlot >= 0 && this.nonNativeAbilitiesStartSlotIndex >= 0 && abilitySlot < this.nonNativeAbilitiesStartSlotIndex;
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x0001D658 File Offset: 0x0001B858
		public bool IsNativeAbility(int abilityID)
		{
			for (int i = 0; i < this.nonNativeAbilitiesStartSlotIndex; i++)
			{
				BaseAbility baseAbility = this.abilitySlots[i];
				if (!baseAbility.IsNull() && baseAbility.ID == abilityID)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x0001D694 File Offset: 0x0001B894
		public bool HasNonNativeAbilities()
		{
			for (int i = this.nonNativeAbilitiesStartSlotIndex; i < this.abilitySlots.Length; i++)
			{
				if (!this.IsNativeAbilitySlot(i) && this.IsBusyAbilitySlot(i))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x0001D6CE File Offset: 0x0001B8CE
		public override bool IsSpecialAbility(int abilityID)
		{
			return !this.IsNativeAbility(abilityID);
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x0001D6DA File Offset: 0x0001B8DA
		public bool IsBusyAbilitySlot(int slot)
		{
			return this.IsNativeAbilitySlot(slot) || !this.IsValidAbilitySlot(slot) || !this.abilitySlots[slot].IsNull();
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x0001D700 File Offset: 0x0001B900
		public bool CanBeAddedToSlot(IAbility ability, int slot)
		{
			return this.IsValidAbilitySlot(slot) && !this.IsBusyAbilitySlot(slot) && (ability != null && ability.ShouldBeActivatedByOwner()) && this.IsValidAbilitySlotType(slot, ability.Type);
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x0001D730 File Offset: 0x0001B930
		public bool CanBeAddedAsMainBattleAbility(BaseAbility ability)
		{
			return this.FindMainBattleAbilityExchangeSlot(ability) >= 0;
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x0001D73F File Offset: 0x0001B93F
		public bool CanBeActivated(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			return ability != null && ability.CanBeActivated(usingArgs);
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x0001D753 File Offset: 0x0001B953
		public bool CanBeActivated(int slot, BaseAbility.UsingArgs usingArgs)
		{
			return this.IsValidAbilitySlot(slot) && this.CanBeActivated(this.abilitySlots[slot], usingArgs);
		}

		// Token: 0x060008E3 RID: 2275 RVA: 0x0001D76F File Offset: 0x0001B96F
		public bool AddAbilityToSlot(IAbility ability, int slot)
		{
			return this.AddAbilityToSlot(ability, slot, true);
		}

		// Token: 0x060008E4 RID: 2276 RVA: 0x0001D77A File Offset: 0x0001B97A
		IAbility ISlotsBasedAbilitiesController<AbilityInfo>.AddAbilityToSlot(AbilityInfo abilityDescription, int abilitySlot, bool force)
		{
			return this.AddAbilityToSlot(abilityDescription, abilitySlot, force);
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x0001D788 File Offset: 0x0001B988
		public BaseAbility AddAbilityToSlot(AbilityInfo abilityDescription, int slot, bool force = false)
		{
			BaseAbility baseAbility = base.CreateAbility(ref abilityDescription);
			if (this.AddAbilityToSlot(baseAbility, slot, !force))
			{
				return baseAbility;
			}
			if (baseAbility != null)
			{
				baseAbility.Destroy();
			}
			return null;
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x0001D7B8 File Offset: 0x0001B9B8
		public BaseAbility GetMainBattleAbility()
		{
			for (int i = 0; i < this.nonNativeAbilitiesStartSlotIndex; i++)
			{
				BaseAbility baseAbility = this.abilitySlots[i];
				if (baseAbility.IsProjectileAbility(true) && baseAbility.IsDamagingAbility(null))
				{
					return baseAbility;
				}
			}
			return null;
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x0001D7F4 File Offset: 0x0001B9F4
		public AbilityID GetMainBattleAbilityID()
		{
			BaseAbility mainBattleAbility = this.GetMainBattleAbility();
			if (!(mainBattleAbility == null))
			{
				return (AbilityID)mainBattleAbility.ID;
			}
			return AbilityID.None;
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x0001D81C File Offset: 0x0001BA1C
		public int FindMainBattleAbilityExchangeSlot(BaseAbility newMainBattleAbility)
		{
			if (newMainBattleAbility.IsPlayerMainBattleAbility())
			{
				bool flag = newMainBattleAbility.IsProjectileAbility(true);
				for (int i = 0; i < this.nonNativeAbilitiesStartSlotIndex; i++)
				{
					BaseAbility baseAbility = this.abilitySlots[i];
					if (!(baseAbility == newMainBattleAbility) && (!flag || baseAbility.IsProjectileAbility(true)) && baseAbility.IsDamagingAbility(null))
					{
						return i;
					}
				}
			}
			return -1;
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x0001D874 File Offset: 0x0001BA74
		public BaseAbility AddMainBattleAbility(AbilityInfo abilityDescription, bool dropCurrentAbility = true)
		{
			BaseAbility baseAbility = base.CreateAbility(ref abilityDescription);
			int num = this.FindMainBattleAbilityExchangeSlot(baseAbility);
			if (num < 0)
			{
				if (baseAbility != null)
				{
					baseAbility.Destroy();
				}
				return null;
			}
			bool flag;
			if (dropCurrentAbility)
			{
				flag = (this.DropAbility(num, null, 0f, 0f, true) != null);
			}
			else
			{
				flag = this.ClearAbilitySlot(num);
			}
			if (!flag || !this.AddAbilityToSlot(baseAbility, num, false))
			{
				baseAbility.Destroy();
				return null;
			}
			return baseAbility;
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x0001D8E8 File Offset: 0x0001BAE8
		public bool ClearAbilitySlot(int abilitySlot)
		{
			if (this.IsValidAbilitySlot(abilitySlot))
			{
				BaseAbility baseAbility = this.abilitySlots[abilitySlot];
				if (!baseAbility.IsNull())
				{
					int num = this.abilities.IndexOf(baseAbility);
					if (num != -1)
					{
						this.abilitySlots[abilitySlot] = null;
						base.RemoveAbilityAt(num);
						Action<IAbility, int> abilityRemovedFromSlot = this.AbilityRemovedFromSlot;
						if (abilityRemovedFromSlot != null)
						{
							abilityRemovedFromSlot(baseAbility, abilitySlot);
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x0001D948 File Offset: 0x0001BB48
		public void SelectNextSlot()
		{
			if (!this.HasNonNativeAbilities())
			{
				return;
			}
			int num = this.SelectedAbilitySlot + 1;
			while (!this.IsBusyAbilitySlot(num) || !this.IsValidAbilitySlot(num) || this.IsNativeAbilitySlot(num))
			{
				if (num >= this.abilitySlots.Length)
				{
					num = this.nonNativeAbilitiesStartSlotIndex - 1;
				}
				num++;
			}
			this.SelectNonNativeActiveSlot(num);
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x0001D9A4 File Offset: 0x0001BBA4
		public void SelectPrevSlot()
		{
			if (!this.HasNonNativeAbilities())
			{
				return;
			}
			int num = this.SelectedAbilitySlot - 1;
			while (!this.IsBusyAbilitySlot(num) || !this.IsValidAbilitySlot(num) || this.IsNativeAbilitySlot(num))
			{
				if (num < 0)
				{
					num = this.abilitySlots.Length;
				}
				num--;
			}
			this.SelectNonNativeActiveSlot(num);
		}

		// Token: 0x060008ED RID: 2285 RVA: 0x0001D9F8 File Offset: 0x0001BBF8
		public void SetExclusiveMode(BaseAbility ability, bool isActive)
		{
			if (ability == null)
			{
				return;
			}
			if (isActive)
			{
				this.exclusiveUsingAbilities.Add(ability.GetInstanceID());
				return;
			}
			this.exclusiveUsingAbilities.Remove(ability.GetInstanceID());
		}

		// Token: 0x060008EE RID: 2286 RVA: 0x0001DA2C File Offset: 0x0001BC2C
		public bool SwapAbilities(int slot0, int slot1)
		{
			if (!this.CanBeMoved(slot0) || !this.CanBeMoved(slot1) || (this.abilitySlots[slot0].IsNull() && this.abilitySlots[slot1].IsNull()))
			{
				return false;
			}
			BaseAbility baseAbility = this.abilitySlots[slot0];
			BaseAbility baseAbility2 = this.abilitySlots[slot1];
			if (baseAbility != null && !this.IsValidAbilitySlotType(slot1, baseAbility.Type))
			{
				return false;
			}
			if (baseAbility2 != null && !this.IsValidAbilitySlotType(slot0, baseAbility2.Type))
			{
				return false;
			}
			this.abilitySlots[slot0] = baseAbility2;
			this.abilitySlots[slot1] = baseAbility;
			return true;
		}

		// Token: 0x060008EF RID: 2287 RVA: 0x0001DAC7 File Offset: 0x0001BCC7
		public GameObject DropAbility(int abilitySlot, Vector2? targetPosition = null, float dropRadius = 0f, float maxRandomRotation = 0f)
		{
			return this.DropAbility(abilitySlot, targetPosition, dropRadius, maxRandomRotation, false);
		}

		// Token: 0x060008F0 RID: 2288 RVA: 0x0001DAD8 File Offset: 0x0001BCD8
		public void UseExternalAbility(AbilityInfo abilityInfo, BaseAbility.UsingArgs usingArgs)
		{
			IGameAbilitiesFactory abilitiesFactory;
			if (abilityInfo.abilityID.IsHPContainerAbility())
			{
				abilitiesFactory = this.hpContainersAbilityFactory;
			}
			else
			{
				abilitiesFactory = this.AbilitiesFactory;
			}
			BaseAbility ability = base.CreateAbility(abilitiesFactory, ref abilityInfo);
			this.UseExternalAbility(ability, usingArgs);
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x0001DB14 File Offset: 0x0001BD14
		public void UseExternalAbility(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			if (ability == null)
			{
				return;
			}
			if (base.AddAbility(ability))
			{
				this.externalAbilities.Add(ability);
				this.externalAbilitiesArgs[this.externalAbilities.Count - 1] = usingArgs;
				return;
			}
			ability.Destroy();
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x0001DB51 File Offset: 0x0001BD51
		public void SetSuspended()
		{
			this.isSuspended = true;
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x0001DB5C File Offset: 0x0001BD5C
		public void Initialize(IList<AbilityInfo> nativeAbilities, IList<AbilityTypes> nativeAbilityTypes, IList<AbilityInfo> abilities, IList<AbilityTypes> abilityTypes)
		{
			base.RemoveAllAbilities(null);
			if (abilityTypes == null)
			{
				this.abilitySlotsTypes = Array.Empty<int>();
			}
			else
			{
				this.abilitySlotsTypes = new int[nativeAbilities.Count + abilities.Count];
				for (int i = 0; i < this.abilitySlotsTypes.Length; i++)
				{
					if (i < nativeAbilities.Count)
					{
						this.abilitySlotsTypes[i] = (int)((i < nativeAbilityTypes.Count) ? nativeAbilityTypes[i] : AbilityTypes.None);
					}
					else
					{
						int num = i - nativeAbilities.Count;
						this.abilitySlotsTypes[i] = (int)((num < abilityTypes.Count) ? abilityTypes[num] : AbilityTypes.None);
					}
				}
			}
			this.NativeAbilitiesCount = nativeAbilities.Count;
			this.NonNativeAbilitiesCount = abilities.Count;
			this.nonNativeAbilitiesStartSlotIndex = 0;
			this.activeAbilitySlot = -1;
			this.abilitySlots = new BaseAbility[nativeAbilities.Count + abilities.Count];
			this.abilitiesUsingArgs = new BaseAbility.UsingArgs[this.abilitySlots.Length];
			for (int j = 0; j < this.abilitiesUsingArgs.Length; j++)
			{
				this.abilitiesUsingArgs[j] = new BaseAbility.UsingArgs();
			}
			if (nativeAbilities != null && nativeAbilities.Count != 0)
			{
				int count = nativeAbilities.Count;
				int num2 = 0;
				for (int k = 0; k < count; k++)
				{
					AbilityInfo abilityInfo = nativeAbilities[k];
					if (abilityInfo.abilityID != AbilityID.None)
					{
						BaseAbility baseAbility = this.AddAbilityToSlot(abilityInfo, num2, false);
						if (baseAbility != null)
						{
							if (baseAbility.IsPlayerMainBattleAbility())
							{
								this.mainBattleAbilitiesSlots.Add(num2);
							}
							num2++;
						}
					}
				}
				this.nonNativeAbilitiesStartSlotIndex = nativeAbilities.Count;
			}
			int num3 = -1;
			if (abilities != null && abilities.Count != 0)
			{
				int num4 = Mathf.Min(this.abilitySlots.Length, this.nonNativeAbilitiesStartSlotIndex + abilities.Count) - 1;
				int num5 = this.nonNativeAbilitiesStartSlotIndex;
				for (int l = 0; l < abilities.Count; l++)
				{
					AbilityInfo abilityInfo2 = abilities[l];
					if (abilityInfo2.abilityID != AbilityID.None && this.AddAbilityToSlot(abilityInfo2, num5, false) != null && num3 == -1)
					{
						num3 = num5;
					}
					if (++num5 > num4)
					{
						break;
					}
				}
			}
			this.SelectedAbilitySlot = (this.useActiveAbilitySlot ? num3 : -1);
			if (this.abilitiesWithoutTimeout != null)
			{
				this.abilitiesWithoutTimeoutSet = new HashSet<int>();
				for (int m = 0; m < this.abilitiesWithoutTimeout.Length; m++)
				{
					this.abilitiesWithoutTimeoutSet.Add((int)this.abilitiesWithoutTimeout[m]);
				}
			}
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x0001DDC0 File Offset: 0x0001BFC0
		protected override void OnAbilityAdded(BaseAbility ability)
		{
			ability.ActivationFailed += this.OnAbilityActivationFailed;
			base.OnAbilityAdded(ability);
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x0001DDDB File Offset: 0x0001BFDB
		protected override void OnAbilityRemoved(BaseAbility ability, int abilityIndex)
		{
			ability.ActivationFailed -= this.OnAbilityActivationFailed;
			this.TryRemoveForcedUseAbility(ability);
			base.OnAbilityRemoved(ability, abilityIndex);
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x0001DE00 File Offset: 0x0001C000
		protected override void OnAbilityUsed(IAbility ability, object abilityUsingArgs)
		{
			BaseAbility baseAbility = ability as BaseAbility;
			if (baseAbility != null)
			{
				int num;
				if (this.IsExternalAbility(baseAbility, out num))
				{
					base.OnAbilityUsed(ability, abilityUsingArgs);
					return;
				}
				if (!baseAbility.WasUsed)
				{
					this.ApplyAbilityUsingImpulse(baseAbility, (BaseAbility.UsingArgs)abilityUsingArgs);
				}
			}
			base.OnAbilityUsed(ability, abilityUsingArgs);
			if (this.activeAbilitySlot < 0 && !this.forcedUseAbilitySlots.Contains(this.GetActualAbilitySlot(ability)))
			{
				ability.Complete();
			}
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x0001DE6C File Offset: 0x0001C06C
		protected override void OnAbilityCompleted(IAbility ability, object abilityUsingArgs)
		{
			base.OnAbilityCompleted(ability, abilityUsingArgs);
			BaseAbility baseAbility = (BaseAbility)ability;
			int index;
			if (this.IsExternalAbility(baseAbility, out index))
			{
				base.RemoveAbility(baseAbility);
				this.externalAbilities.RemoveAt(index);
				return;
			}
			if (this.GetAbility(this.activeAbilitySlot) == baseAbility)
			{
				this.ResetActiveAbility();
			}
			int actualAbilitySlot = this.GetActualAbilitySlot(ability);
			if (actualAbilitySlot >= 0)
			{
				this.forcedUseAbilitySlots.Remove(actualAbilitySlot);
				this.GetAbilityUsingArgs(actualAbilitySlot).Reset();
			}
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x0001DEE7 File Offset: 0x0001C0E7
		private void OnAbilityActivationFailed(BaseAbility ability, BaseAbility.ActivationError activationError)
		{
			this.TryRemoveForcedUseAbility(ability);
			Action<BaseAbility, BaseAbility.ActivationError> abilityActivationFailed = this.AbilityActivationFailed;
			if (abilityActivationFailed == null)
			{
				return;
			}
			abilityActivationFailed(ability, activationError);
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x0001DF04 File Offset: 0x0001C104
		private void OnPlayerActionPerformed(PlayerInputController.ActionArgs inputActionArgs)
		{
			if (this.isSuspended)
			{
				return;
			}
			this.targetActiveAbilitySlots.Clear();
			this.TryQueueActiveAbilitySlot(inputActionArgs, PlayerAction.PLAYER_USE_NATIVE_ABILITY_1, 0);
			this.TryQueueActiveAbilitySlot(inputActionArgs, PlayerAction.PLAYER_USE_NATIVE_ABILITY_2, 1);
			this.TryQueueActiveAbilitySlot(inputActionArgs, PlayerAction.PLAYER_USE_NATIVE_ABILITY_3, 2);
			this.TryQueueActiveAbilitySlot(inputActionArgs, PlayerAction.PLAYER_USE_NATIVE_ABILITY_4, 3);
			this.TryQueueActiveAbilitySlot(inputActionArgs, PlayerAction.PLAYER_USE_NATIVE_ABILITY_5, 4);
			if (this.useActiveAbilitySlot)
			{
				if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_1))
				{
					this.SelectNonNativeActiveSlot(0);
				}
				else if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_2))
				{
					this.SelectNonNativeActiveSlot(1);
				}
				else if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_3))
				{
					this.SelectNonNativeActiveSlot(2);
				}
				else if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_4))
				{
					this.SelectNonNativeActiveSlot(3);
				}
			}
			else if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_1))
			{
				this.UseNonNativeSlot(0);
			}
			else if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_2))
			{
				this.UseNonNativeSlot(1);
			}
			else if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_3))
			{
				this.UseNonNativeSlot(2);
			}
			else if (inputActionArgs.HasActionFlag(PlayerAction.PLAYER_USE_ABILITY_4))
			{
				this.UseNonNativeSlot(3);
			}
			if (this.targetActiveAbilitySlots.Count != 0)
			{
				this.abilityUsingCursorPosition = inputActionArgs.worldCursorPosition;
				Transform clickedObjectTransform = inputActionArgs.clickedObjectTransform;
				Vector2 movementInput = this.playerInputController.GetMovementInput();
				movementInput.Normalize();
				for (int i = 0; i < this.targetActiveAbilitySlots.Count; i++)
				{
					int actualAbilitySlot = this.GetActualAbilitySlot(this.targetActiveAbilitySlots[i]);
					BaseAbility ability = this.GetAbility(actualAbilitySlot);
					if (!(ability == null) && (!ability.IsBusy() || ability.IsContinuous))
					{
						BaseAbility.UsingArgs abilityUsingArgs = this.GetAbilityUsingArgs(actualAbilitySlot);
						abilityUsingArgs.targetObject = clickedObjectTransform;
						abilityUsingArgs.targetPosition = this.abilityUsingCursorPosition;
						abilityUsingArgs.inputDirection = movementInput;
						if (!this.forcedUseAbilitySlots.Contains(actualAbilitySlot))
						{
							PlayerAbilityParameters playerAbilityParams = ability.GetPlayerAbilityParams();
							if (playerAbilityParams != null && playerAbilityParams.isForcedUsingAbility)
							{
								this.forcedUseAbilitySlots.Add(actualAbilitySlot);
							}
						}
					}
				}
				this.TryUseInputActionForNativeAbilities(inputActionArgs);
			}
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x0001E0E4 File Offset: 0x0001C2E4
		protected override void UpdateAbilities(float deltaTime)
		{
			if (!this.isSuspended)
			{
				this.ActivateExternalAbilities();
				for (int i = this.forcedUseAbilitySlots.Count - 1; i >= 0; i--)
				{
					int num = this.forcedUseAbilitySlots[i];
					BaseAbility baseAbility = this.abilitySlots[num];
					if (!(baseAbility == null))
					{
						baseAbility.Activate(this.GetAbilityUsingArgs(num));
						if (baseAbility.PrepProgress != 0f)
						{
							this.hasActiveForcedUseAbilities = true;
						}
					}
				}
				if (this.activeAbilitySlot >= 0 && this.activeAbilitySlot < this.abilitySlots.Length)
				{
					BaseAbility baseAbility2 = this.abilitySlots[this.activeAbilitySlot];
					if (baseAbility2 != null)
					{
						baseAbility2.Activate(this.GetAbilityUsingArgs(this.activeAbilitySlot));
					}
				}
			}
			for (int j = 0; j < this.abilities.Count; j++)
			{
				BaseAbility baseAbility3 = this.abilities[j];
				baseAbility3.OwnerPosition = this.currentPlayerPosition;
				baseAbility3.UpdateAbility(deltaTime);
			}
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x0001E1DC File Offset: 0x0001C3DC
		public void OnLateUpdate()
		{
			this.currentPlayerPosition = this.currentPlayer.HitColliderCenter;
			if (!this.isSuspended)
			{
				if (!this.hasActiveForcedUseAbilities && this.targetActiveAbilitySlots.Count != 0)
				{
					float num = Time.time - this.lastAbilityActivationTime;
					for (int i = 0; i < this.targetActiveAbilitySlots.Count; i++)
					{
						int num2 = this.targetActiveAbilitySlots[i];
						int actualAbilitySlot = this.GetActualAbilitySlot(num2);
						BaseAbility ability = this.GetAbility(actualAbilitySlot);
						if (!(ability == null))
						{
							BaseAbility.UsingArgs abilityUsingArgs = this.GetAbilityUsingArgs(actualAbilitySlot);
							BaseAbility.ActivationError activationError = ability.GetActivationError(abilityUsingArgs);
							if (actualAbilitySlot >= 0)
							{
								Action<IPlayerAbilitiesController, int, BaseAbility.UsingArgs, BaseAbility.ActivationErrorType> activeAbilitySlotSelected = this.ActiveAbilitySlotSelected;
								if (activeAbilitySlotSelected != null)
								{
									activeAbilitySlotSelected(this, actualAbilitySlot, abilityUsingArgs, activationError.type);
								}
							}
							if (this.isSuspended)
							{
								return;
							}
							if (activationError.type != BaseAbility.ActivationErrorType.None)
							{
								this.OnAbilityActivationFailed(ability, activationError);
							}
							else
							{
								bool flag = this.IsAbilityWithoutUsingTimeout(ability.ID);
								if (flag || num > this.abilityActivationTimeout)
								{
									if (!flag)
									{
										this.lastAbilityActivationTime = Time.time;
									}
									this.SetActiveAbility(num2);
									break;
								}
							}
						}
					}
				}
				else
				{
					this.ResetActiveAbility();
				}
				this.targetActiveAbilitySlots.Clear();
			}
			this.hasActiveForcedUseAbilities = false;
			this.isSuspended = false;
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x0001E31C File Offset: 0x0001C51C
		private void OnPlayerDestroyed(object playerBehaviour)
		{
			this.ResetActiveAbility();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.Destroyed -= this.OnPlayerDestroyed;
				if (this.playerInputController != null)
				{
					this.playerInputController.PlayerActionPerformed -= this.OnPlayerActionPerformed;
				}
			}
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x0001E374 File Offset: 0x0001C574
		[CompilerGenerated]
		internal static bool <SetActiveAbility>g__TryInterruptAbility|78_0(BaseAbility ability)
		{
			IInterruptableAction interruptableAction = ability as IInterruptableAction;
			return interruptableAction == null || interruptableAction.TryInterrupt(false);
		}

		// Token: 0x040004E7 RID: 1255
		public bool useActiveAbilitySlot;

		// Token: 0x040004E8 RID: 1256
		public float abilityActivationTimeout;

		// Token: 0x040004E9 RID: 1257
		public AbilityID[] abilitiesWithoutTimeout;

		// Token: 0x040004EA RID: 1258
		public PlayerAbilitiesController.AbilitySlotFallback[] nativeAbilitySlotsFallback;

		// Token: 0x040004EB RID: 1259
		private readonly IAbilityActivatedContainersFactory hpContainersAbilityFactory;

		// Token: 0x040004EC RID: 1260
		private readonly PlayerBehaviour currentPlayer;

		// Token: 0x040004ED RID: 1261
		private readonly PlayerInputController playerInputController;

		// Token: 0x040004EE RID: 1262
		private readonly List<int> targetActiveAbilitySlots = new List<int>(8);

		// Token: 0x040004EF RID: 1263
		private readonly HashSet<int> exclusiveUsingAbilities = new HashSet<int>();

		// Token: 0x040004F0 RID: 1264
		private readonly List<int> mainBattleAbilitiesSlots = new List<int>(2);

		// Token: 0x040004F1 RID: 1265
		private readonly PlayerActionsMask usedNativeAbilityActions = new PlayerActionsMask();

		// Token: 0x040004F2 RID: 1266
		private readonly List<BaseAbility> externalAbilities = new List<BaseAbility>(8);

		// Token: 0x040004F3 RID: 1267
		private readonly BaseAbility.UsingArgs[] externalAbilitiesArgs = new BaseAbility.UsingArgs[8];

		// Token: 0x040004F4 RID: 1268
		private readonly List<int> forcedUseAbilitySlots = new List<int>(4);

		// Token: 0x040004F5 RID: 1269
		private BaseAbility[] abilitySlots;

		// Token: 0x040004F6 RID: 1270
		private BaseAbility.UsingArgs[] abilitiesUsingArgs;

		// Token: 0x040004F7 RID: 1271
		private int[] abilitySlotsTypes;

		// Token: 0x040004F8 RID: 1272
		private int nonNativeAbilitiesStartSlotIndex;

		// Token: 0x040004F9 RID: 1273
		private Vector2 abilityUsingCursorPosition;

		// Token: 0x040004FA RID: 1274
		private int activeAbilitySlot;

		// Token: 0x040004FB RID: 1275
		private Vector2 currentPlayerPosition;

		// Token: 0x040004FC RID: 1276
		private float lastAbilityActivationTime;

		// Token: 0x040004FD RID: 1277
		private HashSet<int> abilitiesWithoutTimeoutSet;

		// Token: 0x040004FE RID: 1278
		private bool hasActiveForcedUseAbilities;

		// Token: 0x040004FF RID: 1279
		private bool isSuspended;

		// Token: 0x02000457 RID: 1111
		[Serializable]
		public struct AbilitySlotFallback
		{
			// Token: 0x04001700 RID: 5888
			public int sourceSlotIndex;

			// Token: 0x04001701 RID: 5889
			public int fallbackSlotIndex;
		}
	}
}
