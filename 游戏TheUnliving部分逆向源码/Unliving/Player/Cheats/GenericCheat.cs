using System;
using System.Collections.Generic;
using Common;
using Game.Buffs;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Mobs;

namespace Unliving.Player.Cheats
{
	// Token: 0x0200017C RID: 380
	[Serializable]
	public sealed class GenericCheat : CheatBase
	{
		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06000A90 RID: 2704 RVA: 0x00022CD7 File Offset: 0x00020ED7
		public override string ID
		{
			get
			{
				return this.cheatID;
			}
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x00022CE0 File Offset: 0x00020EE0
		private async void ApplyBuffs(IBuffsController buffReceiver)
		{
			await new WaitForEndOfFrame();
			if (Application.isPlaying && !GameApplication.IsGameStateChanging && ((buffReceiver != null) ? buffReceiver.Owner : null) != null)
			{
				for (int i = 0; i < this.buffGeneratorInstances.Length; i++)
				{
					IBuff buff = this.buffGeneratorInstances[i].GenerateBuff(buffReceiver.Owner, true);
					if (buffReceiver.AddBuff(buff))
					{
						this.appliedBuffs.Add(buff);
					}
				}
			}
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x00022D24 File Offset: 0x00020F24
		protected override bool Activate(CheatContext context)
		{
			if ((!this.canBeUsedInHomespace && context.isHomespace) || (!this.canBeUsedInTutorial && context.isTutorial) || (this.newGameOnlyCheat && !context.isNewGame))
			{
				return false;
			}
			for (int i = 0; i < this.currencyModifiers.Length; i++)
			{
				GenericCheat.CurrencyModifier currencyModifier = this.currencyModifiers[i];
				GenericCheat.CurrencyArgs.CurrencyID = currencyModifier.currency;
				GenericCheat.CurrencyArgs.Amount = (float)currencyModifier.amountModifier;
				context.playerProfile.TryExecuteCurrencyOperation(GenericCheat.CurrencyArgs);
			}
			if (this.buffGeneratorInstances == null)
			{
				BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.buffGenerators;
				generatorsBuilders.Instantiate(out this.buffGeneratorInstances);
			}
			PlayerBehaviour currentPlayer = context.currentPlayer;
			this.ApplyBuffs(currentPlayer.BuffsController);
			return true;
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x00022DE4 File Offset: 0x00020FE4
		protected override void Deactivate(CheatContext context)
		{
			foreach (IBuff buff in this.appliedBuffs)
			{
				buff.Complete();
			}
			this.appliedBuffs.Clear();
		}

		// Token: 0x0400061F RID: 1567
		private static readonly GenericCheat.CurrencyModificationArgs CurrencyArgs = new GenericCheat.CurrencyModificationArgs();

		// Token: 0x04000620 RID: 1568
		[SerializeField]
		private string cheatID;

		// Token: 0x04000621 RID: 1569
		public bool canBeUsedInHomespace;

		// Token: 0x04000622 RID: 1570
		public bool canBeUsedInTutorial;

		// Token: 0x04000623 RID: 1571
		public bool newGameOnlyCheat;

		// Token: 0x04000624 RID: 1572
		[Space]
		public GenericCheat.CurrencyModifier[] currencyModifiers;

		// Token: 0x04000625 RID: 1573
		public BuffsGeneratorBuilderAsset.Reference[] buffGenerators;

		// Token: 0x04000626 RID: 1574
		public VitalContainer.VitalContainerData[] additionalHPContainers;

		// Token: 0x04000627 RID: 1575
		private readonly List<IBuff> appliedBuffs = new List<IBuff>(4);

		// Token: 0x04000628 RID: 1576
		private IBuffsGenerator[] buffGeneratorInstances;

		// Token: 0x02000472 RID: 1138
		[Serializable]
		public struct CurrencyModifier
		{
			// Token: 0x0400175F RID: 5983
			public CurrencyID currency;

			// Token: 0x04001760 RID: 5984
			public int amountModifier;
		}

		// Token: 0x02000473 RID: 1139
		private sealed class CurrencyModificationArgs : ICurrencyOperationArgs, ICloneable<ICurrencyOperationArgs>
		{
			// Token: 0x17000742 RID: 1858
			// (get) Token: 0x060023D3 RID: 9171 RVA: 0x0006EDB7 File Offset: 0x0006CFB7
			// (set) Token: 0x060023D4 RID: 9172 RVA: 0x0006EDBF File Offset: 0x0006CFBF
			public CurrencyID CurrencyID { get; set; }

			// Token: 0x17000743 RID: 1859
			// (get) Token: 0x060023D5 RID: 9173 RVA: 0x0006EDC8 File Offset: 0x0006CFC8
			// (set) Token: 0x060023D6 RID: 9174 RVA: 0x0006EDD5 File Offset: 0x0006CFD5
			public float Amount
			{
				get
				{
					return Mathf.Abs(this.amount);
				}
				set
				{
					this.amount = value;
				}
			}

			// Token: 0x17000744 RID: 1860
			// (get) Token: 0x060023D7 RID: 9175 RVA: 0x0006EDDE File Offset: 0x0006CFDE
			public bool Spending
			{
				get
				{
					return this.amount < 0f;
				}
			}

			// Token: 0x17000745 RID: 1861
			// (get) Token: 0x060023D8 RID: 9176 RVA: 0x0006EDED File Offset: 0x0006CFED
			public object Sender
			{
				get
				{
					return null;
				}
			}

			// Token: 0x060023D9 RID: 9177 RVA: 0x0006EDF0 File Offset: 0x0006CFF0
			public ICurrencyOperationArgs Clone()
			{
				return null;
			}

			// Token: 0x04001762 RID: 5986
			private float amount;
		}
	}
}
