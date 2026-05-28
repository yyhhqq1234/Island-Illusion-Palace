using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unliving.Player
{
	// Token: 0x02000150 RID: 336
	public sealed class PlayerActionsMask
	{
		// Token: 0x06000932 RID: 2354 RVA: 0x0001F38F File Offset: 0x0001D58F
		static PlayerActionsMask()
		{
			PlayerActionsMask.MaxActionID = Enum.GetValues(typeof(PlayerAction)).Cast<int>().Max() + 1;
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000933 RID: 2355 RVA: 0x0001F3BB File Offset: 0x0001D5BB
		// (set) Token: 0x06000934 RID: 2356 RVA: 0x0001F3C3 File Offset: 0x0001D5C3
		public int ActionsCount { get; private set; }

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000935 RID: 2357 RVA: 0x0001F3CC File Offset: 0x0001D5CC
		// (set) Token: 0x06000936 RID: 2358 RVA: 0x0001F3D4 File Offset: 0x0001D5D4
		public List<PlayerAction> CurrentActions { get; private set; }

		// Token: 0x06000937 RID: 2359 RVA: 0x0001F3DD File Offset: 0x0001D5DD
		public PlayerActionsMask(BitArray actionsMask)
		{
			this.ActionsMask = actionsMask;
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0001F3EC File Offset: 0x0001D5EC
		public PlayerActionsMask()
		{
			this.CurrentActions = new List<PlayerAction>(PlayerActionsMask.MaxActionID);
			this.ActionsMask = new BitArray(PlayerActionsMask.MaxActionID, false);
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0001F418 File Offset: 0x0001D618
		public PlayerActionsMask(IList<PlayerAction> actions) : this()
		{
			foreach (PlayerAction flag in actions)
			{
				this.AddActionFlag(flag);
			}
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x0001F468 File Offset: 0x0001D668
		public void ClearFlags()
		{
			this.ActionsMask.SetAll(false);
			this.ActionsCount = 0;
			this.CurrentActions.Clear();
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x0001F488 File Offset: 0x0001D688
		public void AddActionFlag(PlayerAction flag)
		{
			if (this.ActionsMask[(int)flag])
			{
				return;
			}
			this.ActionsMask[(int)flag] = true;
			int actionsCount = this.ActionsCount;
			this.ActionsCount = actionsCount + 1;
			this.CurrentActions.Add(flag);
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x0001F4D0 File Offset: 0x0001D6D0
		public void ResetActionFlag(PlayerAction flag)
		{
			if (!this.ActionsMask[(int)flag])
			{
				return;
			}
			this.ActionsMask[(int)flag] = false;
			int actionsCount = this.ActionsCount;
			this.ActionsCount = actionsCount - 1;
			this.CurrentActions.Remove(flag);
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x0001F518 File Offset: 0x0001D718
		public bool HasActionFlag(PlayerAction flag)
		{
			return this.ActionsMask[(int)flag];
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x0001F528 File Offset: 0x0001D728
		public override string ToString()
		{
			PlayerActionsMask.StringBuilder.Clear();
			PlayerActionsMask.StringBuilder.Append("{ ");
			int count = this.CurrentActions.Count;
			for (int i = 0; i < count; i++)
			{
				PlayerActionsMask.StringBuilder.Append(this.CurrentActions[i]);
				if (i < count - 1)
				{
					PlayerActionsMask.StringBuilder.Append(", ");
				}
			}
			PlayerActionsMask.StringBuilder.Append(" }");
			return PlayerActionsMask.StringBuilder.ToString();
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x0001F5B4 File Offset: 0x0001D7B4
		public static PlayerActionsMask operator &(PlayerActionsMask left, PlayerActionsMask right)
		{
			return new PlayerActionsMask(left.ActionsMask.And(right.ActionsMask));
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x0001F5CC File Offset: 0x0001D7CC
		public static PlayerActionsMask operator |(PlayerActionsMask left, PlayerActionsMask right)
		{
			return new PlayerActionsMask(left.ActionsMask.Or(right.ActionsMask));
		}

		// Token: 0x0400052B RID: 1323
		private static readonly int MaxActionID;

		// Token: 0x0400052C RID: 1324
		private static readonly StringBuilder StringBuilder = new StringBuilder();

		// Token: 0x0400052F RID: 1327
		public readonly BitArray ActionsMask;
	}
}
