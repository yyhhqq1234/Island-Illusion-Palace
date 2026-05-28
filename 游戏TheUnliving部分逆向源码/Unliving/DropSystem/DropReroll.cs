using System;
using System.Collections;
using Common.UnityExtensions;
using Game.Damage;
using Game.Localization;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Pickables;
using Unliving.PlayerProfileManagement;

namespace Unliving.DropSystem
{
	// Token: 0x02000289 RID: 649
	public class DropReroll : PickableBase
	{
		// Token: 0x170004D1 RID: 1233
		// (get) Token: 0x06001666 RID: 5734 RVA: 0x00047E94 File Offset: 0x00046094
		public override PickableObjectData PickableObjectData
		{
			get
			{
				if (this.pickableObjectData == null)
				{
					this.pickableObjectData = new PickableObjectData
					{
						metadata = this.localizationManager.GetMetadata(this.LocalizationID, new string[]
						{
							this.CurrentCost.ToString()
						})
					};
				}
				return this.pickableObjectData;
			}
		}

		// Token: 0x170004D2 RID: 1234
		// (get) Token: 0x06001667 RID: 5735 RVA: 0x00047EEA File Offset: 0x000460EA
		public int CurrentCost
		{
			get
			{
				return this.costLevels[this.currentCostLevel];
			}
		}

		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x06001668 RID: 5736 RVA: 0x00047EFC File Offset: 0x000460FC
		protected override string LocalizationID
		{
			get
			{
				switch (this.resourceType)
				{
				case DropReroll.ResourceType.HP:
					return "drop_reroll_hp";
				case DropReroll.ResourceType.GOLD:
					return "drop_reroll_gold";
				case DropReroll.ResourceType.HEALTH_CONTAINER:
					return "drop_reroll_health_containers";
				default:
					return string.Empty;
				}
			}
		}

		// Token: 0x06001669 RID: 5737 RVA: 0x00047F3B File Offset: 0x0004613B
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x0600166A RID: 5738 RVA: 0x00047F40 File Offset: 0x00046140
		private void Start()
		{
			this.localizationManager = base.CurrentGame.Services.Get<LocalizationManager>();
			this.hpController = base.GetComponentInChildren<IDamageable>();
			this.hpController.TotallyDestroyed += this.OnTotallyDestroyed;
			this.hpController.HitPointsChanged += this.OnHitPointsChanged;
			this.playerHPController = (base.CurrentPlayer.HitPointsController as IContainerBasedHPController);
			this.playerProfile = base.CurrentGame.Services.Get<PlayerProfileManager>().CurrentPlayerProfile;
			foreach (DropSpawner dropSpawner in this.dropSpawners)
			{
				if (!dropSpawner.IsNull())
				{
					dropSpawner.PickablePickedUp += this.OnSpawnerPickablePickedUp;
					dropSpawner.PickableDestroyed += this.OnSpawnerPickableDestroyed;
				}
			}
		}

		// Token: 0x0600166B RID: 5739 RVA: 0x00048013 File Offset: 0x00046213
		private void LateUpdate()
		{
			if (this.checkIsAlive)
			{
				this.checkIsAlive = false;
				base.StopAllCoroutines();
				base.StartCoroutine(this.DestroyObjectAfter3Frames());
			}
		}

		// Token: 0x0600166C RID: 5740 RVA: 0x00048037 File Offset: 0x00046237
		private IEnumerator DestroyObjectAfter3Frames()
		{
			yield return null;
			yield return null;
			yield return null;
			if (!this.IsAlive())
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			yield break;
		}

		// Token: 0x0600166D RID: 5741 RVA: 0x00048046 File Offset: 0x00046246
		private void OnSpawnerPickableDestroyed(DropSpawner obj)
		{
			this.checkIsAlive = true;
		}

		// Token: 0x0600166E RID: 5742 RVA: 0x0004804F File Offset: 0x0004624F
		private void OnSpawnerPickablePickedUp(DropSpawner spawner, IPickableObject pickable)
		{
			this.checkIsAlive = true;
		}

		// Token: 0x0600166F RID: 5743 RVA: 0x00048058 File Offset: 0x00046258
		private void OnHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
		}

		// Token: 0x06001670 RID: 5744 RVA: 0x0004805A File Offset: 0x0004625A
		private void OnTotallyDestroyed(IDamageable target)
		{
			this.PerformPickingUp(base.CurrentPlayer.PickableObjectsController);
		}

