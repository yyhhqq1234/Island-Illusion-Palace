using System;
using Common.Editor;
using Common.Factories;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.Pickables;

namespace Unliving.Mobs.ActivationModifiers
{
	// Token: 0x02000223 RID: 547
	[Service(typeof(MobsActivationModifiersFactory), new Type[]
	{
		typeof(IObjectFactory<MobActivationAbilityModifier>),
		typeof(IFactory<int, MobActivationAbilityModifier>)
	})]
	public sealed class MobsActivationModifiersFactory : PrototypeBasedFactory<MobsActivationModifiersFactory.PrototypeInfo, MobActivationAbilityModifier>, IAbilityPropertiesOverridesHandler
	{
		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x060012CA RID: 4810 RVA: 0x0003BBB5 File Offset: 0x00039DB5
		// (set) Token: 0x060012CB RID: 4811 RVA: 0x0003BBBD File Offset: 0x00039DBD
		public IAbilityPropertiesOverridesSource AbilityPropertiesOverridesSource { get; set; }

		// Token: 0x060012CC RID: 4812 RVA: 0x0003BBC6 File Offset: 0x00039DC6
		protected override void OnPrototypeDataAdded(MobsActivationModifiersFactory.PrototypeInfo prototypeData)
		{
			prototypeData.modifierPrototype.ModifierID = prototypeData.modifierID;
		}

		// Token: 0x060012CD RID: 4813 RVA: 0x0003BBDC File Offset: 0x00039DDC
		private void OnMultiRepresentationObjectCreated(UnityEngine.Object createdObject, MultiRepresentationObjectInstantiator.IObjectData objectData, MultiRepresentationObjectInstantiator.IArgs args)
		{
			IPurchasableObject purchasableObject = createdObject.CastOrGetComponent<IPurchasableObject>();
			MobsActivationModifiersFactory.PrototypeInfo prototypeInfo = objectData as MobsActivationModifiersFactory.PrototypeInfo;
			if (prototypeInfo != null)
			{
				if (prototypeInfo.uiIcon.IsNull())
				{
					prototypeInfo.uiIcon = this.defaultModifiersData.uiIcon;
				}
				if (prototypeInfo.icon.IsNull())
				{
					prototypeInfo.icon = this.defaultModifiersData.objectIcon;
				}
			}
			if (purchasableObject != null)
			{
				purchasableObject.CurrentPickingContext = args.Type;
				purchasableObject.InitializeData(args, objectData);
			}
		}

		// Token: 0x060012CE RID: 4814 RVA: 0x0003BC50 File Offset: 0x00039E50
		public override object Create(object args)
		{
			if (this.representationsInstantiator == null)
			{
				this.representationsInstantiator = new MultiRepresentationObjectInstantiator(new Func<MultiRepresentationObjectInstantiator.IArgs, MultiRepresentationObjectInstantiator.IObjectData>(base.GetObjectPrototype), this.defaultModifiersData, new Action<UnityEngine.Object, MultiRepresentationObjectInstantiator.IObjectData, MultiRepresentationObjectInstantiator.IArgs>(this.OnMultiRepresentationObjectCreated));
			}
			return this.representationsInstantiator.CreateObject<MobActivationModifierID>(args, new Func<object, object>(base.Create));
		}

