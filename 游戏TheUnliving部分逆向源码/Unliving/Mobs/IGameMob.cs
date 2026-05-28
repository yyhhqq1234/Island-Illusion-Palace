using System;
using Game.Buffs;
using Game.Damage;
using UnityEngine;
using UnityEngine.AI;
using Unliving.AbilityResources;
using Unliving.LevelGeneration;
using Unliving.Mobs.Motion;

namespace Unliving.Mobs
{
	// Token: 0x020001E7 RID: 487
	public interface IGameMob : IBuffableObject
	{
		// Token: 0x17000336 RID: 822
		// (get) Token: 0x0600101B RID: 4123
		GameObject GameObject { get; }

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x0600101C RID: 4124
		string Name { get; }

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x0600101D RID: 4125
		int LayerMask { get; }

		// Token: 0x17000339 RID: 825
		// (get) Token: 0x0600101E RID: 4126
		bool IsCharacter { get; }

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x0600101F RID: 4127
		float Radius { get; }

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x06001020 RID: 4128
		bool IsCrowdObstacle { get; }

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x06001021 RID: 4129
		// (set) Token: 0x06001022 RID: 4130
		bool IsKinematic { get; set; }

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x06001023 RID: 4131
		NavMeshAgent NavMeshAgent { get; }

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x06001024 RID: 4132
		Collider2D HitCollider { get; }

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x06001025 RID: 4133
		Vector2 HitColliderCenter { get; }

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x06001026 RID: 4134
		// (set) Token: 0x06001027 RID: 4135
		Vector2 Position { get; set; }

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x06001028 RID: 4136
		IDamageable HitPointsController { get; }

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x06001029 RID: 4137
		GameMobFactions Faction { get; }

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x0600102A RID: 4138
		// (set) Token: 0x0600102B RID: 4139
		GameMobsGroupControllerBase Group { get; set; }

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x0600102C RID: 4140
		GameMobsGroupControllerBase LastGroup { get; }

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x0600102D RID: 4141
		GameLocation CurrentLocation { get; }

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x0600102E RID: 4142
		GameMobMotionControllerBase MotionController { get; }

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x0600102F RID: 4143
		AbilityResourcesGenerator ResourcesGenerator { get; }

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x06001030 RID: 4144
		GameMobSummoningContext SummonerInfo { get; }

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x06001031 RID: 4145
		bool IsPlayerMob { get; }

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x06001032 RID: 4146
		bool IsMinorAttackTarget { get; }

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x06001033 RID: 4147
		bool IsSummoned { get; }

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x06001034 RID: 4148
		bool IsRevived { get; }

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x06001035 RID: 4149
		bool IsSacrificed { get; }

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x06001036 RID: 4150
		bool IsKilled { get; }

		// Token: 0x140000B3 RID: 179
		// (add) Token: 0x06001037 RID: 4151
		// (remove) Token: 0x06001038 RID: 4152
		event Action<IGameMob> Killed;

		// Token: 0x06001039 RID: 4153
		void SetCreationType(GameMobCreationType birthType, object context);

		// Token: 0x0600103A RID: 4154
		void SetNavMeshAgentActive(bool isActive);

		// Token: 0x0600103B RID: 4155
		bool CanBeAttackedBy(IGameMob mob);

		// Token: 0x0600103C RID: 4156
		void KillMob(object mobKiller);
	}
}
