using System;
using System.Collections.Generic;
using Common.Utility;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.Abilities;
using Unliving.LevelGeneration;

namespace Unliving.Mobs
{
	// Token: 0x020001CF RID: 463
	public sealed class GameMobAICullingController : MonoBehaviour
	{
		// Token: 0x06000EBD RID: 3773 RVA: 0x0002EB98 File Offset: 0x0002CD98
		private static bool TryGetAIController(ILocationChunkVisitor chunkVisitor, out GameMobAIController aiController)
		{
			if (!chunkVisitor.IsPlayerMobVisitor() && !chunkVisitor.IsPlayerVisitor())
			{
				BaseGameMob baseGameMob = chunkVisitor as BaseGameMob;
				if (baseGameMob != null && baseGameMob.AIController != null && baseGameMob.AIController.CurrentParams.isAggressiveByDefault)
				{
					aiController = baseGameMob.AIController;
					return true;
				}
			}
			aiController = null;
			return false;
		}

		// Token: 0x06000EBE RID: 3774 RVA: 0x0002EBE8 File Offset: 0x0002CDE8
		private static void SetMobActive(GameMobAIController mobAI, bool isActive)
		{
			GameAbilitiesController abilitiesController = mobAI.ControllerOwner.AbilitiesController;
			if (abilitiesController != null)
			{
				abilitiesController.IsActive = isActive;
			}
			mobAI.IsActive = isActive;
		}

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000EBF RID: 3775 RVA: 0x0002EC12 File Offset: 0x0002CE12
		// (set) Token: 0x06000EC0 RID: 3776 RVA: 0x0002EC1F File Offset: 0x0002CE1F
		public float UpdateRate
		{
			get
			{
				return this.updateTimer.Rate;
			}
			set
			{
				if (this.updateTimer.Rate == value)
				{
					return;
				}
				this.updateTimer = new FixedRateTimer(value);
			}
		}

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000EC1 RID: 3777 RVA: 0x0002EC3C File Offset: 0x0002CE3C
		public ILocationChunk TargetLocationChunk
		{
			get
			{
				return this.targetLocationChunk;
			}
		}

		// Token: 0x06000EC2 RID: 3778 RVA: 0x0002EC44 File Offset: 0x0002CE44
		private void RegisterAIController(ILocationChunkVisitor chunkVisitor)
		{
			GameMobAIController gameMobAIController;
			if (GameMobAICullingController.TryGetAIController(chunkVisitor, out gameMobAIController))
			{
				GameMobAICullingController.SetMobActive(gameMobAIController, false);
				this.aiControllers.Add(gameMobAIController);
			}
		}

