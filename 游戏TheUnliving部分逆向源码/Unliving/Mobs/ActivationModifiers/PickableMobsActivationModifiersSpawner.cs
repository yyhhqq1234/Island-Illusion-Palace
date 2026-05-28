using System;
using Common.Editor;
using Common.Factories;
using Game.Core;
using Game.Factories;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.Mobs.ActivationModifiers
{
	// Token: 0x02000225 RID: 549
	public sealed class PickableMobsActivationModifiersSpawner : ObjectSpawnerBase<MobActivationModifierID, GameObject>
	{
		// Token: 0x060012D5 RID: 4821 RVA: 0x0003BDE9 File Offset: 0x00039FE9
		protected override IFactory GetFactory(IGame currentGame)
		{
			return currentGame.Services.Get<IObjectFactory<MobActivationAbilityModifier>>();
		}

		// Token: 0x060012D6 RID: 4822 RVA: 0x0003BDF8 File Offset: 0x00039FF8
		protected override object Spawn(ObjectSpawnerBase<MobActivationModifierID, GameObject>.BaseSpawningInfoItem spawningInfo, IFactory targetFactory)
		{
			MultiRepresentationObjectInstantiator.ObjectType objectType = this.spawningContext;
			if (objectType == MultiRepresentationObjectInstantiator.ObjectType.Default || objectType == MultiRepresentationObjectInstantiator.ObjectType.Icon)
			{
				Debug.LogError(string.Format("{0}: {1} spawning is not allowed.", this, this.spawningContext));
				return null;
			}
			PickableMobsActivationModifiersSpawner.FactoryQuery.modifierID = spawningInfo.ObjectID;
			PickableMobsActivationModifiersSpawner.FactoryQuery.objectType = this.spawningContext;
			PickableMobsActivationModifiersSpawner.FactoryQuery.spawnPosition = base.transform.position;
			return targetFactory.Create(PickableMobsActivationModifiersSpawner.FactoryQuery);
		}

		// Token: 0x060012D7 RID: 4823 RVA: 0x0003BE78 File Offset: 0x0003A078
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			ObjectSpawnerBase<MobActivationModifierID, GameObject>.BaseSpawningInfoItem[] spawningInfo = this.spawnerData;
			base.SpawningInfo = spawningInfo;
		}

		// Token: 0x04000B10 RID: 2832
		private static readonly MobsActivationModifiersFactory.Args FactoryQuery = new MobsActivationModifiersFactory.Args();

		// Token: 0x04000B11 RID: 2833
		public MultiRepresentationObjectInstantiator.ObjectType spawningContext = MultiRepresentationObjectInstantiator.ObjectType.PickableObject;

		// Token: 0x04000B12 RID: 2834
		public PickableMobsActivationModifiersSpawner.ModifierSpawningInfo[] spawnerData;

		// Token: 0x020004C5 RID: 1221
		[Serializable]
		public sealed class ModifierSpawningInfo : ObjectSpawnerBase<MobActivationModifierID, GameObject>.BaseSpawningInfoItem
		{
			// Token: 0x1700079A RID: 1946
			// (get) Token: 0x06002542 RID: 9538 RVA: 0x00073960 File Offset: 0x00071B60
			// (set) Token: 0x06002543 RID: 9539 RVA: 0x00073968 File Offset: 0x00071B68
			public override MobActivationModifierID ObjectID
			{
				get
				{
					return this.modifierID;
				}
				set
				{
					this.modifierID = value;
				}
			}

			// Token: 0x040019B2 RID: 6578
			[SerializeField]
			[EnumPopup]
			private MobActivationModifierID modifierID;
		}
	}
}
