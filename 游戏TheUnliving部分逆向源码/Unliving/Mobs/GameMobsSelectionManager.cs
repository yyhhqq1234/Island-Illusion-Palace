using System;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Core;
using Game.Utility;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001D9 RID: 473
	[Service(typeof(GameMobsSelectionManager), new Type[]
	{
		typeof(SelectableObjectsManager2D)
	})]
	[DisallowMultipleComponent]
	public sealed class GameMobsSelectionManager : SelectableObjectsManager2D
	{
		// Token: 0x06000F52 RID: 3922 RVA: 0x000304EC File Offset: 0x0002E6EC
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			PrototypeBasedFactory<MobBehaviour.FactoryPrototype, IGameMob> prototypeBasedFactory = currentGame.Services.Get<GameMobsFactory>();
			this.maxMobSelectableRadius = float.NegativeInfinity;
			foreach (MobBehaviour.FactoryPrototype factoryPrototype in prototypeBasedFactory.GetObjectPrototypes())
			{
				if (!(factoryPrototype.objectPrefab == null))
				{
					BaseGameMob component = factoryPrototype.objectPrefab.GetComponent<BaseGameMob>();
					if (!(component == null))
					{
						float selectableObjectRadius = SelectableObjectsManager2D.GetSelectableObjectRadius(component.selectionBounds.size);
						if (selectableObjectRadius > this.maxMobSelectableRadius)
						{
							this.maxMobSelectableRadius = selectableObjectRadius;
						}
					}
				}
			}
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x00030598 File Offset: 0x0002E798
		protected override void Start()
		{
			if (this.maxMobSelectableRadius > 0f)
			{
				this.objectsGridCellSize = this.maxMobSelectableRadius * 3.5f;
			}
			base.Start();
		}

		// Token: 0x040008F7 RID: 2295
		private float maxMobSelectableRadius;
	}
}
