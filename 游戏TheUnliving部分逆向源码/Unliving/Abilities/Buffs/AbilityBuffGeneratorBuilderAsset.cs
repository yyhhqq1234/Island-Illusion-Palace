using System;
using Game.Buffs;
using UnityEngine;
using Unliving.Abilities.VFX;

namespace Unliving.Abilities.Buffs
{
	// Token: 0x020003DB RID: 987
	public abstract class AbilityBuffGeneratorBuilderAsset<TGenerator> : BuffsGeneratorBuilderAsset where TGenerator : BuffsGeneratorBase
	{
		// Token: 0x170006D0 RID: 1744
		// (get) Token: 0x0600218B RID: 8587 RVA: 0x00068DB7 File Offset: 0x00066FB7
		public sealed override IBuffsGenerator BuffsGeneratorPrototype
		{
			get
			{
				return this.sourceBuffsGenerator;
			}
		}

		// Token: 0x0600218C RID: 8588 RVA: 0x00068DC4 File Offset: 0x00066FC4
		protected override IBuffsGenerator InstantiateBuffsGenerator()
		{
			TGenerator tgenerator = this.sourceBuffsGenerator;
			BuffsGeneratorBase buffsGeneratorBase = (BuffsGeneratorBase)((tgenerator != null) ? tgenerator.Clone() : null);
			if (buffsGeneratorBase != null && this.buffsVFXData != null)
			{
				for (int i = 0; i < this.buffsVFXData.Length; i++)
				{
					this.buffsVFXData[i].HasDuration = true;
				}
				buffsGeneratorBase.BuffsVFXData = this.buffsVFXData;
			}
			return buffsGeneratorBase;
		}

		// Token: 0x0600218D RID: 8589 RVA: 0x00068E27 File Offset: 0x00067027
		public sealed override int GetBuffsID()
		{
			return this.assetID;
		}

		// Token: 0x0600218E RID: 8590 RVA: 0x00068E2F File Offset: 0x0006702F
		private void Awake()
		{
			if (this.assetID == 0)
			{
				this.assetID = base.GetInstanceID();
			}
			BuffBase.BlockBuffTypeID(this.assetID);
		}

		// Token: 0x040014ED RID: 5357
		[SerializeField]
		private TGenerator sourceBuffsGenerator;

		// Token: 0x040014EE RID: 5358
		public AbilityVFXController.ObjectEffectInfo[] buffsVFXData;

		// Token: 0x040014EF RID: 5359
		[SerializeField]
		[HideInInspector]
		private int assetID;
	}
}
