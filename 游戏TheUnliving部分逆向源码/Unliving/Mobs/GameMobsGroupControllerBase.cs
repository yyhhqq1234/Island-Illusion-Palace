using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Mobs
{
	// Token: 0x020001D8 RID: 472
	public abstract class GameMobsGroupControllerBase
	{
		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06000F28 RID: 3880 RVA: 0x0003017B File Offset: 0x0002E37B
		public int GroupID
		{
			get
			{
				return this.groupID;
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000F29 RID: 3881 RVA: 0x00030183 File Offset: 0x0002E383
		public GameObject GroupHolder
		{
			get
			{
				return this.groupHolder;
			}
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000F2A RID: 3882 RVA: 0x0003018B File Offset: 0x0002E38B
		public IReadOnlyList<BaseGameMob> Mobs
		{
			get
			{
				return this.mobs;
			}
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000F2B RID: 3883 RVA: 0x00030193 File Offset: 0x0002E393
		public IReadOnlyList<GameMobsGroupControllerBase> CoupledGroups
		{
			get
			{
				return this.coupledGroups;
			}
		}

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000F2C RID: 3884 RVA: 0x0003019B File Offset: 0x0002E39B
		public bool HasMobs
		{
			get
			{
				return this.mobs.Count != 0;
			}
		}

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000F2D RID: 3885 RVA: 0x000301AB File Offset: 0x0002E3AB
		public Vector2 InitialPosition
		{
			get
			{
				return this.initialPosition;
			}
		}

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000F2E RID: 3886 RVA: 0x000301B3 File Offset: 0x0002E3B3
		// (set) Token: 0x06000F2F RID: 3887 RVA: 0x000301BB File Offset: 0x0002E3BB
		public IGameMobsSpawner GroupMobsSpawner { get; set; }

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000F30 RID: 3888
		// (set) Token: 0x06000F31 RID: 3889
		public abstract IGameMob Leader { get; set; }

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06000F32 RID: 3890
		public abstract Vector2 Position { get; }

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000F33 RID: 3891
		// (set) Token: 0x06000F34 RID: 3892
		public abstract Vector2? GroupDestination { get; set; }

		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000F35 RID: 3893
		public abstract Vector2 GroupDestinationDirection { get; }

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000F36 RID: 3894
		public abstract bool HasForcedGroupDestination { get; }

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000F37 RID: 3895
		public abstract bool IsGroupDestinationReached { get; }

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000F38 RID: 3896
		public abstract bool IsFollowingGroupLeader { get; }

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06000F39 RID: 3897
		public abstract bool IsVisibleGroup { get; }

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06000F3A RID: 3898
		public abstract bool HasAttackTargets { get; }

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06000F3B RID: 3899
		public abstract bool InBattle { get; }

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06000F3C RID: 3900
		public abstract bool IsRetreating { get; }

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06000F3D RID: 3901 RVA: 0x000301C4 File Offset: 0x0002E3C4
		// (set) Token: 0x06000F3E RID: 3902 RVA: 0x000301CC File Offset: 0x0002E3CC
		public GameMobFactions Faction
		{
			get
			{
				return this.faction;
			}
			set
			{
				this.faction = value;
			}
		}

		// Token: 0x140000A1 RID: 161
		// (add) Token: 0x06000F3F RID: 3903 RVA: 0x000301D8 File Offset: 0x0002E3D8
		// (remove) Token: 0x06000F40 RID: 3904 RVA: 0x00030210 File Offset: 0x0002E410
		public event Action<GameMobsGroupControllerBase, BaseGameMob> MobAdded;

		// Token: 0x140000A2 RID: 162
		// (add) Token: 0x06000F41 RID: 3905 RVA: 0x00030248 File Offset: 0x0002E448
		// (remove) Token: 0x06000F42 RID: 3906 RVA: 0x00030280 File Offset: 0x0002E480
		public event Action<GameMobsGroupControllerBase, BaseGameMob> MobRemoved;

		// Token: 0x06000F43 RID: 3907 RVA: 0x000302B5 File Offset: 0x0002E4B5
		protected GameMobsGroupControllerBase(int mobsCapacity, int coupledGroupsCapacity)
		{
			this.mobs = new List<BaseGameMob>(mobsCapacity);
			this.coupledGroups = new List<GameMobsGroupControllerBase>(coupledGroupsCapacity);
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x000302D5 File Offset: 0x0002E4D5
		protected virtual void OnMobAdded(BaseGameMob mob)
		{
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x000302D7 File Offset: 0x0002E4D7
		protected virtual void OnMobRemoved(BaseGameMob mob)
		{
		}

		// Token: 0x06000F46 RID: 3910
		public abstract void OnUpdate();

		// Token: 0x06000F47 RID: 3911 RVA: 0x000302D9 File Offset: 0x0002E4D9
		public virtual void Initialize(int groupID, GameObject groupHolder, Vector2 initialGroupPosition)
		{
			this.groupID = groupID;
			this.groupHolder = groupHolder;
			this.initialPosition = initialGroupPosition;
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x000302F0 File Offset: 0x0002E4F0
		public bool TryGetGroupComponent(out IGameMobGroupControllerProvider groupComponent)
		{
			if (this.groupHolder != null && this.groupHolder.TryGetComponent<IGameMobGroupControllerProvider>(out groupComponent))
			{
				return true;
			}
			groupComponent = null;
			return false;
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x00030314 File Offset: 0x0002E514
		public void AddCoupledGroup(GameMobsGroupControllerBase group)
		{
			if (group != null && group != this)
			{
				this.coupledGroups.Add(group);
			}
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x0003032C File Offset: 0x0002E52C
		public bool IsCoupledGroup(int groupID)
		{
			if (groupID != this.groupID)
			{
				for (int i = 0; i < this.coupledGroups.Count; i++)
				{
					if (this.coupledGroups[i].GroupID == groupID)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x0003036F File Offset: 0x0002E56F
		public bool HasCoupledGroup(GameMobsGroupControllerBase group)
		{
			return group != null && this.IsCoupledGroup(group.GroupID);
		}

		// Token: 0x06000F4C RID: 3916 RVA: 0x00030382 File Offset: 0x0002E582
		public void RemoveCoupledGroup(GameMobsGroupControllerBase group)
		{
			this.coupledGroups.Remove(group);
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x00030394 File Offset: 0x0002E594
		public bool IsAnyGroup(Predicate<GameMobsGroupControllerBase> match)
		{
			if (match(this))
			{
				return true;
			}
			for (int i = 0; i < this.coupledGroups.Count; i++)
			{
				if (match(this.coupledGroups[i]))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x000303DC File Offset: 0x0002E5DC
		public bool AddMob(BaseGameMob mob, Action<BaseGameMob> additionCallback = null)
		{
			if (mob == null || mob.Group == this || !mob.IsAlive())
			{
				return false;
			}
			GameMobsGroupControllerBase group = mob.Group;
			mob.Group = this;
			if (group != null)
			{
				group.RemoveMob(mob);
			}
			this.mobs.Add(mob);
			if (additionCallback != null)
			{
				additionCallback(mob);
			}
			this.OnMobAdded(mob);
			Action<GameMobsGroupControllerBase, BaseGameMob> mobAdded = this.MobAdded;
			if (mobAdded != null)
			{
				mobAdded(this, mob);
			}
			return true;
		}

		// Token: 0x06000F4F RID: 3919 RVA: 0x00030450 File Offset: 0x0002E650
		public BaseGameMob AddMob(int mobID, Action<BaseGameMob> additionCallback = null)
		{
			if (this.GroupMobsSpawner != null)
			{
				BaseGameMob baseGameMob = this.GroupMobsSpawner.SpawnMob(mobID);
				if (baseGameMob != null)
				{
					this.AddMob(baseGameMob, additionCallback);
					return baseGameMob;
				}
			}
			return null;
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x00030488 File Offset: 0x0002E688
		public bool RemoveMob(BaseGameMob mob)
		{
			if (mob != null)
			{
				int num = this.mobs.IndexOf(mob);
				if (num >= 0)
				{
					if (mob.Group == this)
					{
						mob.Group = null;
					}
					this.mobs.RemoveBySwap(num);
					this.OnMobRemoved(mob);
					Action<GameMobsGroupControllerBase, BaseGameMob> mobRemoved = this.MobRemoved;
					if (mobRemoved != null)
					{
						mobRemoved(this, mob);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000F51 RID: 3921 RVA: 0x000304E8 File Offset: 0x0002E6E8
		public virtual void DrawGizmos()
		{
		}

		// Token: 0x040008F1 RID: 2289
		[SerializeField]
		[FormerlySerializedAs("_faction")]
		protected GameMobFactions faction;

		// Token: 0x040008F2 RID: 2290
		protected readonly List<BaseGameMob> mobs;

		// Token: 0x040008F3 RID: 2291
		protected readonly List<GameMobsGroupControllerBase> coupledGroups;

		// Token: 0x040008F4 RID: 2292
		protected Vector2 initialPosition;

		// Token: 0x040008F5 RID: 2293
		private int groupID;

		// Token: 0x040008F6 RID: 2294
		private GameObject groupHolder;
	}
}
