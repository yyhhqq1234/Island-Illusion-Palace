using System;
using System.Linq;
using Common.UnityExtensions;
using Game.Factories;
using UnityEngine;
using UnityEngine.Events;
using Unliving.DropSystem;
using Unliving.Misc;
using Unliving.Mobs;
using Unliving.Pickables;
using Unliving.Player;
using Unliving.Purchasing;

namespace Unliving.Interactables
{
	// Token: 0x0200029A RID: 666
	public class MerchantNPCController : NPCControllerBase<RandomMessageObject.ID>
	{
		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x060016E6 RID: 5862 RVA: 0x000492A4 File Offset: 0x000474A4
		public override RandomMessageObject.ID ID
		{
			get
			{
				return this.objectID;
			}
		}

		// Token: 0x060016E7 RID: 5863 RVA: 0x000492AC File Offset: 0x000474AC
		protected override async void Start()
		{
			await new WaitForEndOfFrame();
			base.Start();
			IPlayerProvider playerProvider = base.CurrentGame.Services.Get<IPlayerProvider>();
			PlayerBehaviour playerBehaviour = (playerProvider != null) ? playerProvider.CurrentPlayer : null;
			this.playerGroup = (playerBehaviour.Group as PlayerMobsGroupController);
			base.TryGetComponent<PlayerMobsMovementBlocker>(out this.mobsMovementBlocker);
			this.messageManager = base.CurrentGame.Services.Get<RandomMessageManager>();
			this.enterTriggerCollider.onTriggerEnterEvents.AddListener(new UnityAction(this.OnTriggerEnter));
			this.exitTriggerCollider.onTriggerExitEvents.AddListener(new UnityAction(this.OnTriggerExit));
			for (int i = 0; i < this.storeItems.Length; i++)
			{
				this.storeItems[i].PickablePickedUp += this.OnStoreItemPickedUp;
			}
		}

		// Token: 0x060016E8 RID: 5864 RVA: 0x000492E5 File Offset: 0x000474E5
		private void OnStoreItemPickedUp(DropSpawner spawner, IPickableObject item)
		{
			this.storeItemBought = true;
			Action<RandomMessageObject.Message> messageFired = this.MessageFired;
			if (messageFired == null)
			{
				return;
			}
			messageFired(this.messageManager.GetMessage(this.objectID, RandomMessageObject.MessageType.buy_smth) as RandomMessageObject.Message);
		}

		// Token: 0x060016E9 RID: 5865 RVA: 0x00049315 File Offset: 0x00047515
		private void OnTriggerExit()
		{
			this.mobsMovementBlocker.SetActiveState(false);
			if (this.storeItemBought)
			{
				return;
			}
			Action<RandomMessageObject.Message> messageFired = this.MessageFired;
			if (messageFired == null)
			{
				return;
			}
			messageFired(this.messageManager.GetMessage(this.objectID, RandomMessageObject.MessageType.buy_nothing) as RandomMessageObject.Message);
		}

		// Token: 0x060016EA RID: 5866 RVA: 0x00049354 File Offset: 0x00047554
		private void OnTriggerEnter()
		{
			this.mobsMovementBlocker.SetActiveState(true);
			int num = 0;
			if (this.unwelcomeMobs.Any((MobBehaviour.ID id) => this.playerGroup.HasMobWithID(id)))
			{
				this.messageTypesBuffer[num++] = RandomMessageObject.MessageType.unwelcome_message;
			}
			if (this.playerGroup.Mobs.Count >= this.bigArmyCount)
			{
				this.messageTypesBuffer[num++] = RandomMessageObject.MessageType.big_army;
			}
			if (num == 0)
			{
				return;
			}
			RandomMessageObject.Message obj = this.messageManager.GetMaxPriorityMessage(this.objectID, this.messageTypesBuffer, num) as RandomMessageObject.Message;
			Action<RandomMessageObject.Message> messageFired = this.MessageFired;
			if (messageFired == null)
			{
				return;
			}
			messageFired(obj);
		}

		// Token: 0x060016EB RID: 5867 RVA: 0x000493F0 File Offset: 0x000475F0
		protected override void OnDestroy()
		{
			base.OnDestroy();
			for (int i = 0; i < this.storeItems.Length; i++)
			{
				if (!this.storeItems[i].IsNull())
				{
					this.storeItems[i].PickablePickedUp -= this.OnStoreItemPickedUp;
				}
			}
		}

		// Token: 0x04000D43 RID: 3395
		public Action<RandomMessageObject.Message> MessageFired;

		// Token: 0x04000D44 RID: 3396
		public RandomMessageObject.ID objectID;

		// Token: 0x04000D45 RID: 3397
		public int showMessageTimeout = 5;

		// Token: 0x04000D46 RID: 3398
		public int bigArmyCount = 2;

		// Token: 0x04000D47 RID: 3399
		[ObjectFactoryIDPopup(typeof(MobBehaviour))]
		public MobBehaviour.ID[] unwelcomeMobs;

		// Token: 0x04000D48 RID: 3400
		public IngameStoreObjectsSpawner[] storeItems;

		// Token: 0x04000D49 RID: 3401
		public CustomEventsTrigger enterTriggerCollider;

		// Token: 0x04000D4A RID: 3402
		public CustomEventsTrigger exitTriggerCollider;

		// Token: 0x04000D4B RID: 3403
		private RandomMessageManager messageManager;

		// Token: 0x04000D4C RID: 3404
		private readonly RandomMessageObject.MessageType[] messageTypesBuffer = new RandomMessageObject.MessageType[20];

		// Token: 0x04000D4D RID: 3405
		private bool storeItemBought;

		// Token: 0x04000D4E RID: 3406
		private PlayerMobsGroupController playerGroup;

		// Token: 0x04000D4F RID: 3407
		private PlayerMobsMovementBlocker mobsMovementBlocker;
	}
}
