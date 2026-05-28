using System;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x02000044 RID: 68
	public sealed class TestBuffsReceiver : MonoBehaviour, IBuffableObject
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000242 RID: 578 RVA: 0x000098C8 File Offset: 0x00007AC8
		public IBuffsController BuffsController
		{
			get
			{
				return this._buffsController;
			}
		}

		// Token: 0x06000243 RID: 579 RVA: 0x000098D0 File Offset: 0x00007AD0
		private void Awake()
		{
			this._buffsController = new BaseBuffsController(this, null);
		}

		// Token: 0x06000244 RID: 580 RVA: 0x000098DF File Offset: 0x00007ADF
		private void Update()
		{
			this._buffsController.UpdateBuffs();
		}

		// Token: 0x0400016F RID: 367
		private IBuffsController _buffsController;
	}
}
