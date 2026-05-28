using System;
using Common;
using Unliving.Player;

namespace Unliving.Pickables
{
	// Token: 0x0200019D RID: 413
	public abstract class PickingSettingsBase : IPickingSettings, ICloneable<IPickingSettings>
	{
		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000BD3 RID: 3027
		public abstract float CurrentProgress { get; }

		// Token: 0x06000BD4 RID: 3028
		public abstract float UpdateCurrentPickingProgress(PlayerInputController.ActionArgs inputArgs);

		// Token: 0x06000BD5 RID: 3029
		public abstract void ResetCurrentPickingProgress();

		// Token: 0x06000BD6 RID: 3030 RVA: 0x000258AE File Offset: 0x00023AAE
		protected bool HasPickingAction(PlayerInputController.ActionArgs inputArgs)
		{
			return inputArgs.HasActionFlag(PlayerAction.PLAYER_COLLECT_OBJECT);
		}

		// Token: 0x06000BD7 RID: 3031 RVA: 0x000258B8 File Offset: 0x00023AB8
		protected bool HasPickingInterruptAction(PlayerInputController.ActionArgs inputArgs)
		{
			return inputArgs.HasActionFlag(PlayerAction.PLAYER_COLLECT_OBJECT_INTERRUPT);
		}

		// Token: 0x06000BD8 RID: 3032 RVA: 0x000258C2 File Offset: 0x00023AC2
		public IPickingSettings Clone()
		{
			return (IPickingSettings)base.MemberwiseClone();
		}

		// Token: 0x04000698 RID: 1688
		private const PlayerAction PICKING_ACTION = PlayerAction.PLAYER_COLLECT_OBJECT;

		// Token: 0x04000699 RID: 1689
		private const PlayerAction PICKING_INTERRUPT_ACTION = PlayerAction.PLAYER_COLLECT_OBJECT_INTERRUPT;
	}
}
