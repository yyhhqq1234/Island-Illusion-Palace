using System;
using Game.Core;
using UltEvents;
using UnityEngine;
using Unliving.Cutscenes;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000318 RID: 792
	public class PlotMilestoneSwitch : GameBehaviourBase
	{
		// Token: 0x06001AA1 RID: 6817 RVA: 0x000533FC File Offset: 0x000515FC
		public void Start()
		{
			if (this.gameStateUpdateLogic && this.updateGameStateBeforeMilestoneEvents)
			{
				this.gameStateUpdateLogic.UpdateMilestones();
			}
			PlotMilestoneManager plotMilestoneManager;
			if (!base.CurrentGame.Services.TryGet<PlotMilestoneManager>(out plotMilestoneManager))
			{
				return;
			}
			MilestoneEventsData milestoneEventsData = null;
			foreach (MilestoneEventsData milestoneEventsData2 in this.milestones)
			{
				if (!plotMilestoneManager.IsMilestoneReached(milestoneEventsData2.milestoneID))
				{
					break;
				}
				milestoneEventsData = milestoneEventsData2;
			}
			if (milestoneEventsData != null)
			{
				UltEvent milestoneEvents = milestoneEventsData.milestoneEvents;
				if (milestoneEvents != null)
				{
					milestoneEvents.Invoke();
				}
			}
			if (this.gameStateUpdateLogic && !this.updateGameStateBeforeMilestoneEvents)
			{
				this.gameStateUpdateLogic.UpdateMilestones();
			}
		}

		// Token: 0x06001AA2 RID: 6818 RVA: 0x000534A0 File Offset: 0x000516A0
		public void OnValidate()
		{
			if (this.validator == null)
			{
				this.validator = new PlotMilestoneInEditorValidator();
			}
			if (this.milestones == null || this.milestones.Length == 0)
			{
				return;
			}
			this.errorMessage = "";
			foreach (MilestoneEventsData milestoneEventsData in this.milestones)
			{
				if (!this.validator.IsMilestoneIDValid(milestoneEventsData.milestoneID))
				{
					this.errorMessage = this.errorMessage + "Wrong milestone ID: '" + milestoneEventsData.milestoneID + "'";
				}
			}
		}

		// Token: 0x04000ECF RID: 3791
		[SerializeField]
		[Tooltip("Linked MilestoneManager will be called before addressing the rest of the milestone-linked events")]
		protected HomespaceMilestoneManager gameStateUpdateLogic;

		// Token: 0x04000ED0 RID: 3792
		[SerializeField]
		protected bool updateGameStateBeforeMilestoneEvents = true;

		// Token: 0x04000ED1 RID: 3793
		[SerializeField]
		protected MilestoneEventsData[] milestones;

		// Token: 0x04000ED2 RID: 3794
		private PlotMilestoneInEditorValidator validator;

		// Token: 0x04000ED3 RID: 3795
		[HideInInspector]
		public string errorMessage;
	}
}
