using System;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C8 RID: 200
	[Name("Spawn Mob", 0)]
	[Category("Unliving/Spawners")]
	public sealed class SpawnMobNode : CallableFunctionNode<BaseGameMob, GameObject, MobBehaviour.ID, Vector2, bool>
	{
		// Token: 0x060004F9 RID: 1273 RVA: 0x000121C0 File Offset: 0x000103C0
		public override BaseGameMob Invoke(GameObject spawnerObject, MobBehaviour.ID mobID = MobBehaviour.ID.None, Vector2 mobPosition = default(Vector2), bool addToGroup = true)
		{
			if (spawnerObject == null || mobID == MobBehaviour.ID.None)
			{
				return null;
			}
			if (this.cachedSpawner != null || spawnerObject.TryGetComponent<MobBehaviourSpawner>(out this.cachedSpawner))
			{
				MobBehaviour mobBehaviour = this.cachedSpawner.SpawnIndividualMob(mobID, mobPosition);
				if (addToGroup && mobBehaviour != null)
				{
					this.cachedSpawner.SpawnedGroup.AddMob(mobBehaviour, null);
				}
				return mobBehaviour;
			}
			return null;
		}

		// Token: 0x04000366 RID: 870
		private MobBehaviourSpawner cachedSpawner;
	}
}
