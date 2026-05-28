using System;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Factories;
using UnityEngine;

namespace Unliving.Mobs.Speech
{
	// Token: 0x0200020A RID: 522
	[Service(typeof(MobsPhrasesFactory), new Type[]
	{
		typeof(IObjectFactory<MobPhraseData>)
	})]
	public sealed class MobsPhrasesFactory : PrototypeBasedFactory<MobsPhrasesFactory.PrototypeInfo, MobPhraseData>
	{
		// Token: 0x06001194 RID: 4500 RVA: 0x0003751B File Offset: 0x0003571B
		private float GetTime()
		{
			return Time.time;
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x00037524 File Offset: 0x00035724
		private bool CheckGenerationCondition(float randomValue)
		{
			if (randomValue > this.parameters.globalPhraseProbability)
			{
				return false;
			}
			if (this.GetTime() - this.lastPhraseTime >= this.parameters.phraseGenerationPeriod)
			{
				this.generatedPhrasesCount = 0;
			}
			return this.generatedPhrasesCount < Mathf.Max(this.parameters.maxPhrasesCount, 1);
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x0003757B File Offset: 0x0003577B
		public MobsPhrasesFactory(MobsPhrasesFactory.Parameters parameters)
		{
			this.parameters = parameters;
			this.lastPhraseTime = -parameters.phraseGenerationPeriod;
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x00037598 File Offset: 0x00035798
		protected override MobPhraseData Create(MobsPhrasesFactory.PrototypeInfo prototypeInfo, IBaseObjectDescription query)
		{
			if (prototypeInfo != null)
			{
				MobsPhrasesFactory.Args args = query as MobsPhrasesFactory.Args;
				if (args != null)
				{
					MobPhrasesList mobPhrases = prototypeInfo.GetMobPhrases(args.phraseTrigger);
					MobPhrasesList.MobPhrase mobPhrase;
					if (mobPhrases != null && !mobPhrases.IsEmpty && (args.isForcedQuery || this.CheckGenerationCondition(UnityEngine.Random.value)) && mobPhrases.TryGetRandomPhrase(out mobPhrase))
					{
						this.lastPhraseTime = Time.time;
						this.generatedPhrasesCount++;
						return new MobPhraseData(prototypeInfo.mobID, mobPhrase.localizationKey);
					}
				}
			}
			return null;
		}

		// Token: 0x04000A12 RID: 2578
		private readonly MobsPhrasesFactory.Parameters parameters;

		// Token: 0x04000A13 RID: 2579
		private float lastPhraseTime;

		// Token: 0x04000A14 RID: 2580
		private int generatedPhrasesCount;

		// Token: 0x020004B5 RID: 1205
		public sealed class Args : IBaseObjectDescription
		{
			// Token: 0x17000782 RID: 1922
			// (get) Token: 0x060024FA RID: 9466 RVA: 0x00072F8F File Offset: 0x0007118F
			// (set) Token: 0x060024FB RID: 9467 RVA: 0x00072F97 File Offset: 0x00071197
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.mobID;
				}
				set
				{
					this.mobID = (MobBehaviour.ID)value;
				}
			}

			// Token: 0x04001976 RID: 6518
			public MobBehaviour.ID mobID;

			// Token: 0x04001977 RID: 6519
			public MobPhraseTrigger phraseTrigger;

			// Token: 0x04001978 RID: 6520
			public bool isForcedQuery;
		}

		// Token: 0x020004B6 RID: 1206
		[Serializable]
		public sealed class PrototypeInfo : IBaseObjectDescription
		{
			// Token: 0x17000783 RID: 1923
			// (get) Token: 0x060024FD RID: 9469 RVA: 0x00072FA8 File Offset: 0x000711A8
			// (set) Token: 0x060024FE RID: 9470 RVA: 0x00072FB0 File Offset: 0x000711B0
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.mobID;
				}
				set
				{
					this.mobID = (MobBehaviour.ID)value;
				}
			}

			// Token: 0x060024FF RID: 9471 RVA: 0x00072FBC File Offset: 0x000711BC
			public MobPhrasesList GetMobPhrases(MobPhraseTrigger phraseTrigger)
			{
				for (int i = 0; i < this.mobPhrases.Length; i++)
				{
					MobPhrasesList mobPhrasesList = this.mobPhrases[i];
					if (mobPhrasesList != null && mobPhrasesList.targetTrigger == phraseTrigger)
					{
						return mobPhrasesList;
					}
				}
				return null;
			}

			// Token: 0x04001979 RID: 6521
			[ObjectFactoryIDPopup(typeof(MobBehaviour))]
			public MobBehaviour.ID mobID;

			// Token: 0x0400197A RID: 6522
			public MobPhrasesList[] mobPhrases;
		}

		// Token: 0x020004B7 RID: 1207
		[Serializable]
		public sealed class Parameters
		{
			// Token: 0x0400197B RID: 6523
			[Range(0f, 1f)]
			public float globalPhraseProbability = 1f;

			// Token: 0x0400197C RID: 6524
			public float phraseGenerationPeriod;

			// Token: 0x0400197D RID: 6525
			public int maxPhrasesCount;
		}
	}
}
