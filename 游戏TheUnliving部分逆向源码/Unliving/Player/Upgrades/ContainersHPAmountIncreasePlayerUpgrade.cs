using System;
using System.Collections;
using Game.Core;
using Game.Damage;
using UnityEngine;
using Unliving.LeveledItems;

namespace Unliving.Player.Upgrades
{
	// Token: 0x0200016A RID: 362
	[CreateAssetMenu(fileName = "ContainersHPAmountIncreasePlayerUpgrade", menuName = "Game/Player/Player Upgrade/Containers HP Amount Increase Upgrade")]
	public class ContainersHPAmountIncreasePlayerUpgrade : ScriptableObject, IPlayerUpgrade, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000A02 RID: 2562 RVA: 0x00021E67 File Offset: 0x00020067
		// (set) Token: 0x06000A03 RID: 2563 RVA: 0x00021E6F File Offset: 0x0002006F
		public IPlayerUpgrade UpgradePrototype { get; private set; }

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000A04 RID: 2564 RVA: 0x00021E78 File Offset: 0x00020078
		// (set) Token: 0x06000A05 RID: 2565 RVA: 0x00021E7B File Offset: 0x0002007B
		public int ItemLevel
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000A06 RID: 2566 RVA: 0x00021E7D File Offset: 0x0002007D
		// (set) Token: 0x06000A07 RID: 2567 RVA: 0x00021E85 File Offset: 0x00020085
		public bool IsActivated { get; private set; }

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000A08 RID: 2568 RVA: 0x00021E8E File Offset: 0x0002008E
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x00021E91 File Offset: 0x00020091
		public IEnumerator Activate(IPlayerUpgradesRegistry upgradesRegistry, object activationContext)
		{
			if (this.IsActivated)
			{
				yield break;
			}
			this.IsActivated = true;
			yield return null;
			this.currentGame = (activationContext as IGame);
			GameManager gameManager;
			if (this.currentGame.Services.TryGet<GameManager>(out gameManager) && !gameManager.IsStartSceneLoaded)
			{
				yield break;
			}
			IPlayerProvider playerProvider;
			if (this.currentGame != null && this.currentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				PlayerBehaviour currentPlayer = playerProvider.CurrentPlayer;
				IDamageable damageable = (currentPlayer != null) ? currentPlayer.HitPointsController : null;
				if (damageable != null)
				{
					damageable.InitialHitPoints += (float)this.additionalHPAmount;
				}
			}
			yield break;
		}

		// Token: 0x06000A0A RID: 2570 RVA: 0x00021EA7 File Offset: 0x000200A7
		public IPlayerUpgrade Clone()
		{
			ContainersHPAmountIncreasePlayerUpgrade containersHPAmountIncreasePlayerUpgrade = (ContainersHPAmountIncreasePlayerUpgrade)base.MemberwiseClone();
			containersHPAmountIncreasePlayerUpgrade.UpgradePrototype = (this.UpgradePrototype ?? this);
			return containersHPAmountIncreasePlayerUpgrade;
		}

		// Token: 0x040005EE RID: 1518
		public int additionalHPAmount = 5;

		// Token: 0x040005EF RID: 1519
		private IGame currentGame;
	}
}
