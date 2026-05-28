using System;
using Common.Factories;
using Common.UnityExtensions;
using Game.Core;
using Game.Factories;
using UltEvents;
using Unliving.DropSystem;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.Essence
{
	// Token: 0x020002C9 RID: 713
	public class EssenceSpawner : ObjectSpawnerBase<EssenceType, EssenceBase>, IPickingContextProvider, IPickableObjectSpawner
	{
		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x060018E2 RID: 6370 RVA: 0x0004E96B File Offset: 0x0004CB6B
		// (set) Token: 0x060018E3 RID: 6371 RVA: 0x0004E973 File Offset: 0x0004CB73
		public MultiRepresentationObjectInstantiator.ObjectType CurrentPickingContext
		{
			get
			{
				return this.currentPickingContext;
			}
			set
			{
				this.currentPickingContext = value;
			}
		}

		// Token: 0x060018E4 RID: 6372 RVA: 0x0004E97C File Offset: 0x0004CB7C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			ObjectSpawnerBase<EssenceType, EssenceBase>.BaseSpawningInfoItem[] spawningInfo = this.initialSpawningInfo;
			base.SpawningInfo = spawningInfo;
		}

		// Token: 0x060018E5 RID: 6373 RVA: 0x0004E99E File Offset: 0x0004CB9E
		public void SpawnEssence()
		{
			base.Spawn();
		}

		// Token: 0x060018E6 RID: 6374 RVA: 0x0004E9A8 File Offset: 0x0004CBA8
		protected override object Spawn(ObjectSpawnerBase<EssenceType, EssenceBase>.BaseSpawningInfoItem spawningInfo, IFactory targetFactory)
		{
			EssenceFactoryArgs query = new EssenceFactoryArgs
			{
				spawnPosition = base.transform.position,
				essenceType = spawningInfo.ObjectID,
				objectType = this.CurrentPickingContext
			};
			object obj = targetFactory.Create(query);
			this.spawnedEssence = obj.CastOrGetComponent<EssenceBase>();
			this.spawnedEssence.transform.SetParent(base.transform);
			EssenceSpawner.EssenceSpawningInfoItem essenceSpawningInfoItem = spawningInfo as EssenceSpawner.EssenceSpawningInfoItem;
			if (essenceSpawningInfoItem != null)
			{
				this.spawnedEssence.dropSpawners = essenceSpawningInfoItem.dropSpawners;
				this.spawnedEssence.essenceType = essenceSpawningInfoItem.essenceType;
				this.spawnedEssence.hideCloseButton = this.hideCloseButton;
			}
			this.spawnedEssence.EmptyEssenceSpawned += this.OnEmptyEssenceSpawned;
			Action<IPickableObject> action = this.onPickableObjectSpawned;
			if (action != null)
			{
				action(this.spawnedEssence);
			}
			this.onPickableObjectSpawned = null;
			return this.spawnedEssence;
		}

		// Token: 0x060018E7 RID: 6375 RVA: 0x0004EA8B File Offset: 0x0004CC8B
		public void InvokeAfterPickableObjectSpawned(Action<IPickableObject> onPickableObjectSpawned)
		{
			if (this.spawnedEssence == null)
			{
				this.onPickableObjectSpawned = onPickableObjectSpawned;
				return;
			}
			if (onPickableObjectSpawned != null)
			{
				onPickableObjectSpawned(this.spawnedEssence);
			}
		}

		// Token: 0x060018E8 RID: 6376 RVA: 0x0004EAB2 File Offset: 0x0004CCB2
		private void OnEmptyEssenceSpawned()
		{
			UltEvent ultEvent = this.emptyEssenceSpawnedEvents;
			if (ultEvent == null)
			{
				return;
			}
			ultEvent.Invoke();
		}

		// Token: 0x060018E9 RID: 6377 RVA: 0x0004EAC4 File Offset: 0x0004CCC4
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.spawnedEssence.IsNull())
			{
				this.spawnedEssence.EmptyEssenceSpawned -= this.OnEmptyEssenceSpawned;
			}
		}

		// Token: 0x04000DF9 RID: 3577
		public EssenceSpawner.EssenceSpawningInfoItem[] initialSpawningInfo;

		// Token: 0x04000DFA RID: 3578
		public UltEvent emptyEssenceSpawnedEvents;

		// Token: 0x04000DFB RID: 3579
		public MultiRepresentationObjectInstantiator.ObjectType currentPickingContext = MultiRepresentationObjectInstantiator.ObjectType.PickableObject;

		// Token: 0x04000DFC RID: 3580
		public bool hideCloseButton;

		// Token: 0x04000DFD RID: 3581
		private EssenceBase spawnedEssence;

		// Token: 0x04000DFE RID: 3582
		private Action<IPickableObject> onPickableObjectSpawned;

		// Token: 0x02000533 RID: 1331
		[Serializable]
		public sealed class EssenceSpawningInfoItem : ObjectSpawnerBase<EssenceType, EssenceBase>.BaseSpawningInfoItem
		{
			// Token: 0x170007D0 RID: 2000
			// (get) Token: 0x0600267C RID: 9852 RVA: 0x000782DD File Offset: 0x000764DD
			// (set) Token: 0x0600267D RID: 9853 RVA: 0x000782E5 File Offset: 0x000764E5
			public override EssenceType ObjectID
			{
				get
				{
					return this.essenceType;
				}
				set
				{
					this.essenceType = value;
				}
			}

			// Token: 0x04001B6B RID: 7019
			public EssenceType essenceType;

			// Token: 0x04001B6C RID: 7020
			public DropSpawner[] dropSpawners;
		}
	}
}
