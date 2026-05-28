using System;
using Common.Factories;
using Game.Abilities;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.Factories;
using Unliving.MobsStats;

namespace Unliving.Mobs.ActivationModifiers
{
	// Token: 0x02000222 RID: 546
	public sealed class MobsActivationModifiersController : BaseGameMob.ControllerBase<BaseGameMob>
	{
		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x060012A8 RID: 4776 RVA: 0x0003B334 File Offset: 0x00039534
		public MobsActivationModifiersController.Slot[] Slots
		{
			get
			{
				return this.slots;
			}
		}

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x060012A9 RID: 4777 RVA: 0x0003B33C File Offset: 0x0003953C
		public StatsController ModifiersStats
		{
			get
			{
				return this.modifiersStats;
			}
		}

		// Token: 0x140000C2 RID: 194
		// (add) Token: 0x060012AA RID: 4778 RVA: 0x0003B344 File Offset: 0x00039544
		// (remove) Token: 0x060012AB RID: 4779 RVA: 0x0003B37C File Offset: 0x0003957C
		public event Action<MobsActivationModifiersController, int> ModifierAdded;

		// Token: 0x140000C3 RID: 195
		// (add) Token: 0x060012AC RID: 4780 RVA: 0x0003B3B4 File Offset: 0x000395B4
		// (remove) Token: 0x060012AD RID: 4781 RVA: 0x0003B3EC File Offset: 0x000395EC
		public event Action<MobsActivationModifiersController, int, MobActivationAbilityModifier> ModifierRemoved;

		// Token: 0x140000C4 RID: 196
		// (add) Token: 0x060012AE RID: 4782 RVA: 0x0003B424 File Offset: 0x00039624
		// (remove) Token: 0x060012AF RID: 4783 RVA: 0x0003B45C File Offset: 0x0003965C
		public event Action<BaseGameMob, MobActivationAbilityType> MobRegistered;

		// Token: 0x140000C5 RID: 197
		// (add) Token: 0x060012B0 RID: 4784 RVA: 0x0003B494 File Offset: 0x00039694
		// (remove) Token: 0x060012B1 RID: 4785 RVA: 0x0003B4CC File Offset: 0x000396CC
		public event Action<BaseGameMob, MobActivationAbilityType> MobUnregistered;

		// Token: 0x060012B2 RID: 4786 RVA: 0x0003B504 File Offset: 0x00039704
		public MobsActivationModifiersController(BaseGameMob targetMob, MobsActivationModifiersController.Slot[] slotsInfo, IObjectFactory<MobActivationAbilityModifier> modifiersFactory) : base(targetMob)
		{
			this.slots = new MobsActivationModifiersController.Slot[slotsInfo.Length];
			for (int i = 0; i < this.slots.Length; i++)
			{
				this.slots[i] = new MobsActivationModifiersController.Slot(slotsInfo[i].AllowedModifierType);
			}
			if (targetMob.Group != null)
			{
				targetMob.Group.MobAdded += this.OnGroupMobAdded;
				targetMob.Group.MobRemoved += this.OnGroupMobRemoved;
			}
			this.modifiersFactory = modifiersFactory;
			this.modifiersStats = new StatsController(this, false);
			this.modifiersStats.AddStat(new MobProxyStat(MobStatID.GroupMobsActivationModifiersDamage, this));
			this.modifiersStats.AddStat(new MobProxyStat(MobStatID.GroupMobsActivationModifiersBuffDuration, this));
		}

		// Token: 0x060012B3 RID: 4787 RVA: 0x0003B5C8 File Offset: 0x000397C8
		private MobActivationAbilityModifier GetModifierPrototype(MobActivationModifierID modifierID)
		{
			if (this.modifiersFactory == null)
			{
				return null;
			}
			MobsActivationModifiersFactory.PrototypeInfo objectPrototype = ((PrototypeBasedFactory<MobsActivationModifiersFactory.PrototypeInfo, MobActivationAbilityModifier>)this.modifiersFactory).GetObjectPrototype((int)modifierID);
			if (objectPrototype == null)
			{
				return null;
			}
			return objectPrototype.modifierPrototype;
		}

