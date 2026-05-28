using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.RestorableState;
using UnityEngine;

namespace Unliving.Player.Cheats
{
	// Token: 0x0200017B RID: 379
	[Serializable]
	public sealed class CheatManagerRestorableState : RestorableStateBase<CheatManager>, ICloneable<CheatManagerRestorableState>
	{
		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06000A89 RID: 2697 RVA: 0x00022C2E File Offset: 0x00020E2E
		public bool IsAnyCheatSelected
		{
			get
			{
				return this.SelectedCheats != null && this.SelectedCheats.Count > 0;
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06000A8A RID: 2698 RVA: 0x00022C48 File Offset: 0x00020E48
		public IReadOnlyList<string> SelectedCheats
		{
			get
			{
				return this.selectedCheats;
			}
		}

		// Token: 0x06000A8B RID: 2699 RVA: 0x00022C50 File Offset: 0x00020E50
		public CheatManagerRestorableState() : base(null)
		{
		}

		// Token: 0x06000A8C RID: 2700 RVA: 0x00022C59 File Offset: 0x00020E59
		public CheatManagerRestorableState(CheatManager cheatManager) : base(cheatManager)
		{
		}

		// Token: 0x06000A8D RID: 2701 RVA: 0x00022C64 File Offset: 0x00020E64
		public override void Store(CheatManager cheatManager)
		{
			this.selectedCheats.Clear();
			this.selectedCheats.AddRange(from cheat in cheatManager.GetSelectedCheats()
			select cheat.ID);
		}

		// Token: 0x06000A8E RID: 2702 RVA: 0x00022CB1 File Offset: 0x00020EB1
		public override void Restore(CheatManager cheatManager, object args = null)
		{
			cheatManager.SetSelectedCheats(this.selectedCheats);
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x00022CBF File Offset: 0x00020EBF
		public CheatManagerRestorableState Clone()
		{
			return new CheatManagerRestorableState
			{
				selectedCheats = new List<string>(this.selectedCheats)
			};
		}

		// Token: 0x0400061E RID: 1566
		[SerializeField]
		private List<string> selectedCheats;
	}
}
