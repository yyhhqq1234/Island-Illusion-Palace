using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.ResourcesExchange
{
	// Token: 0x020000DB RID: 219
	public class SacrificePositionTrigger : GameBehaviourBase
	{
		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000562 RID: 1378 RVA: 0x0001364F File Offset: 0x0001184F
		// (set) Token: 0x06000563 RID: 1379 RVA: 0x00013657 File Offset: 0x00011857
		public int SacrificedMobsCount { get; private set; }

		// Token: 0x14000032 RID: 50
		// (add) Token: 0x06000564 RID: 1380 RVA: 0x00013660 File Offset: 0x00011860
		// (remove) Token: 0x06000565 RID: 1381 RVA: 0x00013698 File Offset: 0x00011898
		public event Action<BaseGameMob, SacrificePositionTrigger> MobSacrificed;

		// Token: 0x06000566 RID: 1382 RVA: 0x000136CD File Offset: 0x000118CD
		public void SetMobsLayerMask(LayerMask layerMask)
		{
			this.mobsLayerMask = layerMask;
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x000136D8 File Offset: 0x000118D8
		public void Deactivate()
		{
			this.SacrificedMobsCount = 0;
			this.MobSacrificed = null;
			Collider2D component = base.GetComponent<Collider2D>();
			if (component != null)
			{
				component.enabled = false;
			}
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0001370C File Offset: 0x0001190C
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (!collider.InLayerMask(this.mobsLayerMask))
			{
				return;
			}
			BaseGameMob component = collider.GetComponent<BaseGameMob>();
			if (component == null || this.mobs.Contains(component))
			{
				return;
			}
			this.mobs.Add(component);
			component.Sacrificed += this.OnMobSacrificed;
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0001376C File Offset: 0x0001196C
		private void OnTriggerExit2D(Collider2D collider)
		{
			if (!collider.InLayerMask(this.mobsLayerMask))
			{
				return;
			}
			BaseGameMob component = collider.GetComponent<BaseGameMob>();
			if (component == null || !this.mobs.Contains(component))
			{
				return;
			}
			this.mobs.Remove(component);
			component.Sacrificed -= this.OnMobSacrificed;
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x000137CC File Offset: 0x000119CC
		private void OnMobSacrificed(BaseGameMob mob)
		{
			int sacrificedMobsCount = this.SacrificedMobsCount;
			this.SacrificedMobsCount = sacrificedMobsCount + 1;
			Action<BaseGameMob, SacrificePositionTrigger> mobSacrificed = this.MobSacrificed;
			if (mobSacrificed == null)
			{
				return;
			}
			mobSacrificed(mob, this);
		}

		// Token: 0x040003B0 RID: 944
		private readonly List<BaseGameMob> mobs = new List<BaseGameMob>(100);

		// Token: 0x040003B1 RID: 945
		private LayerMask mobsLayerMask;
	}
}
