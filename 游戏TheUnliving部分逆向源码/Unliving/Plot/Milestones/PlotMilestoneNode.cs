using System;
using Common;
using Common.Editor;
using Game.Core;
using GraphProcessor;
using UnityEngine;
using Unliving.PlayerProfileManagement;
using Unliving.Plot.TreeBasedCharacterPlot;
using Unliving.Plot.TreeBasedCharacterPlot.Triggers;
using Unliving.Plot.Triggers;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000317 RID: 791
	[NodeMenuItem("Plot/Milestone Trigger Node", null)]
	[Serializable]
	public class PlotMilestoneNode : BaseNode, ICloneable<PlotMilestoneNode>
	{
		// Token: 0x17000591 RID: 1425
		// (get) Token: 0x06001A97 RID: 6807 RVA: 0x00053211 File Offset: 0x00051411
		public override string name
		{
			get
			{
				return this.milestoneID;
			}
		}

		// Token: 0x17000592 RID: 1426
		// (get) Token: 0x06001A98 RID: 6808 RVA: 0x00053219 File Offset: 0x00051419
		public override string layoutStyle
		{
			get
			{
				return "PlotMilestoneNodesStyle";
			}
		}

		// Token: 0x06001A99 RID: 6809 RVA: 0x00053220 File Offset: 0x00051420
		public Sprite GetMilestoneIconSprite()
		{
			return this.iconSprite;
		}

		// Token: 0x06001A9A RID: 6810 RVA: 0x00053228 File Offset: 0x00051428
		public override void OnNodeCreated()
		{
			base.OnNodeCreated();
			if (string.IsNullOrEmpty(this.milestoneID))
			{
				this.milestoneID = this.GUID;
			}
		}

		// Token: 0x06001A9B RID: 6811 RVA: 0x0005324C File Offset: 0x0005144C
		public float GetMilestoneProgress(IGame game, out float currentValue, out float targetValue)
		{
			if (this.triggers == null || this.triggers.Length == 0)
			{
				currentValue = 0f;
				targetValue = 1f;
				return 0f;
			}
			CharacterPlotContext context = this.CreateCharacterPlotContext(game);
			currentValue = 0f;
			targetValue = 0f;
			CharacterPlotItemTriggerBase[] array = this.triggers;
			for (int i = 0; i < array.Length; i++)
			{
				float num;
				float num2;
				array[i].GetProgress(context, out num, out num2);
				currentValue += num;
				targetValue += num2;
			}
			return currentValue / targetValue;
		}

		// Token: 0x06001A9C RID: 6812 RVA: 0x000532CC File Offset: 0x000514CC
		public bool IsMilestoneReached(IGame game)
		{
			if (this.triggers == null || this.triggers.Length == 0)
			{
				return false;
			}
			CharacterPlotContext context = this.CreateCharacterPlotContext(game);
			for (int i = 0; i < this.triggers.Length; i++)
			{
				if (!this.IsTriggerFired(this.triggers[i], context))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001A9D RID: 6813 RVA: 0x0005331C File Offset: 0x0005151C
		private CharacterPlotContext CreateCharacterPlotContext(IGame game)
		{
			CharacterPlotContext characterPlotContext = new CharacterPlotContext
			{
				currentGame = game
			};
			PlayerProfileManager playerProfileManager;
			if (game.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				CharacterPlotContext characterPlotContext2 = characterPlotContext;
				PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				characterPlotContext2.totalPlotProgress = ((currentPlayerProfile != null) ? currentPlayerProfile.gamePlotProgress : null);
			}
			return characterPlotContext;
		}

		// Token: 0x06001A9E RID: 6814 RVA: 0x00053360 File Offset: 0x00051560
		private bool IsTriggerFired(CharacterPlotItemTriggerBase trigger, CharacterPlotContext context)
		{
			if (trigger == null)
			{
				Debug.LogError("Milestone " + this.milestoneID + " has null trigger in it.");
				return true;
			}
			PlotNodeStateTrigger plotNodeStateTrigger = trigger as PlotNodeStateTrigger;
			if (plotNodeStateTrigger != null)
			{
				CharacterPlotThreadNodeAsset nodeAsset = plotNodeStateTrigger.nodeAsset;
				string characterID = (nodeAsset != null) ? nodeAsset.Node.ID : null;
				context.characterID = characterID;
				TotalGamePlotProgressBase totalPlotProgress = context.totalPlotProgress;
				context.characterPlotProgress = ((totalPlotProgress != null) ? totalPlotProgress.GetCharacterPlotProgress(characterID) : null);
			}
			else
			{
				context.characterID = null;
				context.characterPlotProgress = null;
			}
			return trigger.IsFired(context);
		}

		// Token: 0x06001A9F RID: 6815 RVA: 0x000533E5 File Offset: 0x000515E5
		public PlotMilestoneNode Clone()
		{
			return (PlotMilestoneNode)base.MemberwiseClone();
		}

		// Token: 0x04000EC9 RID: 3785
		public string milestoneID;

		// Token: 0x04000ECA RID: 3786
		public bool achievementMilestone;

		// Token: 0x04000ECB RID: 3787
		public bool steamAchievement;

		// Token: 0x04000ECC RID: 3788
		public bool sendAnalyticsEvent;

		// Token: 0x04000ECD RID: 3789
		public Sprite iconSprite;

		// Token: 0x04000ECE RID: 3790
		[SerializeReference]
		[ManagedObjectField(typeof(CharacterPlotItemTriggerBase))]
		public CharacterPlotItemTriggerBase[] triggers;
	}
}
