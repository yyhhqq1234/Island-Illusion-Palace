using System;
using System.Collections;
using Game.Core;
using Game.Damage;
using UnityEngine;
using Unliving.LeveledItems;

namespace Unliving.Player.Upgrades
{
	// Token: 0x0200016B RID: 363
	[CreateAssetMenu(fileName = "DamageResistanceModificationPlayerUpgrade", menuName = "Game/Player/Player Upgrade/Damage Resistance Modification Upgrade")]
	public class DamageResistanceModificationPlayerUpgrade : ScriptableObject, IPlayerUpgrade, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000A0C RID: 2572 RVA: 0x00021ED4 File Offset: 0x000200D4
		// (set) Token: 0x06000A0D RID: 2573 RVA: 0x00021EDC File Offset: 0x000200DC
		public IPlayerUpgrade UpgradePrototype { get; private set; }

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000A0E RID: 2574 RVA: 0x00021EE5 File Offset: 0x000200E5
		// (set) Token: 0x06000A0F RID: 2575 RVA: 0x00021EED File Offset: 0x000200ED
		public int ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
			set
			{
			}
		}

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000A10 RID: 2576 RVA: 0x00021EEF File Offset: 0x000200EF
		// (set) Token: 0x06000A11 RID: 2577 RVA: 0x00021EF7 File Offset: 0x000200F7
		public bool IsActivated { get; private set; }

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000A12 RID: 2578 RVA: 0x00021F00 File Offset: 0x00020100
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
		}

		// Token: 0x06000A13 RID: 2579 RVA: 0x00021F08 File Offset: 0x00020108
		public IEnumerator Activate(IPlayerUpgradesRegistry upgradesRegistry, object activationContext)
		{
			if (this.IsActivated)
			{
				yield break;
			}
			this.IsActivated = true;
			yield return null;
			this.currentGame = (activationContext as IGame);
			IPlayerProvider playerProvider;
			if (this.currentGame != null && this.currentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				PlayerBehaviour currentPlayer = playerProvider.CurrentPlayer;
				IContainerBasedHPController containerBasedHPController = ((currentPlayer != null) ? currentPlayer.HitPointsController : null) as IContainerBasedHPController;
				if (containerBasedHPController != null)
				{
					containerBasedHPController.DamageResistanceTimeoutMultiplier = this.damageResistanceTimeoutMultiplier;
				}
			}
			yield break;
		}

		// Token: 0x06000A14 RID: 2580 RVA: 0x00021F1E File Offset: 0x0002011E
		public IPlayerUpgrade Clone()
		{
			DamageResistanceModificationPlayerUpgrade damageResistanceModificationPlayerUpgrade = (DamageResistanceModificationPlayerUpgrade)base.MemberwiseClone();
			damageResistanceModificationPlayerUpgrade.UpgradePrototype = (this.UpgradePrototype ?? this);
			return damageResistanceModificationPlayerUpgrade;
		}

		// Token: 0x040005F2 RID: 1522
		public float damageResistanceTimeoutMultiplier = 1f;

		// Token: 0x040005F3 RID: 1523
		private IGame currentGame;

		// Token: 0x040005F4 RID: 1524
		private int itemLevel;
	}
}
