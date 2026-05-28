using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Game.Localization;
using Unliving.Plot.Triggers;

namespace Unliving.Plot
{
	// Token: 0x020002D9 RID: 729
	[Serializable]
	public sealed class CharactersConversation : CharacterPlotItemsPool<CharacterPhrase>, ICharactersConversation, ICharacterPlotItem, IWeighted, IEnumerable<CharacterPhrase>, IEnumerable, ILocalizableDataHolder, ICloneable<CharactersConversation>
	{
		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x06001954 RID: 6484 RVA: 0x0004FC82 File Offset: 0x0004DE82
		string ILocalizableDataHolder.DataID
		{
			get
			{
				return this.ID;
			}
		}

		// Token: 0x140000F9 RID: 249
		// (add) Token: 0x06001955 RID: 6485 RVA: 0x0004FC8C File Offset: 0x0004DE8C
		// (remove) Token: 0x06001956 RID: 6486 RVA: 0x0004FCC4 File Offset: 0x0004DEC4
		public event Action<ICharactersConversation, ICharacterPlotItem> PhraseStarted;

		// Token: 0x140000FA RID: 250
		// (add) Token: 0x06001957 RID: 6487 RVA: 0x0004FCFC File Offset: 0x0004DEFC
		// (remove) Token: 0x06001958 RID: 6488 RVA: 0x0004FD34 File Offset: 0x0004DF34
		public event Action<ICharactersConversation, ICharacterPlotItem> PhraseCompleted;

		// Token: 0x06001959 RID: 6489 RVA: 0x0004FD69 File Offset: 0x0004DF69
		public CharactersConversation(CharacterPhrase[] items, CharacterPlotItemTriggerBase trigger) : base(items, trigger)
		{
		}

		// Token: 0x0600195A RID: 6490 RVA: 0x0004FD73 File Offset: 0x0004DF73
		public IEnumerator<ICharacterPlotItem> GetConversationIterator()
		{
			return new CharactersConversation.Iterator(this);
		}

		// Token: 0x0600195B RID: 6491 RVA: 0x0004FD7B File Offset: 0x0004DF7B
		IEnumerator<CharacterPhrase> IEnumerable<CharacterPhrase>.GetEnumerator()
		{
			return base.Items.GetEnumerator();
		}

		// Token: 0x0600195C RID: 6492 RVA: 0x0004FD88 File Offset: 0x0004DF88
		IEnumerator IEnumerable.GetEnumerator()
		{
			return base.Items.GetEnumerator();
		}

		// Token: 0x0600195D RID: 6493 RVA: 0x0004FD98 File Offset: 0x0004DF98
		void ILocalizableDataHolder.SetLocalizedData(LocalizationManager localizationManager, object data)
		{
			Metadata metadata = data as Metadata;
			if (metadata != null)
			{
				string[] additionalText = metadata.AdditionalText;
				CharacterPhrase[] array = new CharacterPhrase[additionalText.Length];
				for (int i = 0; i < additionalText.Length; i++)
				{
					string text = additionalText[i];
					int num = text.IndexOf(':');
					if (num > 0 && num < text.Length - 1)
					{
						string id = text.Substring(0, num).Trim();
						CharacterPhrase characterPhrase = new CharacterPhrase
						{
							ID = id,
							text = text.Substring(num + 1, text.Length - (num + 1)),
							RuntimeData = new CharacterPlotItemRuntimeData
							{
								isFinalPlotItem = (i == additionalText.Length - 1)
							},
							speakerName = localizationManager.GetTitle(id),
							voiceoverEvent = metadata.Description
						};
						array[i] = characterPhrase;
					}
					else
					{
						array[i] = new CharacterPhrase
						{
							text = text
						};
					}
				}
				base.SetItems(array);
			}
		}

		// Token: 0x0600195E RID: 6494 RVA: 0x0004FE89 File Offset: 0x0004E089
		public CharactersConversation Clone()
		{
			return new CharactersConversation((CharacterPhrase[])base.Items, this.Trigger)
			{
				ID = this.ID,
				RuntimeData = base.RuntimeData
			};
		}

		// Token: 0x0200053B RID: 1339
		private sealed class Iterator : IEnumerator<CharacterPhrase>, IEnumerator, IDisposable
		{
			// Token: 0x170007DA RID: 2010
			// (get) Token: 0x06002695 RID: 9877 RVA: 0x000783E6 File Offset: 0x000765E6
			public CharacterPhrase Current
			{
				get
				{
					if (this.phraseIndex < 0 || this.phraseIndex >= this.phraseCount)
					{
						throw new InvalidOperationException();
					}
					return this.phrases[this.phraseIndex];
				}
			}

			// Token: 0x170007DB RID: 2011
			// (get) Token: 0x06002696 RID: 9878 RVA: 0x00078416 File Offset: 0x00076616
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			// Token: 0x06002697 RID: 9879 RVA: 0x0007841E File Offset: 0x0007661E
			public Iterator(CharactersConversation conversation)
			{
				this.conversation = conversation;
				this.phrases = conversation.Items;
				this.phraseCount = conversation.Items.Count;
				this.phraseIndex = -1;
			}

			// Token: 0x06002698 RID: 9880 RVA: 0x00078454 File Offset: 0x00076654
			public bool MoveNext()
			{
				int num = this.phraseIndex;
				this.phraseIndex++;
				if (this.phraseIndex < this.phraseCount)
				{
					if (num >= 0)
					{
						Action<ICharactersConversation, ICharacterPlotItem> phraseCompleted = this.conversation.PhraseCompleted;
						if (phraseCompleted != null)
						{
							phraseCompleted(this.conversation, this.phrases[num]);
						}
					}
					Action<ICharactersConversation, ICharacterPlotItem> phraseStarted = this.conversation.PhraseStarted;
					if (phraseStarted != null)
					{
						phraseStarted(this.conversation, this.phrases[this.phraseIndex]);
					}
					return true;
				}
				if (this.phraseIndex == this.phraseCount)
				{
					Action<ICharactersConversation, ICharacterPlotItem> phraseCompleted2 = this.conversation.PhraseCompleted;
					if (phraseCompleted2 != null)
					{
						phraseCompleted2(this.conversation, this.phrases[num]);
					}
				}
				return false;
			}

			// Token: 0x06002699 RID: 9881 RVA: 0x00078516 File Offset: 0x00076716
			public void Reset()
			{
				this.phraseIndex = -1;
			}

			// Token: 0x0600269A RID: 9882 RVA: 0x0007851F File Offset: 0x0007671F
			public void Dispose()
			{
			}

			// Token: 0x04001B80 RID: 7040
			private readonly CharactersConversation conversation;

			// Token: 0x04001B81 RID: 7041
			private readonly IReadOnlyList<CharacterPhrase> phrases;

			// Token: 0x04001B82 RID: 7042
			private readonly int phraseCount;

			// Token: 0x04001B83 RID: 7043
			private int phraseIndex;
		}
	}
}
