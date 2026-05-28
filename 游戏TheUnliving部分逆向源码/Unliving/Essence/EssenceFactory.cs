using System;
using Common;
using Common.Factories;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Factories;
using Unliving.LevelGeneration;
using Unliving.Plot.TreeBasedCharacterPlot;
using Unliving.Plot.TreeBasedCharacterPlot.Test;

namespace Unliving.Essence
{
	// Token: 0x020002CB RID: 715
	[Service(typeof(EssenceFactory), new Type[]
	{
		typeof(IObjectFactory<EssenceBase>),
		typeof(IFactory<int, EssenceBase>)
	})]
	public sealed class EssenceFactory : UnityObjectPrototypeBasedFactory<EssenceFactory.PrototypeInfo, EssenceBase>, IInitializable<IGame>
	{
		// Token: 0x060018EB RID: 6379 RVA: 0x0004EB00 File Offset: 0x0004CD00
		private void OnMultiRepresentationObjectCreated(UnityEngine.Object createdObject, MultiRepresentationObjectInstantiator.IObjectData objectData, MultiRepresentationObjectInstantiator.IArgs args)
		{
			EssenceFactory.PrototypeInfo prototypeInfo = objectData as EssenceFactory.PrototypeInfo;
			if (prototypeInfo != null)
			{
				EssenceBase essenceBase = createdObject.CastOrGetComponent<EssenceBase>();
				if (essenceBase != null)
				{
					essenceBase.characterPlotGraph = prototypeInfo.characterPlotGraph;
					essenceBase.plotProgressOverrideGenerator = prototypeInfo.plotProgressOverrideGenerator;
					essenceBase.CurrentPickingContext = args.Type;
					essenceBase.locationsLevelsData = this.locationsLevelsData;
					essenceBase.levelGenerationSettings = this.levelGenerationSettings;
					essenceBase.InitializeData(args, objectData);
				}
			}
		}

		// Token: 0x060018EC RID: 6380 RVA: 0x0004EB6C File Offset: 0x0004CD6C
		public UnityEngine.Object Create(EssenceFactoryArgs args)
		{
			if (this.representationsInstantiator == null)
			{
				this.representationsInstantiator = new MultiRepresentationObjectInstantiator(new Func<MultiRepresentationObjectInstantiator.IArgs, MultiRepresentationObjectInstantiator.IObjectData>(base.GetObjectPrototype), this.defaultEssenceData, new Action<UnityEngine.Object, MultiRepresentationObjectInstantiator.IObjectData, MultiRepresentationObjectInstantiator.IArgs>(this.OnMultiRepresentationObjectCreated))
				{
					PickableObjectSelector = delegate(MultiRepresentationObjectInstantiator.IObjectData data)
					{
						LocationDependentObjectInfo locationDependentObjectInfo;
						if (LocationDependentDataSelector<LocationDependentObjectInfo>.TryGetData(((EssenceFactory.PrototypeInfo)data).pickableEssencePrefabs, this.currentGame, out locationDependentObjectInfo))
						{
							return locationDependentObjectInfo.gameObject;
						}
						return null;
					}
				};
			}
			return this.representationsInstantiator.CreateObject<EssenceType>(args, new Func<object, object>(base.Create)).CastOrGetComponent<EssenceBase>();
		}

		// Token: 0x060018ED RID: 6381 RVA: 0x0004EBD9 File Offset: 0x0004CDD9
		public override object Create(object args)
		{
			return this.Create((EssenceFactoryArgs)args);
		}

		// Token: 0x060018EE RID: 6382 RVA: 0x0004EBE7 File Offset: 0x0004CDE7
		void IInitializable<IGame>.Initialize(IGame game)
		{
			this.currentGame = game;
		}

		// Token: 0x04000E0B RID: 3595
		public MultiRepresentationObjectInstantiator.DefaultData defaultEssenceData;

		// Token: 0x04000E0C RID: 3596
		public EssenceFactoryBuilder.LocationLevelData[] locationsLevelsData;

		// Token: 0x04000E0D RID: 3597
		public EssenceFactoryBuilder.LevelGenerationSettings levelGenerationSettings;