		// Token: 0x060012CF RID: 4815 RVA: 0x0003BCA8 File Offset: 0x00039EA8
		protected override MobActivationAbilityModifier Create(MobsActivationModifiersFactory.PrototypeInfo data, IBaseObjectDescription args)
		{
			MobsActivationModifiersFactory.Args args2 = args as MobsActivationModifiersFactory.Args;
			MobActivationAbilityModifier mobActivationAbilityModifier = ((args2 != null) ? args2.modifierPrototype : null) ?? ((data != null) ? data.modifierPrototype : null);
			MobActivationAbilityModifier mobActivationAbilityModifier2 = null;
			BaseAbility baseAbility = (args2 != null) ? args2.targetAbility : null;
			if (mobActivationAbilityModifier != null)
			{
				IAbilityExtension abilityExtension;
				mobActivationAbilityModifier.TryInstantiate(out abilityExtension);
				mobActivationAbilityModifier2 = (MobActivationAbilityModifier)abilityExtension;
				mobActivationAbilityModifier2.ModifierID = ((args2 != null) ? args2.modifierID : data.modifierID);
				if (baseAbility != null)
				{
					int num = Mathf.Max(args2.modifierLevel, 1);
					AbilityLevelBasedPropertiesModifier abilityLevelBasedPropertiesModifier = mobActivationAbilityModifier2.modifierLevelingController;
					if (num > 1)
					{
						baseAbility.GetOrAddModifiersOverrides().levelOverride = num;
					}
					baseAbility.AddExtension(mobActivationAbilityModifier2);
					if (abilityLevelBasedPropertiesModifier != null)
					{
						IAbilityExtension abilityExtension2;
						abilityLevelBasedPropertiesModifier.TryInstantiate(out abilityExtension2);
						abilityLevelBasedPropertiesModifier = (AbilityLevelBasedPropertiesModifier)abilityExtension2;
						if (this.AbilityPropertiesOverridesSource != null)
						{
							AbilityLevelBasedPropertiesModifier modifierLevelingController = mobActivationAbilityModifier.modifierLevelingController;
							abilityLevelBasedPropertiesModifier.AbilityPropertiesOverrides = this.AbilityPropertiesOverridesSource.GetAbilityPropertiesOverrides(modifierLevelingController);
						}
						abilityLevelBasedPropertiesModifier.SetAbilityLevelOverride(baseAbility, num);
						baseAbility.AddExtension(abilityLevelBasedPropertiesModifier);
					}
				}
			}
			return mobActivationAbilityModifier2;
		}

		// Token: 0x04000B0C RID: 2828
		public MultiRepresentationObjectInstantiator.DefaultData defaultModifiersData;

		// Token: 0x04000B0D RID: 2829
		private MultiRepresentationObjectInstantiator representationsInstantiator;

		// Token: 0x020004C3 RID: 1219
		public sealed class Args : MultiRepresentationObjectInstantiator.IArgs, IBaseObjectDescription, IItemLevelProvider
		{
			// Token: 0x1700078E RID: 1934
			// (get) Token: 0x06002531 RID: 9521 RVA: 0x000738D5 File Offset: 0x00071AD5
			// (set) Token: 0x06002532 RID: 9522 RVA: 0x000738DD File Offset: 0x00071ADD
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.modifierID;
				}
				set
				{
					this.modifierID = (MobActivationModifierID)value;
				}
			}

			// Token: 0x1700078F RID: 1935
			// (get) Token: 0x06002533 RID: 9523 RVA: 0x000738E6 File Offset: 0x00071AE6
			// (set) Token: 0x06002534 RID: 9524 RVA: 0x000738EE File Offset: 0x00071AEE
			MultiRepresentationObjectInstantiator.ObjectType MultiRepresentationObjectInstantiator.IArgs.Type
			{
				get
				{
					return this.objectType;
				}
				set
				{
					this.objectType = value;
				}
			}

			// Token: 0x17000790 RID: 1936
			// (get) Token: 0x06002535 RID: 9525 RVA: 0x000738F7 File Offset: 0x00071AF7
			int IItemLevelProvider.ItemLevel
			{
				get
				{
					return this.modifierLevel;
				}
			}

			// Token: 0x17000791 RID: 1937
			// (get) Token: 0x06002536 RID: 9526 RVA: 0x000738FF File Offset: 0x00071AFF
			Vector3 MultiRepresentationObjectInstantiator.IArgs.SpawnPosition
			{
				get
				{
					return this.spawnPosition;
				}
			}

			// Token: 0x040019A5 RID: 6565
			public MobActivationAbilityModifier modifierPrototype;

