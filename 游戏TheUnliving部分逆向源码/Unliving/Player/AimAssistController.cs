using System;
using Common;
using Common.Editor;
using Game.Core;
using Game.InputManager;
using UnityEngine;
using Unliving.GameSettings;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000135 RID: 309
	public class AimAssistController : BaseGameMob.ControllerBase<PlayerBehaviour>, IAimAssistController
	{
		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060007D5 RID: 2005 RVA: 0x00019CB5 File Offset: 0x00017EB5
		public IAimAssistMode CurrentMode
		{
			get
			{
				return this.aimAssistMode;
			}
		}

		// Token: 0x14000047 RID: 71
		// (add) Token: 0x060007D6 RID: 2006 RVA: 0x00019CC0 File Offset: 0x00017EC0
		// (remove) Token: 0x060007D7 RID: 2007 RVA: 0x00019CF8 File Offset: 0x00017EF8
		public event Action AimAssistModeChanged;

		// Token: 0x060007D8 RID: 2008 RVA: 0x00019D30 File Offset: 0x00017F30
		public AimAssistController(PlayerBehaviour targetMob) : base(targetMob)
		{
			IGame currentGame = targetMob.CurrentGame;
			currentGame.BindDataDirectly(ref this.data);
			if (currentGame.Services.TryGet<IInputManager>(out this.inputManager))
			{
				this.inputManager.ButtonClicked += this.OnButtonClicked;
			}
			if (currentGame.Services.TryGet<IGameSettingsManager>(out this.gameSettingsManager))
			{
				this.gameSettingsManager.CurrentSettingsStateChanged += this.OnCurrentSettingsStateChanged;
				this.UpdateAimAssistMode();
			}
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x00019DB0 File Offset: 0x00017FB0
		private void OnButtonClicked(int actionID)
		{
			if (actionID == (int)this.data.switchAimAssistModeAction)
			{
				this.SwitchAimAssistType();
			}
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x00019DC6 File Offset: 0x00017FC6
		public void OnLateUpdate()
		{
			IAimAssistMode aimAssistMode = this.aimAssistMode;
			if (aimAssistMode == null)
			{
				return;
			}
			aimAssistMode.OnUpdate();
		}

		// Token: 0x060007DB RID: 2011 RVA: 0x00019DD8 File Offset: 0x00017FD8
		private void OnCurrentSettingsStateChanged(GameSettingsState obj)
		{
			this.UpdateAimAssistMode();
		}

		// Token: 0x060007DC RID: 2012 RVA: 0x00019DE0 File Offset: 0x00017FE0
		private void UpdateAimAssistMode()
		{
			AimAssistType aimAssistType = (AimAssistType)this.gameSettingsManager.CurrentState.inputData.aimAssistType;
			IAimAssistMode aimAssistMode = this.aimAssistMode;
			if (aimAssistMode != null && aimAssistMode.AimAssistType == aimAssistType)
			{
				return;
			}
			IAimAssistMode aimAssistMode2 = this.aimAssistMode;
			if (aimAssistMode2 != null)
			{
				aimAssistMode2.Destroy();
			}
			this.aimAssistMode = null;
			if (aimAssistType != AimAssistType.Disabled)
			{
				this.aimAssistMode = (IAimAssistMode)aimAssistType.CreateInstanceFromEnum(new object[]
				{
					this.ControllerOwner
				});
				for (int i = 0; i < this.data.modesData.Length; i++)
				{
					AimAssistModeBase.IData data = this.data.modesData[i];
					if (data.AimAssistType == aimAssistType)
					{
						this.aimAssistMode.SetData(data);
						break;
					}
				}
			}
			Action aimAssistModeChanged = this.AimAssistModeChanged;
			if (aimAssistModeChanged == null)
			{
				return;
			}
			aimAssistModeChanged();
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00019EA8 File Offset: 0x000180A8
		private void SwitchAimAssistType()
		{
			IGameSettingsManager gameSettingsManager = this.gameSettingsManager;
			if (gameSettingsManager == null)
			{
				return;
			}
			gameSettingsManager.SwitchAimAssistType(true);
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x00019EBC File Offset: 0x000180BC
		protected override void OnOwnerKilled(IGameMob owner)
		{
			base.OnOwnerKilled(owner);
			if (this.gameSettingsManager != null)
			{
				this.gameSettingsManager.CurrentSettingsStateChanged -= this.OnCurrentSettingsStateChanged;
			}
			if (this.inputManager != null)
			{
				this.inputManager.ButtonClicked -= this.OnButtonClicked;
			}
		}

		// Token: 0x0400047F RID: 1151
		private IGameSettingsManager gameSettingsManager;

		// Token: 0x04000480 RID: 1152
		private IInputManager inputManager;

		// Token: 0x04000481 RID: 1153
		private IAimAssistMode aimAssistMode;

		// Token: 0x04000482 RID: 1154
		private AimAssistController.Data data;

		// Token: 0x02000443 RID: 1091
		[Serializable]
		public struct Data
		{
			// Token: 0x04001698 RID: 5784
			public PlayerAction switchAimAssistModeAction;

			// Token: 0x04001699 RID: 5785
			[SerializeReference]
			[ManagedObjectField(typeof(AimAssistModeBase.IData))]
			public AimAssistModeBase.IData[] modesData;
		}
	}
}
