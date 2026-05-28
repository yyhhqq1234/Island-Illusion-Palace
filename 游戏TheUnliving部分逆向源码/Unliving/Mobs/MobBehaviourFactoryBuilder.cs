using System;
using AK.Wwise;
using Common.Editor.Reorderable;
using Common.Factories;
using Game.Localization;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Mobs.VFX;

namespace Unliving.Mobs
{
	// Token: 0x020001C4 RID: 452
	[CreateAssetMenu(fileName = "MobsFactoryBuilder", menuName = "Game/Factories/Mobs Factory Builder")]
	public sealed class MobBehaviourFactoryBuilder : PrototypeBasedFactoryBuilder<MobBehaviour.FactoryPrototype, IGameMob>, ILocalizable
	{
		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06000DF1 RID: 3569 RVA: 0x0002C5A6 File Offset: 0x0002A7A6
		// (set) Token: 0x06000DF2 RID: 3570 RVA: 0x0002C5B3 File Offset: 0x0002A7B3
		public override MobBehaviour.FactoryPrototype[] FactoryData
		{
			get
			{
				return this.factoryData;
			}
			set
			{
				this.factoryData.list = value;
			}
		}

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06000DF3 RID: 3571 RVA: 0x0002C5C1 File Offset: 0x0002A7C1
		// (set) Token: 0x06000DF4 RID: 3572 RVA: 0x0002C5C9 File Offset: 0x0002A7C9
		public GameMobFactionInfo[] MobFactionsInfo
		{
			get
			{
				return this._mobFactionsInfo;
			}
			set
			{
				this._mobFactionsInfo = value;
			}
		}

		// Token: 0x06000DF5 RID: 3573 RVA: 0x0002C5D4 File Offset: 0x0002A7D4
		protected override PrototypeBasedFactory<MobBehaviour.FactoryPrototype, IGameMob> CreateFactoryInternal()
		{
			return new GameMobsFactory(this._mobFactionsInfo, this.globalResourcesGeneratorData)
			{
				disableAbilityResourcesGeneration = this.disableAbilityResourcesGeneration,
				playerMobsDecayParameters = this.playerMobsDecayParameters,
				playerMobsResourcesCollectionEffects = this.playerMobsResourcesCollectionEffects,
				playerMobsSacrificationIndicationEffect = this.playerMobsSacrificationIndicationEffect,
				playerMobsDeathIndicationEffect = this.playerMobsDeathIndicationEffect,
				playerMobsArmyLimitDeathIndicationEffect = this.playerMobsArmyLimitDeathIndicationEffect,
				useAttachableMobsEffectsPool = this.useAttachableMobsEffectsPool,
				crowdCollisionLayers = this.mobsCrowdCollisionLayers,
				playerMobsSacrificationIndicationEvent = this.playerMobsSacrificationIndicationEvent,
				playerMobsDeathIndicationEvent = this.playerMobsDeathIndicationEvent
			};
		}

		// Token: 0x06000DF6 RID: 3574 RVA: 0x0002C670 File Offset: 0x0002A870
		object[] ILocalizable.GetLocalizableObjects()
		{
			return this.factoryData.list;
		}

		// Token: 0x04000827 RID: 2087
		[SerializeField]
		private GameMobFactionInfo[] _mobFactionsInfo;

		// Token: 0x04000828 RID: 2088
		public LayerMask mobsCrowdCollisionLayers;

		// Token: 0x04000829 RID: 2089
		public bool disableAbilityResourcesGeneration;

		// Token: 0x0400082A RID: 2090
		public BaseGameMob.ResourcesGeneratorData globalResourcesGeneratorData;

		// Token: 0x0400082B RID: 2091
		[FormerlySerializedAs("globalMobsDecayParameters")]
		public MobHealthController.DecayControllerParams playerMobsDecayParameters;

		// Token: 0x0400082C RID: 2092
		public GameMobVFXController.ResourcesCollectionEffect[] playerMobsResourcesCollectionEffects;

		// Token: 0x0400082D RID: 2093
		public AttachableVisualEffectSpawner playerMobsSacrificationIndicationEffect;

		// Token: 0x0400082E RID: 2094
		public AttachableVisualEffectSpawner playerMobsDeathIndicationEffect;

		// Token: 0x0400082F RID: 2095
		public AttachableVisualEffectSpawner playerMobsArmyLimitDeathIndicationEffect;

		// Token: 0x04000830 RID: 2096
		public bool useAttachableMobsEffectsPool;

		// Token: 0x04000831 RID: 2097
		public AK.Wwise.Event playerMobsSacrificationIndicationEvent;

		// Token: 0x04000832 RID: 2098
		public AK.Wwise.Event playerMobsDeathIndicationEvent;

		// Token: 0x04000833 RID: 2099
		[SerializeField]
		[CustomReorderableList(null, "objectPrefab", true, true)]
		private ReorderableListAdapter<MobBehaviour.FactoryPrototype[]> factoryData;
	}
}
