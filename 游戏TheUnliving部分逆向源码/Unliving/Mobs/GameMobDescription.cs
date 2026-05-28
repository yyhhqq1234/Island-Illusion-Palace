using System;
using Common.Editor;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Factories;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs
{
	// Token: 0x020001D2 RID: 466
	[Serializable]
	public struct GameMobDescription : IEquatable<GameMobDescription>
	{
		// Token: 0x06000EDE RID: 3806 RVA: 0x0002F2CD File Offset: 0x0002D4CD
		public static bool operator ==(GameMobDescription left, GameMobDescription right)
		{
			return left.Equals(ref right);
		}

		// Token: 0x06000EDF RID: 3807 RVA: 0x0002F2D8 File Offset: 0x0002D4D8
		public static bool operator !=(GameMobDescription left, GameMobDescription right)
		{
			return !left.Equals(ref right);
		}

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06000EE0 RID: 3808 RVA: 0x0002F2E6 File Offset: 0x0002D4E6
		public static GameMobDescription BlankDescription
		{
			get
			{
				return GameMobDescription.blankDescription;
			}
		}

		// Token: 0x06000EE1 RID: 3809 RVA: 0x0002F2ED File Offset: 0x0002D4ED
		private static bool IsUndefinedTag(string tag)
		{
			return string.IsNullOrEmpty(tag) || tag.Equals("untagged", StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000EE2 RID: 3810 RVA: 0x0002F305 File Offset: 0x0002D505
		private bool FilterByMobID()
		{
			return this.mobID > MobBehaviour.ID.None;
		}

		// Token: 0x06000EE3 RID: 3811 RVA: 0x0002F310 File Offset: 0x0002D510
		private bool FilterByUndeadMobID()
		{
			return this.undeadMobVersionID > MobBehaviour.ID.None;
		}

		// Token: 0x06000EE4 RID: 3812 RVA: 0x0002F31C File Offset: 0x0002D51C
		public bool IsMatch(IGameMob mob)
		{
			if (mob == null)
			{
				return false;
			}
			if (!this.isAny)
			{
				if (this.mobFaction != GameMobFactions.None && mob.Faction != this.mobFaction)
				{
					return false;
				}
				if (this.allowedMobLayers != 0 && !mob.IsLayerInMask(this.allowedMobLayers))
				{
					return false;
				}
				GameObject gameObject = mob.GameObject;
				if (!GameMobDescription.IsUndefinedTag(this.mobTag) && (gameObject == null || !gameObject.CompareTag(this.mobTag)))
				{
					return false;
				}
				if (this.isPlayerMob && !mob.IsPlayerMob)
				{
					return false;
				}
				if (this.isSummoned && !mob.IsSummoned)
				{
					return false;
				}
				if (this.isSacrificed && !mob.IsSacrificed)
				{
					return false;
				}
				if (this.isKilled && !mob.IsKilled)
				{
					return false;
				}
				BaseAbility baseAbility;
				MobActivationAbilityType mobActivationAbilityType;
				if (this.mobActivationAbilityTypes != MobActivationAbilityType.None && mob.TryGetMobActivationAbility(out baseAbility, out mobActivationAbilityType) && (this.mobActivationAbilityTypes & mobActivationAbilityType) == MobActivationAbilityType.None)
				{
					return false;
				}
				MobBehaviour mobBehaviour = mob as MobBehaviour;
				if (mobBehaviour != null)
				{
					if (this.FilterByMobID() && mobBehaviour.ObjectID != this.mobID)
					{
						return false;
					}
					if (this.FilterByUndeadMobID() && mobBehaviour.ZombieMobID != this.undeadMobVersionID)
					{
						return false;
					}
					if (this.isNonCharacter && mobBehaviour.IsCharacter)
					{
						return false;
					}
					if (this.isBoss && !mobBehaviour.IsBoss)
					{
						return false;
					}
				}
				else if (this.isBoss || this.FilterByMobID() || this.FilterByUndeadMobID())
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000EE5 RID: 3813 RVA: 0x0002F482 File Offset: 0x0002D682
		public bool IsMobComponentMatch(Component mobBehaviour)
		{
			return this.IsMatch(mobBehaviour.CastOrGetComponent<IGameMob>());
		}

		// Token: 0x06000EE6 RID: 3814 RVA: 0x0002F490 File Offset: 0x0002D690
		public bool IsBlank()
		{
			return this.Equals(ref GameMobDescription.blankDescription);
		}

		// Token: 0x06000EE7 RID: 3815 RVA: 0x0002F4A0 File Offset: 0x0002D6A0
		public bool Equals(ref GameMobDescription description)
		{
			return this.isAny == description.isAny && this.mobID == description.mobID && this.undeadMobVersionID == description.undeadMobVersionID && this.mobFaction == description.mobFaction && this.mobActivationAbilityTypes == description.mobActivationAbilityTypes && this.allowedMobLayers.value == description.allowedMobLayers.value && this.mobTag == description.mobTag && this.isPlayerMob == description.isPlayerMob && this.isBoss == description.isBoss && this.isSummoned == description.isSummoned && this.isSacrificed == description.isSacrificed && this.isKilled == description.isKilled;
		}

		// Token: 0x06000EE8 RID: 3816 RVA: 0x0002F572 File Offset: 0x0002D772
		bool IEquatable<GameMobDescription>.Equals(GameMobDescription other)
		{
			return this.Equals(ref other);
		}

		// Token: 0x06000EE9 RID: 3817 RVA: 0x0002F57C File Offset: 0x0002D77C
		public override bool Equals(object obj)
		{
			if (obj is GameMobDescription)
			{
				GameMobDescription gameMobDescription = (GameMobDescription)obj;
				return this.Equals(ref gameMobDescription);
			}
			return false;
		}

		// Token: 0x06000EEA RID: 3818 RVA: 0x0002F5A4 File Offset: 0x0002D7A4
		public override int GetHashCode()
		{
			int num = -1637224382;
			if (!GameMobDescription.IsUndefinedTag(this.mobTag))
			{
				num = num * -1521134295 + this.mobTag.GetHashCode();
			}
			num = num * -1521134295 + this.isAny.GetHashCode();
			num = num * -1521134295 + this.mobID.GetHashCode();
			num = num * -1521134295 + this.undeadMobVersionID.GetHashCode();
			num = num * -1521134295 + this.mobFaction.GetHashCode();
			num = num * -1521134295 + this.allowedMobLayers.GetHashCode();
			num = num * -1521134295 + this.mobActivationAbilityTypes.GetHashCode();
			num = num * -1521134295 + this.isPlayerMob.GetHashCode();
			num = num * -1521134295 + this.isBoss.GetHashCode();
			num = num * -1521134295 + this.isSummoned.GetHashCode();
			num = num * -1521134295 + this.isSacrificed.GetHashCode();
			return num * -1521134295 + this.isKilled.GetHashCode();
		}

		// Token: 0x06000EEB RID: 3819 RVA: 0x0002F6D3 File Offset: 0x0002D8D3
		public void PassTo(ref GameMobDescription targetDescription)
		{
			targetDescription = this;
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x0002F6E1 File Offset: 0x0002D8E1
		public bool TryPassTo(ref GameMobDescription targetDescription)
		{
			if (!this.IsBlank())
			{
				targetDescription = this;
				return true;
			}
			return false;
		}

		// Token: 0x06000EED RID: 3821 RVA: 0x0002F6FA File Offset: 0x0002D8FA
		public bool TryPassAsTargetsFilter(ref Predicate<Component> targetsFilter)
		{
			if (!this.IsBlank())
			{
				targetsFilter = new Predicate<Component>(this.IsMobComponentMatch);
				return true;
			}
			return false;
		}

		// Token: 0x06000EEE RID: 3822 RVA: 0x0002F71F File Offset: 0x0002D91F
		public bool TryPassAsTargetsFilter(ref Predicate<BaseGameMob> targetsFilter)
		{
			if (!this.IsBlank())
			{
				targetsFilter = new Predicate<BaseGameMob>(this.IsMatch);
				return true;
			}
			return false;
		}

		// Token: 0x040008C5 RID: 2245
		public static readonly GameMobDescription AnyMob = new GameMobDescription
		{
			isAny = true
		};

		// Token: 0x040008C6 RID: 2246
		private static GameMobDescription blankDescription = new GameMobDescription
		{
			mobID = MobBehaviour.ID.None,
			undeadMobVersionID = MobBehaviour.ID.None,
			mobFaction = GameMobFactions.None,
			mobActivationAbilityTypes = MobActivationAbilityType.None,
			allowedMobLayers = 0
		};

		// Token: 0x040008C7 RID: 2247
		public bool isAny;

		// Token: 0x040008C8 RID: 2248
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID mobID;

		// Token: 0x040008C9 RID: 2249
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID undeadMobVersionID;

		// Token: 0x040008CA RID: 2250
		public GameMobFactions mobFaction;

		// Token: 0x040008CB RID: 2251
		public MobActivationAbilityType mobActivationAbilityTypes;

		// Token: 0x040008CC RID: 2252
		public LayerMask allowedMobLayers;

		// Token: 0x040008CD RID: 2253
		[Tag]
		public string mobTag;

		// Token: 0x040008CE RID: 2254
		public bool isNonCharacter;

		// Token: 0x040008CF RID: 2255
		public bool isPlayerMob;

		// Token: 0x040008D0 RID: 2256
		public bool isBoss;

		// Token: 0x040008D1 RID: 2257
		public bool isSummoned;

		// Token: 0x040008D2 RID: 2258
		public bool isSacrificed;

		// Token: 0x040008D3 RID: 2259
		public bool isKilled;
	}
}