		// Token: 0x060012B4 RID: 4788 RVA: 0x0003B5F0 File Offset: 0x000397F0
		private bool AddModifier(int slotIndex, MobActivationAbilityModifier modifier, int modifierLevel, bool checkBounds)
		{
			if ((!checkBounds || (slotIndex >= 0 && slotIndex < this.slots.Length)) && this.slots[slotIndex].AddModifier(modifier, modifierLevel))
			{
				Action<MobsActivationModifiersController, int> modifierAdded = this.ModifierAdded;
				if (modifierAdded != null)
				{
					modifierAdded(this, slotIndex);
				}
				return true;
			}
			return false;
		}

		// Token: 0x060012B5 RID: 4789 RVA: 0x0003B630 File Offset: 0x00039830
		private MobActivationAbilityModifier RemoveModifier(int slotIndex, bool checkBounds, out int modifierLevel)
		{
			if (!checkBounds || (slotIndex >= 0 && slotIndex < this.slots.Length))
			{
				ref MobsActivationModifiersController.Slot ptr = ref this.slots[slotIndex];
				if (!ptr.IsFree())
				{
					MobActivationAbilityModifier currentModifier = ptr.CurrentModifier;
					modifierLevel = ptr.CurrentModifierLevel;
					ptr.Clear();
					Action<MobsActivationModifiersController, int, MobActivationAbilityModifier> modifierRemoved = this.ModifierRemoved;
					if (modifierRemoved != null)
					{
						modifierRemoved(this, slotIndex, currentModifier);
					}
					return currentModifier;
				}
			}
			modifierLevel = 0;
			return null;
		}

		// Token: 0x060012B6 RID: 4790 RVA: 0x0003B694 File Offset: 0x00039894
		private void AddModifierToMobAbility(BaseAbility mobAbility, MobsActivationModifiersController.Slot modifierSlot)
		{
			MobsActivationModifiersController.ModifiersFactoryArgs.modifierID = modifierSlot.CurrentModifierID;
			MobsActivationModifiersController.ModifiersFactoryArgs.modifierPrototype = modifierSlot.CurrentModifier;
			MobsActivationModifiersController.ModifiersFactoryArgs.modifierLevel = modifierSlot.CurrentModifierLevel;
			MobsActivationModifiersController.ModifiersFactoryArgs.targetAbility = mobAbility;
			MobsActivationModifiersController.ModifiersFactoryArgs.objectType = MultiRepresentationObjectInstantiator.ObjectType.Default;
			MobActivationAbilityModifier mobActivationAbilityModifier = this.modifiersFactory.Create(MobsActivationModifiersController.ModifiersFactoryArgs);
			if (mobActivationAbilityModifier != null)
			{
				mobActivationAbilityModifier.Stats = this.modifiersStats;
			}
		}

		// Token: 0x060012B7 RID: 4791 RVA: 0x0003B710 File Offset: 0x00039910
		public bool HasModifier(MobActivationModifierID modifierID)
		{
			return Array.Exists<MobsActivationModifiersController.Slot>(this.slots, (MobsActivationModifiersController.Slot slot) => slot.CurrentModifierID == modifierID);
		}

