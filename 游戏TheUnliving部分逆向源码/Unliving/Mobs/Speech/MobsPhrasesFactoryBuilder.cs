using System;
using Common.Factories;
using UnityEngine;

namespace Unliving.Mobs.Speech
{
	// Token: 0x0200020B RID: 523
	[CreateAssetMenu(fileName = "MobsPhrasesFactoryBuilder", menuName = "Game/Factories/Mobs Phrases Factory Builder")]
	public sealed class MobsPhrasesFactoryBuilder : PrototypeBasedFactoryBuilder<MobsPhrasesFactory.PrototypeInfo, MobPhraseData>
	{
		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x06001198 RID: 4504 RVA: 0x00037615 File Offset: 0x00035815
		// (set) Token: 0x06001199 RID: 4505 RVA: 0x0003761D File Offset: 0x0003581D
		public override MobsPhrasesFactory.PrototypeInfo[] FactoryData
		{
			get
			{
				return this.factoryData;
			}
			set
			{
				this.factoryData = value;
			}
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x00037626 File Offset: 0x00035826
		protected override PrototypeBasedFactory<MobsPhrasesFactory.PrototypeInfo, MobPhraseData> CreateFactoryInternal()
		{
			return new MobsPhrasesFactory(this.factoryParams);
		}

		// Token: 0x04000A15 RID: 2581
		public MobsPhrasesFactory.Parameters factoryParams;

		// Token: 0x04000A16 RID: 2582
		[SerializeField]
		private MobsPhrasesFactory.PrototypeInfo[] factoryData;
	}
}
