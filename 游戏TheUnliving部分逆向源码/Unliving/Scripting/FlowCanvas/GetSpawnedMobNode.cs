using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.UnityExtensions;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200009F RID: 159
	[Name("Get Spawned Mob", 0)]
	[Category("Unliving/Spawners")]
	public sealed class GetSpawnedMobNode : FlowControlNode
	{
		// Token: 0x06000431 RID: 1073 RVA: 0x0000EBAC File Offset: 0x0000CDAC
		private void UpdateOutValue(Flow flow, BaseGameMob spawnedMob)
		{
			this.spawnedMob = spawnedMob;
			this.flowOut.Call(flow);
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x0000EBC4 File Offset: 0x0000CDC4
		private async void GetSpawnedMob(Flow flow)
		{
			GameObject value = this.spawnerObject.value;
			int mobIndexValue = this.mobIndex.value;
			MobBehaviour.ID mobIDValue = this.mobID.value;
			bool getFirstMobOnlyValue = this.getFirstMobOnly.value;
			this.spawnedMob = null;
			if (value != null)
			{
				if (!(this.cachedSpawner != null))
				{
					if (!value.TryGetComponent<MobBehaviourSpawner>(out this.cachedSpawner))
					{
						return;
					}
				}
				while (!this.cachedSpawner.IsGroupSpawned)
				{
					if (base.graph.IsNull() || !base.graph.isRunning)
					{
						return;
					}
					await Task.Yield();
				}
				GameMobsGroupControllerBase spawnedGroup = this.cachedSpawner.SpawnedGroup;
				IReadOnlyList<BaseGameMob> readOnlyList = (spawnedGroup != null) ? spawnedGroup.Mobs : null;
				if (readOnlyList != null && readOnlyList.Count != 0)
				{
					if (mobIDValue != MobBehaviour.ID.None)
					{
						for (int i = 0; i < readOnlyList.Count; i++)
						{
							MobBehaviour mobBehaviour = readOnlyList[i] as MobBehaviour;
							if (mobBehaviour != null && mobBehaviour.ObjectID == mobIDValue)
							{
								this.UpdateOutValue(flow, mobBehaviour);
								if (getFirstMobOnlyValue)
								{
									break;
								}
							}
						}
					}
					else if (mobIndexValue >= 0 && mobIndexValue < readOnlyList.Count)
					{
						this.UpdateOutValue(flow, readOnlyList[mobIndexValue]);
					}
				}
			}
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x0000EC08 File Offset: 0x0000CE08
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.GetSpawnedMob), "");
			this.spawnerObject = base.AddValueInput<GameObject>("spawnerObject", "");
			this.mobIndex = base.AddValueInput<int>("mobIndex", "");
			this.mobIndex.SetDefaultAndSerializedValue(-1);
			this.mobID = base.AddValueInput<MobBehaviour.ID>("mobID", "");
			this.mobID.SetDefaultAndSerializedValue(MobBehaviour.ID.None);
			this.getFirstMobOnly = base.AddValueInput<bool>("getFirstMobOnly", "");
			this.getFirstMobOnly.SetDefaultAndSerializedValue(false);
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<BaseGameMob>("Spawned Mob", () => this.spawnedMob, "");
		}

		// Token: 0x040002A4 RID: 676
		private ValueInput<GameObject> spawnerObject;

		// Token: 0x040002A5 RID: 677
		private ValueInput<int> mobIndex;

		// Token: 0x040002A6 RID: 678
		private ValueInput<MobBehaviour.ID> mobID;

		// Token: 0x040002A7 RID: 679
		private ValueInput<bool> getFirstMobOnly;

		// Token: 0x040002A8 RID: 680
		private FlowOutput flowOut;

		// Token: 0x040002A9 RID: 681
		private MobBehaviourSpawner cachedSpawner;

		// Token: 0x040002AA RID: 682
		private BaseGameMob spawnedMob;
	}
}