		// Token: 0x06000EC3 RID: 3779 RVA: 0x0002EC70 File Offset: 0x0002CE70
		private void UnregisterAIController(ILocationChunkVisitor chunkVisitor)
		{
			GameMobAIController item;
			if (GameMobAICullingController.TryGetAIController(chunkVisitor, out item))
			{
				this.aiControllers.Remove(item);
			}
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x0002EC94 File Offset: 0x0002CE94
		public void Initialize(ILocationChunk targetLocationChunk)
		{
			if (this.targetLocationChunk != null || targetLocationChunk == null)
			{
				return;
			}
			foreach (ILocationChunkVisitor chunkVisitor in targetLocationChunk.Visitors)
			{
				this.RegisterAIController(chunkVisitor);
			}
			targetLocationChunk.VisitorAdded += this.OnLocationChunkVisitorAdded;
			targetLocationChunk.VisitorRemoved += this.OnLocationChunkVisitorRemoved;
			this.targetLocationChunk = targetLocationChunk;
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x0002ED18 File Offset: 0x0002CF18
		private void OnLocationChunkVisitorAdded(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			this.RegisterAIController(visitor);
		}

		// Token: 0x06000EC6 RID: 3782 RVA: 0x0002ED21 File Offset: 0x0002CF21
		private void OnLocationChunkVisitorRemoved(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			this.UnregisterAIController(visitor);
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x0002ED2A File Offset: 0x0002CF2A
		private void OnEnable()
		{
			if (this.cullingGroup != null)
			{
				this.cullingGroup.enabled = true;
			}
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x0002ED40 File Offset: 0x0002CF40
		private void Start()
		{
			if (this.targetLocationChunk == null)
			{
				this.Initialize(base.GetComponent<ILocationChunk>());
			}
			if (GameMobAICullingController.mainCamera == null)
			{
				GameMobAICullingController.mainCamera = Camera.main;
			}
			this.cullingGroup = new CullingGroup
			{
				targetCamera = GameMobAICullingController.mainCamera
			};
			this.cullingGroup.SetBoundingSpheres(this.cullingSpheres);
			this.cullingGroup.SetBoundingSphereCount(0);
			this.UpdateRate = 5f;
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x0002EDB8 File Offset: 0x0002CFB8
		private void LateUpdate()
		{
			if (!this.updateTimer.IsTimeReached(false))
			{
				return;
			}
			if (this.cullingSpheres.Length < this.aiControllers.Count)
			{
				Array.Resize<BoundingSphere>(ref this.cullingSpheres, this.aiControllers.Count * 2);
			}
			this.cullingGroup.SetBoundingSphereCount(this.aiControllers.Count);
			for (int i = 0; i < this.aiControllers.Count; i++)
			{
				GameMobAIController gameMobAIController = this.aiControllers[i];
				BaseGameMob controllerOwner = gameMobAIController.ControllerOwner;
				BoundingSphere[] array = this.cullingSpheres;
				int num = i;
				array[num].position = controllerOwner.Position;
				array[num].radius = gameMobAIController.CurrentParams.TargetSearchRadius * 0.8f;
				if (controllerOwner.isEnvironmentMob || !gameMobAIController.IsBusy())
				{
					if (!gameMobAIController.IsActive && controllerOwner.HasThreateners())
					{
						GameMobAICullingController.SetMobActive(gameMobAIController, true);
					}
					else
					{
						GameMobAICullingController.SetMobActive(gameMobAIController, this.cullingGroup.IsVisible(i));
					}
				}
			}
		}

		// Token: 0x06000ECA RID: 3786 RVA: 0x0002EEB8 File Offset: 0x0002D0B8
		private void OnDisable()
		{
			if (this.cullingGroup != null)
			{
				this.cullingGroup.enabled = false;
			}
			foreach (GameMobAIController mobAI in this.aiControllers)
			{
				GameMobAICullingController.SetMobActive(mobAI, true);
			}
		}

		// Token: 0x06000ECB RID: 3787 RVA: 0x0002EF20 File Offset: 0x0002D120
		private void OnDestroy()
		{
			CullingGroup cullingGroup = this.cullingGroup;
			if (cullingGroup != null)
			{
				cullingGroup.Dispose();
			}
			this.cullingGroup = null;
			if (this.targetLocationChunk != null)
			{
				this.targetLocationChunk.VisitorAdded -= this.OnLocationChunkVisitorAdded;
				this.targetLocationChunk.VisitorRemoved -= this.OnLocationChunkVisitorRemoved;
			}
		}

		// Token: 0x040008B7 RID: 2231
		private static Camera mainCamera;

		// Token: 0x040008B8 RID: 2232
		private readonly List<GameMobAIController> aiControllers = new List<GameMobAIController>(128);

		// Token: 0x040008B9 RID: 2233
		private BoundingSphere[] cullingSpheres = new BoundingSphere[512];

		// Token: 0x040008BA RID: 2234
		private CullingGroup cullingGroup;

		// Token: 0x040008BB RID: 2235
		private ILocationChunk targetLocationChunk;

		// Token: 0x040008BC RID: 2236
		private FixedRateTimer updateTimer;
	}
}
