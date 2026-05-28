using System;
using Common.Factories;
using UnityEngine;

namespace Unliving.Factories
{
	// Token: 0x020002C5 RID: 709
	[Serializable]
	public sealed class MultiRepresentationObjectInstantiator
	{
		// Token: 0x060018BA RID: 6330 RVA: 0x0004DE02 File Offset: 0x0004C002
		private static GameObject Instantiate(MultiRepresentationObjectInstantiator.IArgs args, GameObject prefab, GameObject defaultPrefab)
		{
			if (prefab == null)
			{
				prefab = defaultPrefab;
				if (defaultPrefab == null)
				{
					return null;
				}
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
			gameObject.transform.position = args.SpawnPosition;
			return gameObject;
		}

		// Token: 0x17000541 RID: 1345
		// (get) Token: 0x060018BB RID: 6331 RVA: 0x0004DE32 File Offset: 0x0004C032
		// (set) Token: 0x060018BC RID: 6332 RVA: 0x0004DE3A File Offset: 0x0004C03A
		public Func<MultiRepresentationObjectInstantiator.IObjectData, GameObject> PickableObjectSelector { private get; set; }

		// Token: 0x060018BD RID: 6333 RVA: 0x0004DE43 File Offset: 0x0004C043
		private bool IsValidArgs(MultiRepresentationObjectInstantiator.IArgs args)
		{
			return args != null && args.Type > MultiRepresentationObjectInstantiator.ObjectType.Default;
		}

		// Token: 0x060018BE RID: 6334 RVA: 0x0004DE53 File Offset: 0x0004C053
		public MultiRepresentationObjectInstantiator(Func<MultiRepresentationObjectInstantiator.IArgs, MultiRepresentationObjectInstantiator.IObjectData> objectDataProvider, MultiRepresentationObjectInstantiator.DefaultData defaultData, Action<UnityEngine.Object, MultiRepresentationObjectInstantiator.IObjectData, MultiRepresentationObjectInstantiator.IArgs> onObjectCreated)
		{
			this.objectDataProvider = objectDataProvider;
			this.defaultData = defaultData;
			this.onObjectCreated = onObjectCreated;
		}

		// Token: 0x060018BF RID: 6335 RVA: 0x0004DE70 File Offset: 0x0004C070
		public bool IsMultiRepresentationSpawningArgs(object args)
		{
			return this.IsValidArgs(args as MultiRepresentationObjectInstantiator.IArgs);
		}

		// Token: 0x060018C0 RID: 6336 RVA: 0x0004DE80 File Offset: 0x0004C080
		public UnityEngine.Object CreateObject(MultiRepresentationObjectInstantiator.IArgs args)
		{
			if (this.IsValidArgs(args))
			{
				Func<MultiRepresentationObjectInstantiator.IArgs, MultiRepresentationObjectInstantiator.IObjectData> func = this.objectDataProvider;
				MultiRepresentationObjectInstantiator.IObjectData objectData = (func != null) ? func(args) : null;
				UnityEngine.Object @object = null;
				if (objectData != null)
				{
					switch (args.Type)
					{
					case MultiRepresentationObjectInstantiator.ObjectType.PickableObject:
					{
						Func<MultiRepresentationObjectInstantiator.IObjectData, GameObject> pickableObjectSelector = this.PickableObjectSelector;
						GameObject gameObject = ((pickableObjectSelector != null) ? pickableObjectSelector(objectData) : null) ?? objectData.PickableObjectPrefab;
						GameObject prefab = gameObject;
						MultiRepresentationObjectInstantiator.DefaultData defaultData = this.defaultData;
						@object = MultiRepresentationObjectInstantiator.Instantiate(args, prefab, (defaultData != null) ? defaultData.pickableObjectPrefab : null);
						break;
					}
					case MultiRepresentationObjectInstantiator.ObjectType.Icon:
						@object = (objectData.ObjectIcon ?? this.defaultData.objectIcon);
						break;
					case MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject:
					{
						GameObject homespaceObjectPrefab = objectData.HomespaceObjectPrefab;
						MultiRepresentationObjectInstantiator.DefaultData defaultData2 = this.defaultData;
						@object = MultiRepresentationObjectInstantiator.Instantiate(args, homespaceObjectPrefab, (defaultData2 != null) ? defaultData2.homespaceObjectPrefab : null);
						break;
					}
					case MultiRepresentationObjectInstantiator.ObjectType.StoreObject:
					{
						GameObject storeObjectPrefab = objectData.StoreObjectPrefab;
						MultiRepresentationObjectInstantiator.DefaultData defaultData3 = this.defaultData;
						@object = MultiRepresentationObjectInstantiator.Instantiate(args, storeObjectPrefab, (defaultData3 != null) ? defaultData3.storeObjectPrefab : null);
						break;
					}
					case MultiRepresentationObjectInstantiator.ObjectType.UIIcon:
						@object = (objectData.UIIcon ?? this.defaultData.uiIcon);
						break;
					}
					if (@object != null)
					{
						Action<UnityEngine.Object, MultiRepresentationObjectInstantiator.IObjectData, MultiRepresentationObjectInstantiator.IArgs> action = this.onObjectCreated;
						if (action != null)
						{
							action(@object, objectData, args);
						}
						return @object;
					}
				}
			}
			return null;
		}

		// Token: 0x060018C1 RID: 6337 RVA: 0x0004DFA7 File Offset: 0x0004C1A7
		public UnityEngine.Object CreateObject<T>(object args, Func<object, object> fallbackObjectCreator) where T : Enum
		{
			if (this.IsMultiRepresentationSpawningArgs(args))
			{
				return this.CreateObject((MultiRepresentationObjectInstantiator.IArgs)args);
			}
			return ((fallbackObjectCreator != null) ? fallbackObjectCreator(args) : null) as UnityEngine.Object;
		}

		// Token: 0x04000DE6 RID: 3558
		public MultiRepresentationObjectInstantiator.DefaultData defaultData;

		// Token: 0x04000DE7 RID: 3559
		private readonly Func<MultiRepresentationObjectInstantiator.IArgs, MultiRepresentationObjectInstantiator.IObjectData> objectDataProvider;

		// Token: 0x04000DE8 RID: 3560
		private readonly Action<UnityEngine.Object, MultiRepresentationObjectInstantiator.IObjectData, MultiRepresentationObjectInstantiator.IArgs> onObjectCreated;

		// Token: 0x0200052C RID: 1324
		public enum ObjectType
		{
			// Token: 0x04001B56 RID: 6998
			Default,
			// Token: 0x04001B57 RID: 6999
			PickableObject,
			// Token: 0x04001B58 RID: 7000
			Icon,
			// Token: 0x04001B59 RID: 7001
			HomespaceObject,
			// Token: 0x04001B5A RID: 7002
			StoreObject,
			// Token: 0x04001B5B RID: 7003
			UIIcon
		}

		// Token: 0x0200052D RID: 1325
		public interface IArgs : IBaseObjectDescription
		{
			// Token: 0x170007BC RID: 1980
			// (get) Token: 0x0600265C RID: 9820
			// (set) Token: 0x0600265D RID: 9821
			MultiRepresentationObjectInstantiator.ObjectType Type { get; set; }

			// Token: 0x170007BD RID: 1981
			// (get) Token: 0x0600265E RID: 9822
			Vector3 SpawnPosition { get; }
		}

		// Token: 0x0200052E RID: 1326
		public sealed class DefaultArgs : MultiRepresentationObjectInstantiator.IArgs, IBaseObjectDescription
		{
			// Token: 0x170007BE RID: 1982
			// (get) Token: 0x0600265F RID: 9823 RVA: 0x000781D8 File Offset: 0x000763D8
			// (set) Token: 0x06002660 RID: 9824 RVA: 0x000781E0 File Offset: 0x000763E0
			MultiRepresentationObjectInstantiator.ObjectType MultiRepresentationObjectInstantiator.IArgs.Type { get; set; }

			// Token: 0x170007BF RID: 1983
			// (get) Token: 0x06002661 RID: 9825 RVA: 0x000781EC File Offset: 0x000763EC
			Vector3 MultiRepresentationObjectInstantiator.IArgs.SpawnPosition
			{
				get
				{
					return default(Vector3);
				}
			}

			// Token: 0x170007C0 RID: 1984
			// (get) Token: 0x06002662 RID: 9826 RVA: 0x00078202 File Offset: 0x00076402
			// (set) Token: 0x06002663 RID: 9827 RVA: 0x00078205 File Offset: 0x00076405
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return -1;
				}
				set
				{
				}
			}
		}

