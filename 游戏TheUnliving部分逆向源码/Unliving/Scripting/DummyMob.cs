using System;
using Game.Buffs;
using Game.Damage;
using UnityEngine;
using UnityEngine.AI;
using Unliving.AbilityResources;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Mobs.Motion;

namespace Unliving.Scripting
{
	// Token: 0x02000076 RID: 118
	public sealed class DummyMob : IGameMob, IBuffableObject
	{
		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000347 RID: 839 RVA: 0x0000BDE0 File Offset: 0x00009FE0
		public GameObject GameObject
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000348 RID: 840 RVA: 0x0000BDE3 File Offset: 0x00009FE3
		// (set) Token: 0x06000349 RID: 841 RVA: 0x0000BDEB File Offset: 0x00009FEB
		public string Name { get; set; } = "DummyMob";

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x0600034A RID: 842 RVA: 0x0000BDF4 File Offset: 0x00009FF4
		// (set) Token: 0x0600034B RID: 843 RVA: 0x0000BDFC File Offset: 0x00009FFC
		public int Layer
		{
			get
			{
				return this.layer;
			}
			set
			{
				this.LayerMask = 1 << value;
				this.layer = value;
			}
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x0600034C RID: 844 RVA: 0x0000BE11 File Offset: 0x0000A011
		// (set) Token: 0x0600034D RID: 845 RVA: 0x0000BE19 File Offset: 0x0000A019
		public int LayerMask { get; private set; }

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x0600034E RID: 846 RVA: 0x0000BE22 File Offset: 0x0000A022
		// (set) Token: 0x0600034F RID: 847 RVA: 0x0000BE2A File Offset: 0x0000A02A
		public bool IsCharacter { get; set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x06000350 RID: 848 RVA: 0x0000BE33 File Offset: 0x0000A033
		// (set) Token: 0x06000351 RID: 849 RVA: 0x0000BE3B File Offset: 0x0000A03B
		public float Radius { get; set; }

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x06000352 RID: 850 RVA: 0x0000BE44 File Offset: 0x0000A044
		public NavMeshAgent NavMeshAgent
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x06000353 RID: 851 RVA: 0x0000BE47 File Offset: 0x0000A047
		public Collider2D HitCollider
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x06000354 RID: 852 RVA: 0x0000BE4A File Offset: 0x0000A04A
		public Vector2 HitColliderCenter
		{
			get
			{
				return this.Position;
			}
		}

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000355 RID: 853 RVA: 0x0000BE52 File Offset: 0x0000A052
		// (set) Token: 0x06000356 RID: 854 RVA: 0x0000BE5A File Offset: 0x0000A05A
		public Vector2 Position { get; set; }

		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000357 RID: 855 RVA: 0x0000BE63 File Offset: 0x0000A063
		// (set) Token: 0x06000358 RID: 856 RVA: 0x0000BE6B File Offset: 0x0000A06B
		public GameMobFactions Faction { get; set; }

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000359 RID: 857 RVA: 0x0000BE74 File Offset: 0x0000A074
		// (set) Token: 0x0600035A RID: 858 RVA: 0x0000BE7C File Offset: 0x0000A07C
		public GameMobsGroupControllerBase Group { get; set; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x0600035B RID: 859 RVA: 0x0000BE85 File Offset: 0x0000A085
		public GameMobsGroupControllerBase LastGroup
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x0600035C RID: 860 RVA: 0x0000BE88 File Offset: 0x0000A088
		// (set) Token: 0x0600035D RID: 861 RVA: 0x0000BE90 File Offset: 0x0000A090
		public GameMobSummoningContext SummonerInfo { get; private set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x0600035E RID: 862 RVA: 0x0000BE99 File Offset: 0x0000A099
		// (set) Token: 0x0600035F RID: 863 RVA: 0x0000BEA1 File Offset: 0x0000A0A1
		public bool IsPlayerMob { get; set; }

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000360 RID: 864 RVA: 0x0000BEAA File Offset: 0x0000A0AA
		// (set) Token: 0x06000361 RID: 865 RVA: 0x0000BEB2 File Offset: 0x0000A0B2
		public bool IsMinorAttackTarget { get; set; }

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000362 RID: 866 RVA: 0x0000BEBB File Offset: 0x0000A0BB
		// (set) Token: 0x06000363 RID: 867 RVA: 0x0000BEC3 File Offset: 0x0000A0C3
		public bool IsSummoned { get; set; }

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000364 RID: 868 RVA: 0x0000BECC File Offset: 0x0000A0CC
		// (set) Token: 0x06000365 RID: 869 RVA: 0x0000BED4 File Offset: 0x0000A0D4
		public bool IsRevived { get; set; }

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000366 RID: 870 RVA: 0x0000BEDD File Offset: 0x0000A0DD
		// (set) Token: 0x06000367 RID: 871 RVA: 0x0000BEE5 File Offset: 0x0000A0E5
		public bool IsSacrificed { get; set; }

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x06000368 RID: 872 RVA: 0x0000BEEE File Offset: 0x0000A0EE
		// (set) Token: 0x06000369 RID: 873 RVA: 0x0000BEF6 File Offset: 0x0000A0F6
		public bool IsKilled { get; private set; }

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x0600036A RID: 874 RVA: 0x0000BEFF File Offset: 0x0000A0FF
		public GameLocation CurrentLocation
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x0600036B RID: 875 RVA: 0x0000BF02 File Offset: 0x0000A102
		public IDamageable HitPointsController
		{
			get
			{
				return this.hitPointsController;
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x0600036C RID: 876 RVA: 0x0000BF0A File Offset: 0x0000A10A
		public AbilityResourcesGenerator ResourcesGenerator
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x0600036D RID: 877 RVA: 0x0000BF0D File Offset: 0x0000A10D
		IBuffsController IBuffableObject.BuffsController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600036E RID: 878 RVA: 0x0000BF10 File Offset: 0x0000A110
		// (set) Token: 0x0600036F RID: 879 RVA: 0x0000BF18 File Offset: 0x0000A118
		public bool IsKinematic { get; set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000370 RID: 880 RVA: 0x0000BF21 File Offset: 0x0000A121
		public GameMobMotionControllerBase MotionController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x06000371 RID: 881 RVA: 0x0000BF24 File Offset: 0x0000A124
		public bool IsCrowdObstacle
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1400002D RID: 45
		// (add) Token: 0x06000372 RID: 882 RVA: 0x0000BF28 File Offset: 0x0000A128
		// (remove) Token: 0x06000373 RID: 883 RVA: 0x0000BF60 File Offset: 0x0000A160
		public event Action<IGameMob> Killed;

		// Token: 0x06000374 RID: 884 RVA: 0x0000BF95 File Offset: 0x0000A195
		public DummyMob(int layer, IDamageable hitPointsController)
		{
			this.hitPointsController = hitPointsController;
			this.Layer = layer;
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0000BFB8 File Offset: 0x0000A1B8
		public void SetCreationType(GameMobCreationType creationType, object context)
		{
			GameMobSummoningContext gameMobSummoningContext = context as GameMobSummoningContext;
			if (gameMobSummoningContext != null)
			{
				this.SummonerInfo = gameMobSummoningContext;
			}
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0000BFD6 File Offset: 0x0000A1D6
		public bool CanBeAttackedBy(IGameMob mob)
		{
			return true;
		}

		// Token: 0x06000377 RID: 887 RVA: 0x0000BFD9 File Offset: 0x0000A1D9
		public void KillMob(object mobKiller)
		{
			if (this.IsKilled)
			{
				return;
			}
			this.IsKilled = true;
			Action<IGameMob> killed = this.Killed;
			if (killed == null)
			{
				return;
			}
			killed(this);
		}

		// Token: 0x06000378 RID: 888 RVA: 0x0000BFFC File Offset: 0x0000A1FC
		public void SetNavMeshAgentActive(bool isActive)
		{
		}

		// Token: 0x04000215 RID: 533
		private readonly IDamageable hitPointsController;

		// Token: 0x04000216 RID: 534
		private int layer;
	}
}
