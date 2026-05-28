using System;
using UnityEngine;

namespace Unliving.AbilityResources
{
	// Token: 0x02000363 RID: 867
	[Serializable]
	public struct GeneratableAbilityResource
	{
		// Token: 0x170005E1 RID: 1505
		// (get) Token: 0x06001C72 RID: 7282 RVA: 0x00059D2F File Offset: 0x00057F2F
		// (set) Token: 0x06001C73 RID: 7283 RVA: 0x00059D37 File Offset: 0x00057F37
		public float GenerationProbability
		{
			get
			{
				return this._generationProbability;
			}
			set
			{
				this._generationProbability = Mathf.Clamp01(value);
			}
		}

		// Token: 0x06001C74 RID: 7284 RVA: 0x00059D45 File Offset: 0x00057F45
		public bool TryGet(float randomValue, out AbilityResourceType generatedResource)
		{
			if (this._generationProbability > 0f && randomValue <= this._generationProbability)
			{
				generatedResource = this.resourceType;
				return true;
			}
			generatedResource = AbilityResourceType.Undefined;
			return false;
		}

		// Token: 0x04001012 RID: 4114
		public AbilityResourceType resourceType;

		// Token: 0x04001013 RID: 4115
		[SerializeField]
		[Range(0f, 1f)]
		private float _generationProbability;
	}
}