		// Token: 0x0200052F RID: 1327
		public interface IObjectData : IBaseObjectDescription
		{
			// Token: 0x170007C1 RID: 1985
			// (get) Token: 0x06002665 RID: 9829
			GameObject PickableObjectPrefab { get; }

			// Token: 0x170007C2 RID: 1986
			// (get) Token: 0x06002666 RID: 9830
			GameObject HomespaceObjectPrefab { get; }

			// Token: 0x170007C3 RID: 1987
			// (get) Token: 0x06002667 RID: 9831
			GameObject StoreObjectPrefab { get; }

			// Token: 0x170007C4 RID: 1988
			// (get) Token: 0x06002668 RID: 9832
			Sprite ObjectIcon { get; }

			// Token: 0x170007C5 RID: 1989
			// (get) Token: 0x06002669 RID: 9833
			Sprite UIIcon { get; }

			// Token: 0x170007C6 RID: 1990
			// (get) Token: 0x0600266A RID: 9834
			bool CanBePickedInHomespace { get; }
		}

		// Token: 0x02000530 RID: 1328
		[Serializable]
		public sealed class DefaultData : MultiRepresentationObjectInstantiator.IObjectData, IBaseObjectDescription
		{
			// Token: 0x170007C7 RID: 1991
			// (get) Token: 0x0600266B RID: 9835 RVA: 0x0007820F File Offset: 0x0007640F
			// (set) Token: 0x0600266C RID: 9836 RVA: 0x00078212 File Offset: 0x00076412
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return -1;
				}
				set
				{
				}
			}

			// Token: 0x170007C8 RID: 1992
			// (get) Token: 0x0600266D RID: 9837 RVA: 0x00078214 File Offset: 0x00076414
			GameObject MultiRepresentationObjectInstantiator.IObjectData.PickableObjectPrefab
			{
				get
				{
					return this.pickableObjectPrefab;
				}
			}

			// Token: 0x170007C9 RID: 1993
			// (get) Token: 0x0600266E RID: 9838 RVA: 0x0007821C File Offset: 0x0007641C
			GameObject MultiRepresentationObjectInstantiator.IObjectData.HomespaceObjectPrefab
			{
				get
				{
					return this.homespaceObjectPrefab;
				}
			}

			// Token: 0x170007CA RID: 1994
			// (get) Token: 0x0600266F RID: 9839 RVA: 0x00078224 File Offset: 0x00076424
			GameObject MultiRepresentationObjectInstantiator.IObjectData.StoreObjectPrefab
			{
				get
				{
					return this.storeObjectPrefab;
				}
			}

			// Token: 0x170007CB RID: 1995
			// (get) Token: 0x06002670 RID: 9840 RVA: 0x0007822C File Offset: 0x0007642C
			Sprite MultiRepresentationObjectInstantiator.IObjectData.ObjectIcon
			{
				get
				{
					return this.objectIcon;
				}
			}

			// Token: 0x170007CC RID: 1996
			// (get) Token: 0x06002671 RID: 9841 RVA: 0x00078234 File Offset: 0x00076434
			Sprite MultiRepresentationObjectInstantiator.IObjectData.UIIcon
			{
				get
				{
					return this.uiIcon;
				}
			}

			// Token: 0x170007CD RID: 1997
			// (get) Token: 0x06002672 RID: 9842 RVA: 0x0007823C File Offset: 0x0007643C
			bool MultiRepresentationObjectInstantiator.IObjectData.CanBePickedInHomespace
			{
				get
				{
					return this.canBePickedInHomespace;
				}
			}

			// Token: 0x04001B5D RID: 7005
			public GameObject pickableObjectPrefab;

			// Token: 0x04001B5E RID: 7006
			public GameObject homespaceObjectPrefab;

			// Token: 0x04001B5F RID: 7007
			public GameObject storeObjectPrefab;

			// Token: 0x04001B60 RID: 7008
			public Sprite objectIcon;

			// Token: 0x04001B61 RID: 7009
			public Sprite uiIcon;

			// Token: 0x04001B62 RID: 7010
			public bool canBePickedInHomespace;
		}
	}
}
