using System;
using Common.Editor;
using Common.Factories;
using Game.Localization;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Factories;

namespace Unliving.PassiveAbilities
{
	// Token: 0x020001A4 RID: 420
	[LocalizationObject(LocalizationPrefix.passiveability_)]
	[Serializable]
	public sealed class PassiveAbilityFactoryPrototype : IUnityObjectDescription, IBaseObjectDescription, MultiRepresentationObjectInstantiator.IObjectData
	{
		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06000BF9 RID: 3065 RVA: 0x00025D2A File Offset: 0x00023F2A
		// (set) Token: 0x06000BFA RID: 3066 RVA: 0x00025D32 File Offset: 0x00023F32
		int IBaseObjectDescription.ObjectID
		{
			get
			{
				return (int)this.abilityID;
			}
			set
			{
				this.abilityID = (PassiveAbilityID)value;
			}
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000BFB RID: 3067 RVA: 0x00025D3B File Offset: 0x00023F3B
		UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
		{
			get
			{
				return this.abilityPrototype;
			}
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000BFC RID: 3068 RVA: 0x00025D43 File Offset: 0x00023F43
		GameObject MultiRepresentationObjectInstantiator.IObjectData.PickableObjectPrefab
		{
			get
			{
				return this.overrideWorldObjectPrototype;
			}
		}

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000BFD RID: 3069 RVA: 0x00025D4B File Offset: 0x00023F4B
		GameObject MultiRepresentationObjectInstantiator.IObjectData.HomespaceObjectPrefab
		{
			get
			{
				return this.overrideHomespaceObjectPrototype;
			}
		}

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06000BFE RID: 3070 RVA: 0x00025D53 File Offset: 0x00023F53
		GameObject MultiRepresentationObjectInstantiator.IObjectData.StoreObjectPrefab
		{
			get
			{
				return this.overrideStoreObjectPrototype;
			}
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x06000BFF RID: 3071 RVA: 0x00025D5B File Offset: 0x00023F5B
		Sprite MultiRepresentationObjectInstantiator.IObjectData.ObjectIcon
		{
			get
			{
				return this.abilityIcon;
			}
		}

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x06000C00 RID: 3072 RVA: 0x00025D63 File Offset: 0x00023F63
		Sprite MultiRepresentationObjectInstantiator.IObjectData.UIIcon
		{
			get
			{
				return this.abilityIcon;
			}
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x06000C01 RID: 3073 RVA: 0x00025D6B File Offset: 0x00023F6B
		bool MultiRepresentationObjectInstantiator.IObjectData.CanBePickedInHomespace
		{
			get
			{
				return false;
			}
		}

		// Token: 0x040006AB RID: 1707
		[EnumPopup]
		[LocalizationID]
		public PassiveAbilityID abilityID;

		// Token: 0x040006AC RID: 1708
		public PassiveAbilityBase abilityPrototype;

		// Token: 0x040006AD RID: 1709
		[FormerlySerializedAs("worldObjectPrototype")]
		public GameObject overrideWorldObjectPrototype;

		// Token: 0x040006AE RID: 1710
		[FormerlySerializedAs("homespaceObjectPrototype")]
		public GameObject overrideHomespaceObjectPrototype;

		// Token: 0x040006AF RID: 1711
		public GameObject overrideStoreObjectPrototype;

		// Token: 0x040006B0 RID: 1712
		public Sprite abilityIcon;

		// Token: 0x040006B1 RID: 1713
		[Header("Метаданные")]
		[LocalizationTitle]
		[HideInInspector]
		public string abilityName;

		// Token: 0x040006B2 RID: 1714
		[LocalizationDescription]
		public string abilityDescription;
	}
}