		// Token: 0x04000E0E RID: 3598
		private MultiRepresentationObjectInstantiator representationsInstantiator;

		// Token: 0x04000E0F RID: 3599
		private IGame currentGame;

		// Token: 0x02000534 RID: 1332
		[Serializable]
		public sealed class PrototypeInfo : IUnityObjectDescription, IBaseObjectDescription, MultiRepresentationObjectInstantiator.IObjectData
		{
			// Token: 0x170007D1 RID: 2001
			// (get) Token: 0x0600267F RID: 9855 RVA: 0x000782F6 File Offset: 0x000764F6
			// (set) Token: 0x06002680 RID: 9856 RVA: 0x000782FE File Offset: 0x000764FE
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.essenceType;
				}
				set
				{
					this.essenceType = (EssenceType)value;
				}
			}

			// Token: 0x170007D2 RID: 2002
			// (get) Token: 0x06002681 RID: 9857 RVA: 0x00078307 File Offset: 0x00076507
			UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
			{
				get
				{
					return this.PickableObjectPrefab;
				}
			}

			// Token: 0x170007D3 RID: 2003
			// (get) Token: 0x06002682 RID: 9858 RVA: 0x0007830F File Offset: 0x0007650F
			public GameObject PickableObjectPrefab
			{
				get
				{
					if (this.pickableEssencePrefabs.Length == 0)
					{
						return null;
					}
					return this.pickableEssencePrefabs[0].gameObject;
				}
			}

			// Token: 0x170007D4 RID: 2004
			// (get) Token: 0x06002683 RID: 9859 RVA: 0x0007832D File Offset: 0x0007652D
			public GameObject HomespaceObjectPrefab
			{
				get
				{
					return null;
				}
			}

			// Token: 0x170007D5 RID: 2005
			// (get) Token: 0x06002684 RID: 9860 RVA: 0x00078330 File Offset: 0x00076530
			public GameObject StoreObjectPrefab
			{
				get
				{
					return this.storeEssencePrefab;
				}
			}

			// Token: 0x170007D6 RID: 2006
			// (get) Token: 0x06002685 RID: 9861 RVA: 0x00078338 File Offset: 0x00076538
			public Sprite ObjectIcon
			{
				get
				{
					return this.icon;
				}
			}

			// Token: 0x170007D7 RID: 2007
			// (get) Token: 0x06002686 RID: 9862 RVA: 0x00078340 File Offset: 0x00076540
			public Sprite UIIcon
			{
				get
				{
					return this.icon;
				}
			}

			// Token: 0x170007D8 RID: 2008
			// (get) Token: 0x06002687 RID: 9863 RVA: 0x00078348 File Offset: 0x00076548
			public bool CanBePickedInHomespace
			{
				get
				{
					return false;
				}
			}

			// Token: 0x04001B6D RID: 7021
			public EssenceType essenceType;

			// Token: 0x04001B6E RID: 7022
			public LocationDependentObjectInfo[] pickableEssencePrefabs;

			// Token: 0x04001B6F RID: 7023
			public GameObject storeEssencePrefab;

			// Token: 0x04001B70 RID: 7024
			public Sprite icon;

			// Token: 0x04001B71 RID: 7025
			public CharacterPlotNodeGraph characterPlotGraph;

			// Token: 0x04001B72 RID: 7026
			public CharacterPlotTreeProgressGenerator plotProgressOverrideGenerator;
		}

		// Token: 0x02000535 RID: 1333
		public sealed class Query : IBaseObjectDescription
		{
			// Token: 0x170007D9 RID: 2009
			// (get) Token: 0x06002689 RID: 9865 RVA: 0x00078353 File Offset: 0x00076553
			// (set) Token: 0x0600268A RID: 9866 RVA: 0x0007835B File Offset: 0x0007655B
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.essenceType;
				}
				set
				{
					this.essenceType = (EssenceType)value;
				}
			}

			// Token: 0x04001B73 RID: 7027
			public EssenceType essenceType;
		}
	}
}
