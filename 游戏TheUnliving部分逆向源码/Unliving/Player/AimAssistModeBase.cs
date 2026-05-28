using System;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000138 RID: 312
	public abstract class AimAssistModeBase : IAimAssistMode
	{
		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060007E3 RID: 2019 RVA: 0x00019F30 File Offset: 0x00018130
		public Vector2 CurrentPointerPosition
		{
			get
			{
				if (!this.selectedMob.IsNull())
				{
					return this.selectedMob.Position;
				}
				return default(Vector2);
			}
		}

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060007E4 RID: 2020
		public abstract AimAssistType AimAssistType { get; }

		// Token: 0x14000048 RID: 72
		// (add) Token: 0x060007E5 RID: 2021 RVA: 0x00019F60 File Offset: 0x00018160
		// (remove) Token: 0x060007E6 RID: 2022 RVA: 0x00019F98 File Offset: 0x00018198
		public event Action ModeStateUpdated;

		// Token: 0x060007E7 RID: 2023 RVA: 0x00019FD0 File Offset: 0x000181D0
		public AimAssistModeBase(BaseGameMob owner)
		{
			this.owner = owner;
			this.playerInput = (owner as PlayerBehaviour).PlayerInputController;
			this.playerInput.PlayerActionPrepared += this.OnPlayerActionPrepared;
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x0001A024 File Offset: 0x00018224
		public virtual void SetData(object data)
		{
			this.data = (AimAssistModeBase.Data)data;
			GameMobsFactory gameMobsFactory;
			if (this.owner.CurrentGame.Services.TryGet<GameMobsFactory>(out gameMobsFactory))
			{
				GameMobFactionInfo factionInfo = gameMobsFactory.GetFactionInfo(this.data.playerGroupFaction);
				if (factionInfo.IsValid())
				{
					this.enemyMobsLayerMask = factionInfo.enemyMobLayers;
				}
			}
		}

		// Token: 0x060007E9 RID: 2025 RVA: 0x0001A084 File Offset: 0x00018284
		private void OnPlayerActionPrepared(PlayerInputController.ActionArgs args)
		{
			if (this.selectedMob.IsNull())
			{
				return;
			}
			if (args.HasActionFlags(this.data.playerMainAttackActions))
			{
				args.clickedObjectTransform = this.selectedMob.transform;
				args.worldCursorPosition = this.selectedMob.Position;
			}
		}

		// Token: 0x060007EA RID: 2026 RVA: 0x0001A0D4 File Offset: 0x000182D4
		public void OnUpdate()
		{
			this.selectedMob = this.FindClosestMob(this.GetSearchPosition());
			Action modeStateUpdated = this.ModeStateUpdated;
			if (modeStateUpdated == null)
			{
				return;
			}
			modeStateUpdated();
		}

		// Token: 0x060007EB RID: 2027
		protected abstract Vector2 GetSearchPosition();

		// Token: 0x060007EC RID: 2028 RVA: 0x0001A0F8 File Offset: 0x000182F8
		protected BaseGameMob FindClosestMob(Vector2 position)
		{
			AimAssistModeBase.<>c__DisplayClass21_0 CS$<>8__locals1;
			CS$<>8__locals1.position = position;
			CS$<>8__locals1.<>4__this = this;
			this.nearestMobsCount = Physics2D.OverlapCircleNonAlloc(CS$<>8__locals1.position, (float)this.data.maxSearchTargetRadius, this.mobsBuffer, this.enemyMobsLayerMask);
			CS$<>8__locals1.minDistance = float.MaxValue;
			Transform transform = null;
			for (int i = 0; i < this.nearestMobsCount; i++)
			{
				Collider2D collider2D = this.mobsBuffer[i];
				if (!(collider2D == null))
				{
					this.<FindClosestMob>g__CheckDistance|21_0(collider2D.transform, ref transform, ref CS$<>8__locals1);
				}
			}
			BaseGameMob result;
			if (transform != null && transform.TryGetComponent<BaseGameMob>(out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x060007ED RID: 2029 RVA: 0x0001A198 File Offset: 0x00018398
		private bool IsValidMob(BaseGameMob mob)
		{
			return !mob.isEnvironmentMob || mob.IsSummoned;
		}

		// Token: 0x060007EE RID: 2030 RVA: 0x0001A1AA File Offset: 0x000183AA
		public virtual void Destroy()
		{
			if (this.playerInput != null)
			{
				this.playerInput.PlayerActionPrepared -= this.OnPlayerActionPrepared;
			}
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x0001A1CC File Offset: 0x000183CC
		[CompilerGenerated]
		private void <FindClosestMob>g__CheckDistance|21_0(Transform transform, ref Transform closestMobTransform, ref AimAssistModeBase.<>c__DisplayClass21_0 A_3)
		{
			float num = (A_3.position - transform.position).SqrMagnitude();
			if (num < A_3.minDistance)
			{
				A_3.minDistance = num;
				BaseGameMob mob;
				if (transform.TryGetComponent<BaseGameMob>(out mob) && this.IsValidMob(mob))
				{
					closestMobTransform = transform;
				}
			}
		}

		// Token: 0x04000484 RID: 1156
		protected BaseGameMob owner;

		// Token: 0x04000485 RID: 1157
		protected PlayerInputController playerInput;

		// Token: 0x04000486 RID: 1158
		private AimAssistModeBase.Data data;

		// Token: 0x04000487 RID: 1159
		private int enemyMobsLayerMask;

		// Token: 0x04000488 RID: 1160
		private Collider2D[] mobsBuffer = new Collider2D[512];

		// Token: 0x04000489 RID: 1161
		private int nearestMobsCount;

		// Token: 0x0400048A RID: 1162
		private BaseGameMob selectedMob;

		// Token: 0x02000444 RID: 1092
		public interface IData
		{
			// Token: 0x1700071D RID: 1821
			// (get) Token: 0x06002341 RID: 9025
			AimAssistType AimAssistType { get; }
		}

		// Token: 0x02000445 RID: 1093
		[Serializable]
		public struct Data : AimAssistModeBase.IData
		{
			// Token: 0x1700071E RID: 1822
			// (get) Token: 0x06002342 RID: 9026 RVA: 0x0006CF75 File Offset: 0x0006B175
			public AimAssistType AimAssistType
			{
				get
				{
					return this.aimAssistType;
				}
			}

			// Token: 0x0400169A RID: 5786
			public AimAssistType aimAssistType;

			// Token: 0x0400169B RID: 5787
			public PlayerAction[] playerMainAttackActions;

			// Token: 0x0400169C RID: 5788
			public GameMobFactions playerGroupFaction;

			// Token: 0x0400169D RID: 5789
			public int maxSearchTargetRadius;
		}
	}
}
