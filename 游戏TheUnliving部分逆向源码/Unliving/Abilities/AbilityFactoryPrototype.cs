using System;
using Common.Editor;
using Common.Factories;
using Game.Abilities;
using Game.Localization;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Factories;

namespace Unliving.Abilities
{
	// Token: 0x0200039F RID: 927
	[LocalizationObject(LocalizationPrefix.ability_)]
	[Serializable]
	public sealed class AbilityFactoryPrototype : IUnityObjectDescription, IBaseObjectDescription, MultiRepresentationObjectInstantiator.IObjectData
	{
		// Token: 0x1700062A RID: 1578
		// (get) Token: 0x06001E8D RID: 7821 RVA: 0x00060BDE File Offset: 0x0005EDDE
		// (set) Token: 0x06001E8E RID: 7822 RVA: 0x00060BE6 File Offset: 0x0005EDE6
		int IBaseObjectDescription.ObjectID
		{
			get
			{
				return (int)this.abilityID;
			}
			set
			{
				this.abilityID = (AbilityID)value;
			}
		}

		// Token: 0x1700062B RID: 1579
		// (get) Token: 0x06001E8F RID: 7823 RVA: 0x00060BEF File Offset: 0x0005EDEF
		UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
		{
			get
			{
				return this.abilityPrototype;
			}
		}

		// Token: 0x1700062C RID: 1580
		// (get) Token: 0x06001E90 RID: 7824 RVA: 0x00060BF7 File Offset: 0x0005EDF7
		GameObject MultiRepresentationObjectInstantiator.IObjectData.PickableObjectPrefab
		{
			get
			{
				return this.overrideWorldObjectPrototype;
			}
		}

		// Token: 0x1700062D RID: 1581
		// (get) Token: 0x06001E91 RID: 7825 RVA: 0x00060BFF File Offset: 0x0005EDFF
		GameObject MultiRepresentationObjectInstantiator.IObjectData.HomespaceObjectPrefab
		{
			get
			{
				return this.overrideHomespaceObjectPrototype;
			}
		}

		// Token: 0x1700062E RID: 1582
		// (get) Token: 0x06001E92 RID: 7826 RVA: 0x00060C07 File Offset: 0x0005EE07
		GameObject MultiRepresentationObjectInstantiator.IObjectData.StoreObjectPrefab
		{
			get
			{
				return this.overrideStoreObjectPrototype;
			}
		}

		// Token: 0x1700062F RID: 1583
		// (get) Token: 0x06001E93 RID: 7827 RVA: 0x00060C0F File Offset: 0x0005EE0F
		Sprite MultiRepresentationObjectInstantiator.IObjectData.ObjectIcon
		{
			get
			{
				return this.abilityIcon;
			}
		}

		// Token: 0x17000630 RID: 1584
		// (get) Token: 0x06001E94 RID: 7828 RVA: 0x00060C17 File Offset: 0x0005EE17
		Sprite MultiRepresentationObjectInstantiator.IObjectData.UIIcon
		{
			get
			{
				return this.abilityUIIcon;
			}
		}

		// Token: 0x17000631 RID: 1585
		// (get) Token: 0x06001E95 RID: 7829 RVA: 0x00060C1F File Offset: 0x0005EE1F
		bool MultiRepresentationObjectInstantiator.IObjectData.CanBePickedInHomespace
		{
			get
			{
				return this.canBePickedInHomespace;
			}
		}

		// Token: 0x04001142 RID: 4418
		[EnumPopup]
		[LocalizationID]
		public AbilityID abilityID;

		// Token: 0x04001143 RID: 4419
		public BaseAbility abilityPrototype;

		// Token: 0x04001144 RID: 4420
		[FormerlySerializedAs("worldObjectPrototype")]
		public GameObject overrideWorldObjectPrototype;

		// Token: 0x04001145 RID: 4421
		[FormerlySerializedAs("homespaceObjectPrototype")]
		public GameObject overrideHomespaceObjectPrototype;

		// Token: 0x04001146 RID: 4422
		public GameObject overrideStoreObjectPrototype;

		// Token: 0x04001147 RID: 4423
		public Sprite abilityIcon;

		// Token: 0x04001148 RID: 4424
		public Sprite abilityUIIcon;

		// Token: 0x04001149 RID: 4425
		[Obsolete]
		[HideInInspector]
		public GameObject rangedAbilityTargetPreviewPrefab;

		// Token: 0x0400114A RID: 4426
		public float playerAbilityUsingImpulse;

		// Token: 0x0400114B RID: 4427
		public bool canBePickedInHomespace;
	}
}
