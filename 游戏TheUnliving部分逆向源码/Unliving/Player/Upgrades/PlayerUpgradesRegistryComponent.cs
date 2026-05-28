using System;
using System.Collections.Generic;
using System.Linq;
using FlowCanvas;
using Game.Core;
using NodeCanvas.Framework;
using UnityEngine;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000172 RID: 370
	public sealed class PlayerUpgradesRegistryComponent : GameBehaviourBase, IPlayerUpgradesRegistry
	{
		// Token: 0x06000A3D RID: 2621 RVA: 0x00022208 File Offset: 0x00020408
		public void AddUpgrade(IPlayerUpgrade playerUpgrade)
		{
			if (playerUpgrade is FlowScript)
			{
				FlowScriptController flowScriptController;
				if (!this.flowScriptPlayerUpgrades.TryGetValue(playerUpgrade, out flowScriptController))
				{
					FlowScriptController flowScriptController2 = base.gameObject.AddComponent<FlowScriptController>();
					flowScriptController2.enableAction = GraphOwner.EnableAction.DoNothing;
					flowScriptController2.disableAction = GraphOwner.DisableAction.DisableBehaviour;
					flowScriptController2.updateMode = Graph.UpdateMode.LateUpdate;
					flowScriptController2.behaviour = (FlowScript)playerUpgrade;
					this.flowScriptPlayerUpgrades.Add(playerUpgrade, flowScriptController2);
					base.StartCoroutine(playerUpgrade.Activate(this, flowScriptController2));
					return;
				}
			}
			else if (!this.commonPlayerUpgrades.Contains(playerUpgrade))
			{
				this.commonPlayerUpgrades.Add(playerUpgrade);
				base.StartCoroutine(playerUpgrade.Activate(this, base.CurrentGame));
			}
		}

		// Token: 0x06000A3E RID: 2622 RVA: 0x000222A8 File Offset: 0x000204A8
		public void RemoveUpgrade(IPlayerUpgrade playerUpgrade)
		{
			if (playerUpgrade is FlowScript)
			{
				FlowScriptController obj;
				if (this.flowScriptPlayerUpgrades.TryGetValue(playerUpgrade, out obj))
				{
					this.flowScriptPlayerUpgrades.Remove(playerUpgrade);
					UnityEngine.Object.Destroy(obj);
					return;
				}
			}
			else if (this.commonPlayerUpgrades.Contains(playerUpgrade))
			{
				this.commonPlayerUpgrades.Remove(playerUpgrade);
			}
		}

		// Token: 0x06000A3F RID: 2623 RVA: 0x000222FC File Offset: 0x000204FC
		public IEnumerable<IPlayerUpgrade> GetUpgrades()
		{
			return this.flowScriptPlayerUpgrades.Keys.Concat(this.commonPlayerUpgrades);
		}

		// Token: 0x06000A40 RID: 2624 RVA: 0x00022314 File Offset: 0x00020514
		protected override void OnDestroy()
		{
			base.OnDestroy();
			base.StopAllCoroutines();
		}

		// Token: 0x04000607 RID: 1543
		private readonly List<IPlayerUpgrade> commonPlayerUpgrades = new List<IPlayerUpgrade>(8);

		// Token: 0x04000608 RID: 1544
		private readonly Dictionary<IPlayerUpgrade, FlowScriptController> flowScriptPlayerUpgrades = new Dictionary<IPlayerUpgrade, FlowScriptController>(16);
	}
}