			// Token: 0x040019A6 RID: 6566
			public MobActivationModifierID modifierID;

			// Token: 0x040019A7 RID: 6567
			public int modifierLevel;

			// Token: 0x040019A8 RID: 6568
			public BaseAbility targetAbility;

			// Token: 0x040019A9 RID: 6569
			public MultiRepresentationObjectInstantiator.ObjectType objectType;

			// Token: 0x040019AA RID: 6570
			public Vector2 spawnPosition;
		}

		// Token: 0x020004C4 RID: 1220
		[Serializable]
		public sealed class PrototypeInfo : IUnityObjectDescription, IBaseObjectDescription, MultiRepresentationObjectInstantiator.IObjectData
		{
			// Token: 0x17000792 RID: 1938
			// (get) Token: 0x06002538 RID: 9528 RVA: 0x00073914 File Offset: 0x00071B14
			// (set) Token: 0x06002539 RID: 9529 RVA: 0x0007391C File Offset: 0x00071B1C
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.modifierID;
				}
				set
				{
					this.modifierID = (MobActivationModifierID)value;
				}
			}

			// Token: 0x17000793 RID: 1939
			// (get) Token: 0x0600253A RID: 9530 RVA: 0x00073925 File Offset: 0x00071B25
			UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
			{
				get
				{
					return this.modifierPrototype;
				}
			}

			// Token: 0x17000794 RID: 1940
			// (get) Token: 0x0600253B RID: 9531 RVA: 0x0007392D File Offset: 0x00071B2D
			GameObject MultiRepresentationObjectInstantiator.IObjectData.PickableObjectPrefab
			{
				get
				{
					return this.pickablePrefab;
				}
			}

			// Token: 0x17000795 RID: 1941
			// (get) Token: 0x0600253C RID: 9532 RVA: 0x00073935 File Offset: 0x00071B35
			GameObject MultiRepresentationObjectInstantiator.IObjectData.HomespaceObjectPrefab
			{
				get
				{
					return this.homespacePrefab;
				}
			}

			// Token: 0x17000796 RID: 1942
			// (get) Token: 0x0600253D RID: 9533 RVA: 0x0007393D File Offset: 0x00071B3D
			GameObject MultiRepresentationObjectInstantiator.IObjectData.StoreObjectPrefab
			{
				get
				{
					return this.storePrefab;
				}
			}

			// Token: 0x17000797 RID: 1943
			// (get) Token: 0x0600253E RID: 9534 RVA: 0x00073945 File Offset: 0x00071B45
			Sprite MultiRepresentationObjectInstantiator.IObjectData.ObjectIcon
			{
				get
				{
					return this.icon;
				}
			}

			// Token: 0x17000798 RID: 1944
			// (get) Token: 0x0600253F RID: 9535 RVA: 0x0007394D File Offset: 0x00071B4D
			Sprite MultiRepresentationObjectInstantiator.IObjectData.UIIcon
			{
				get
				{
					return this.uiIcon;
				}
			}

			// Token: 0x17000799 RID: 1945
			// (get) Token: 0x06002540 RID: 9536 RVA: 0x00073955 File Offset: 0x00071B55
			bool MultiRepresentationObjectInstantiator.IObjectData.CanBePickedInHomespace
			{
				get
				{
					return false;
				}
			}

			// Token: 0x040019AB RID: 6571
			[EnumPopup]
			public MobActivationModifierID modifierID;

			// Token: 0x040019AC RID: 6572
			public MobActivationAbilityModifier modifierPrototype;

			// Token: 0x040019AD RID: 6573
			public GameObject pickablePrefab;

			// Token: 0x040019AE RID: 6574
			public GameObject homespacePrefab;

			// Token: 0x040019AF RID: 6575
			public GameObject storePrefab;

			// Token: 0x040019B0 RID: 6576
			public Sprite icon;

			// Token: 0x040019B1 RID: 6577
			public Sprite uiIcon;
		}
	}
}
