using System;
using Game.Core;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001F3 RID: 499
	public abstract class GameMobGroupComponentBase<TGroup> : GameBehaviourBase, IGameMobGroupControllerProvider where TGroup : GameMobsGroupControllerBase, new()
	{
		// Token: 0x06001071 RID: 4209 RVA: 0x00033674 File Offset: 0x00031874
		public static implicit operator TGroup(GameMobGroupComponentBase<TGroup> groupComponent)
		{
			if (groupComponent == null)
			{
				return default(TGroup);
			}
			return groupComponent.GroupController;
		}

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x06001072 RID: 4210 RVA: 0x00033694 File Offset: 0x00031894
		public TGroup GroupController
		{
			get
			{
				return this._groupController;
			}
		}

		// Token: 0x17000358 RID: 856
		// (get) Token: 0x06001073 RID: 4211 RVA: 0x0003369C File Offset: 0x0003189C
		GameMobsGroupControllerBase IGameMobGroupControllerProvider.GroupController
		{
			get
			{
				return this._groupController;
			}
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x000336AC File Offset: 0x000318AC
		protected virtual void Start()
		{
			this._groupController.Initialize(this.customGroupID ?? base.GetInstanceID(), base.gameObject, base.transform.position);
		}

		// Token: 0x06001075 RID: 4213 RVA: 0x000336FE File Offset: 0x000318FE
		private void LateUpdate()
		{
			this._groupController.OnUpdate();
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x00033710 File Offset: 0x00031910
		protected override void OnDestroy()
		{
			base.OnDestroy();
			IDisposable disposable = this._groupController as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
			this._groupController = default(TGroup);
		}

		// Token: 0x06001077 RID: 4215 RVA: 0x0003373F File Offset: 0x0003193F
		private void OnDrawGizmosSelected()
		{
			TGroup tgroup = this._groupController;
			if (tgroup == null)
			{
				return;
			}
			tgroup.DrawGizmos();
		}

		// Token: 0x04000959 RID: 2393
		public int? customGroupID;

		// Token: 0x0400095A RID: 2394
		[SerializeField]
		protected TGroup _groupController = Activator.CreateInstance<TGroup>();
	}
}