		// Token: 0x060012B8 RID: 4792 RVA: 0x0003B744 File Offset: 0x00039944
		public bool HasModifierOfType(MobActivationAbilityType abilityType)
		{
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.slots[i].CanBeUsedForActivation(abilityType))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060012B9 RID: 4793 RVA: 0x0003B77C File Offset: 0x0003997C
		public bool TryGetSlotWithModifier(MobActivationModifierID modifierID, out MobsActivationModifiersController.Slot slot)
		{
			slot = default(MobsActivationModifiersController.Slot);
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.slots[i].CurrentModifierID == modifierID)
				{
					slot = this.slots[i];
					return true;
				}
			}
			return false;
		}

		// Token: 0x060012BA RID: 4794 RVA: 0x0003B7CC File Offset: 0x000399CC
		public int GetCompatibleSlot(MobActivationAbilityType activationTypes)
		{
			for (int i = 0; i < this.slots.Length; i++)
			{
				if ((this.slots[i].AllowedModifierType & activationTypes) != MobActivationAbilityType.None)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060012BB RID: 4795 RVA: 0x0003B804 File Offset: 0x00039A04
		public MobsActivationModifiersController.Slot GetLowerLevelCompatibleSlot(MobActivationAbilityType activationTypes)
		{
			MobsActivationModifiersController.Slot result = default(MobsActivationModifiersController.Slot);
			for (int i = 0; i < this.slots.Length; i++)
			{
				ref MobsActivationModifiersController.Slot ptr = ref this.slots[i];
				if ((ptr.AllowedModifierType & activationTypes) != MobActivationAbilityType.None && (result.CurrentModifierLevel == 0 || ptr.CurrentModifierLevel < result.CurrentModifierLevel))
				{
					result = ptr;
				}
			}
			return result;
		}

		// Token: 0x060012BC RID: 4796 RVA: 0x0003B864 File Offset: 0x00039A64
		public int GetFreeModifierSlotIndex(MobActivationAbilityType activationTypes, MobActivationModifierID modifierID = MobActivationModifierID.None)
		{
			if (modifierID == MobActivationModifierID.None || !this.HasModifier(modifierID))
			{
				for (int i = 0; i < this.slots.Length; i++)
				{
					if (this.slots[i].IsFree(activationTypes))
					{
						return i;
					}
				}
			}
			return -1;
		}

		// Token: 0x060012BD RID: 4797 RVA: 0x0003B8A8 File Offset: 0x00039AA8
		public bool HasFreeModifierSlot(MobActivationAbilityType activationTypes, MobActivationModifierID modifierID = MobActivationModifierID.None)
		{
			return this.GetFreeModifierSlotIndex(activationTypes, modifierID) != -1;
		}

		// Token: 0x060012BE RID: 4798 RVA: 0x0003B8B8 File Offset: 0x00039AB8
		public bool TryGetSlotOfType(MobActivationAbilityType activationAbilityType, out MobsActivationModifiersController.Slot slot)
		{
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.slots[i].AllowedModifierType == activationAbilityType)
				{
					slot = this.slots[i];
					return true;
				}
			}
			slot = default(MobsActivationModifiersController.Slot);
			return false;
		}

		// Token: 0x060012BF RID: 4799 RVA: 0x0003B908 File Offset: 0x00039B08
		public bool AddModifier(int slotIndex, MobActivationAbilityModifier modifier, int modifierLevel)
		{
			return this.AddModifier(slotIndex, modifier, modifierLevel, true);
		}

		// Token: 0x060012C0 RID: 4800 RVA: 0x0003B914 File Offset: 0x00039B14
		public bool AddModifier(int slotIndex, MobActivationModifierID modifierID, int modifierLevel)
		{
			return this.AddModifier(slotIndex, this.GetModifierPrototype(modifierID), modifierLevel);
		}

		// Token: 0x060012C1 RID: 4801 RVA: 0x0003B925 File Offset: 0x00039B25
		public bool AddModifier(MobActivationModifierID modifierID, int modifierLevel)
		{
			return this.AddModifier(this.GetModifierPrototype(modifierID), modifierLevel);
		}

		// Token: 0x060012C2 RID: 4802 RVA: 0x0003B938 File Offset: 0x00039B38
		public bool AddModifier(MobActivationAbilityModifier modifier, int modifierLevel)
		{
			for (int i = 0; i < this.slots.Length; i++)
			{
				if (this.AddModifier(i, modifier, modifierLevel, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060012C3 RID: 4803 RVA: 0x0003B968 File Offset: 0x00039B68
		public void RemoveModifier(int slotIndex)
		{
			int num;
			this.RemoveModifier(slotIndex, true, out num);
		}

		// Token: 0x060012C4 RID: 4804 RVA: 0x0003B980 File Offset: 0x00039B80
		public GameObject DropModifier(int slotIndex)
		{
			int modifierLevel;
			MobActivationAbilityModifier mobActivationAbilityModifier;
			if (this.modifiersFactory != null && (mobActivationAbilityModifier = this.RemoveModifier(slotIndex, true, out modifierLevel)) != null)
			{
				MobsActivationModifiersController.ModifiersFactoryArgs.modifierID = mobActivationAbilityModifier.ModifierID;
				MobsActivationModifiersController.ModifiersFactoryArgs.modifierPrototype = null;
				MobsActivationModifiersController.ModifiersFactoryArgs.targetAbility = null;
				MobsActivationModifiersController.ModifiersFactoryArgs.modifierLevel = modifierLevel;
				MobsActivationModifiersController.ModifiersFactoryArgs.objectType = MultiRepresentationObjectInstantiator.ObjectType.PickableObject;
				GameObject gameObject = (GameObject)this.modifiersFactory.Create(MobsActivationModifiersController.ModifiersFactoryArgs);
				Vector3 position = this.ControllerOwner.Position + UnityEngine.Random.onUnitSphere * (this.ControllerOwner.Radius + 2f);
				position.z = this.ControllerOwner.transform.position.z;
				gameObject.transform.position = position;
				return gameObject;
			}
			return null;
		}

		// Token: 0x060012C5 RID: 4805 RVA: 0x0003BA5C File Offset: 0x00039C5C
		private void OnGroupMobAdded(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			mob.Sacrificed += this.OnGroupMobSacrificed;
			BaseAbility baseAbility;
			MobActivationAbilityType arg;
			if (mob.TryGetMobActivationAbility(out baseAbility, out arg))
			{
				Action<BaseGameMob, MobActivationAbilityType> mobRegistered = this.MobRegistered;
				if (mobRegistered == null)
				{
					return;
				}
				mobRegistered(mob, arg);
			}
		}

		// Token: 0x060012C6 RID: 4806 RVA: 0x0003BA9C File Offset: 0x00039C9C
		private void OnGroupMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			mob.Sacrificed -= this.OnGroupMobSacrificed;
			BaseAbility baseAbility;
			MobActivationAbilityType arg;
			if (mob.TryGetMobActivationAbility(out baseAbility, out arg))
			{
				Action<BaseGameMob, MobActivationAbilityType> mobUnregistered = this.MobUnregistered;
				if (mobUnregistered == null)
				{
					return;
				}
				mobUnregistered(mob, arg);
			}
		}

		// Token: 0x060012C7 RID: 4807 RVA: 0x0003BADC File Offset: 0x00039CDC
		private void OnGroupMobSacrificed(BaseGameMob mob)
		{
			BaseAbility mobAbility;
			MobActivationAbilityType activationType;
			if (mob.TryGetMobActivationAbility(out mobAbility, out activationType))
			{
				for (int i = 0; i < this.slots.Length; i++)
				{
					ref MobsActivationModifiersController.Slot ptr = ref this.slots[i];
					if (ptr.CanBeUsedForActivation(activationType))
					{
						this.AddModifierToMobAbility(mobAbility, ptr);
					}
				}
			}
		}

		// Token: 0x060012C8 RID: 4808 RVA: 0x0003BB2C File Offset: 0x00039D2C
		protected override void OnOwnerKilled(IGameMob owner)
		{
			if (this.ControllerOwner.Group != null)
			{
				this.ControllerOwner.Group.MobAdded -= this.OnGroupMobAdded;
				this.ControllerOwner.Group.MobRemoved -= this.OnGroupMobRemoved;
			}
			for (int i = 0; i < this.slots.Length; i++)
			{
				this.slots[i].Clear();
			}
			base.OnOwnerKilled(owner);
		}

		// Token: 0x04000B03 RID: 2819
		private static readonly MobsActivationModifiersFactory.Args ModifiersFactoryArgs = new MobsActivationModifiersFactory.Args();

		// Token: 0x04000B08 RID: 2824
		private readonly MobsActivationModifiersController.Slot[] slots;

		// Token: 0x04000B09 RID: 2825
		private readonly IObjectFactory<MobActivationAbilityModifier> modifiersFactory;

		// Token: 0x04000B0A RID: 2826
		private readonly StatsController modifiersStats;

		// Token: 0x020004C1 RID: 1217
		[Serializable]
		public struct Slot
		{
			// Token: 0x1700078A RID: 1930
			// (get) Token: 0x06002525 RID: 9509 RVA: 0x000737E2 File Offset: 0x000719E2
			public MobActivationAbilityType AllowedModifierType
			{
				get
				{
					return this.allowedModifierType;
				}
			}

			// Token: 0x1700078B RID: 1931
			// (get) Token: 0x06002526 RID: 9510 RVA: 0x000737EA File Offset: 0x000719EA
			public MobActivationAbilityModifier CurrentModifier
			{
				get
				{
					return this.currentModifier;
				}
			}

			// Token: 0x1700078C RID: 1932
			// (get) Token: 0x06002527 RID: 9511 RVA: 0x000737F2 File Offset: 0x000719F2
			public MobActivationModifierID CurrentModifierID
			{
				get
				{
					if (!(this.currentModifier != null))
					{
						return MobActivationModifierID.None;
					}
					return this.currentModifier.ModifierID;
				}
			}

			// Token: 0x1700078D RID: 1933
			// (get) Token: 0x06002528 RID: 9512 RVA: 0x0007380F File Offset: 0x00071A0F
			public int CurrentModifierLevel
			{
				get
				{
					return this.currentModifierLevel;
				}
			}

			// Token: 0x06002529 RID: 9513 RVA: 0x00073817 File Offset: 0x00071A17
			public Slot(MobActivationAbilityType allowedModifierType)
			{
				this.allowedModifierType = allowedModifierType;
				this.currentModifier = null;
				this.currentModifierLevel = 0;
			}

			// Token: 0x0600252A RID: 9514 RVA: 0x0007382E File Offset: 0x00071A2E
			public bool IsFree()
			{
				return this.currentModifier == null;
			}

			// Token: 0x0600252B RID: 9515 RVA: 0x0007383C File Offset: 0x00071A3C
			public bool IsFree(MobActivationAbilityType activationTypes)
			{
				return this.IsFree() && (activationTypes & this.allowedModifierType) > MobActivationAbilityType.None;
			}

			// Token: 0x0600252C RID: 9516 RVA: 0x00073853 File Offset: 0x00071A53
			public bool CanBeUsedForActivation(MobActivationAbilityType activationType)
			{
				return this.currentModifier != null && this.allowedModifierType == activationType;
			}

			// Token: 0x0600252D RID: 9517 RVA: 0x0007386E File Offset: 0x00071A6E
			public bool AddModifier(MobActivationAbilityModifier newModifier, int modifierLevel)
			{
				if (this.currentModifier == null && newModifier != null && newModifier.CanBeUsedAsMobActivationModifier(this.allowedModifierType))
				{
					this.currentModifier = newModifier;
					this.currentModifierLevel = Mathf.Max(modifierLevel, 1);
					return true;
				}
				return false;
			}

			// Token: 0x0600252E RID: 9518 RVA: 0x000738AC File Offset: 0x00071AAC
			public void Clear()
			{
				this.currentModifier = null;
				this.currentModifierLevel = 0;
			}

			// Token: 0x040019A1 RID: 6561
			[SerializeField]
			[FormerlySerializedAs("_allowedModifierType")]
			private MobActivationAbilityType allowedModifierType;

			// Token: 0x040019A2 RID: 6562
			private MobActivationAbilityModifier currentModifier;

			// Token: 0x040019A3 RID: 6563
			private int currentModifierLevel;
		}
	}
}