		// Token: 0x06001671 RID: 5745 RVA: 0x00048070 File Offset: 0x00046270
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			if (!this.CanPay())
			{
				return;
			}
			switch (this.resourceType)
			{
			case DropReroll.ResourceType.HP:
			{
				HitPointsController.HPChangingArgs args = new HitPointsController.HPChangingArgs(true)
				{
					isForcedChanging = false,
					amount = (float)this.CurrentCost
				};
				this.playerHPController.ModifyHitPoints(this, args);
				break;
			}
			case DropReroll.ResourceType.GOLD:
				this.currencyOperationArgs.amount = (float)(-(float)this.CurrentCost);
				this.playerProfile.TryExecuteCurrencyOperation(this.currencyOperationArgs);
				break;
			case DropReroll.ResourceType.HEALTH_CONTAINER:
				for (int i = 0; i < this.CurrentCost; i++)
				{
					IEnergyContainer currentHealthContainer = this.playerHPController.CurrentHealthContainer;
					if (currentHealthContainer == null)
					{
						break;
					}
					HitPointsController.HPChangingArgs args = new HitPointsController.HPChangingArgs(true)
					{
						isForcedChanging = true,
						amount = currentHealthContainer.CurrentEnergyAmount
					};
					this.playerHPController.ModifyHitPoints(this, args);
				}
				break;
			}
			DropSpawner[] array = this.dropSpawners;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].RespawnPickable();
			}
			this.currentCostLevel++;
			this.currentCostLevel = Mathf.Clamp(this.currentCostLevel, 0, this.costLevels.Length - 1);
			this.checkIsAlive = true;
		}

		// Token: 0x06001672 RID: 5746 RVA: 0x0004819C File Offset: 0x0004639C
		private bool IsAlive()
		{
			if (!this.hpController.IsAlive)
			{
				return false;
			}
			DropSpawner[] array = this.dropSpawners;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].SpawnedPickable.IsNull())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001673 RID: 5747 RVA: 0x000481DF File Offset: 0x000463DF
		private bool CanPay()
		{
			if (this.resourceType == DropReroll.ResourceType.GOLD)
			{
				return this.playerProfile.GetCurrencyAmount(CurrencyID.Gold) >= this.CurrentCost;
			}
			return this.resourceType == DropReroll.ResourceType.HEALTH_CONTAINER || this.resourceType == DropReroll.ResourceType.HP;
		}

		// Token: 0x06001674 RID: 5748 RVA: 0x00048218 File Offset: 0x00046418
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.hpController.IsNull())
			{
				this.hpController.TotallyDestroyed -= this.OnTotallyDestroyed;
			}
			foreach (DropSpawner dropSpawner in this.dropSpawners)
			{
				if (!dropSpawner.IsNull())
				{
					dropSpawner.PickablePickedUp -= this.OnSpawnerPickablePickedUp;
				}
			}
		}

		// Token: 0x04000D01 RID: 3329
		private const string RESOURCE_ID_HP = "drop_reroll_hp";

		// Token: 0x04000D02 RID: 3330
		private const string RESOURCE_ID_GOLD = "drop_reroll_gold";

		// Token: 0x04000D03 RID: 3331
		private const string RESOURCE_ID_CONTAINER = "drop_reroll_health_containers";

		// Token: 0x04000D04 RID: 3332
		private CurrencyOperationArgs currencyOperationArgs = new CurrencyOperationArgs
		{
			currencyID = CurrencyID.Gold
		};

		// Token: 0x04000D05 RID: 3333
		public DropReroll.ResourceType resourceType;

		// Token: 0x04000D06 RID: 3334
		public DropSpawner[] dropSpawners;

		// Token: 0x04000D07 RID: 3335
		public int[] costLevels;

		// Token: 0x04000D08 RID: 3336
		private int currentCostLevel;

		// Token: 0x04000D09 RID: 3337
		private LocalizationManager localizationManager;

		// Token: 0x04000D0A RID: 3338
		private IDamageable hpController;

		// Token: 0x04000D0B RID: 3339
		private IContainerBasedHPController playerHPController;

		// Token: 0x04000D0C RID: 3340
		private PlayerProfile playerProfile;

		// Token: 0x04000D0D RID: 3341
		private bool checkIsAlive;

		// Token: 0x0200050C RID: 1292
		public enum ResourceType
		{
			// Token: 0x04001AE6 RID: 6886
			HP,
			// Token: 0x04001AE7 RID: 6887
			GOLD,
			// Token: 0x04001AE8 RID: 6888
			HEALTH_CONTAINER
		}
	}
}
