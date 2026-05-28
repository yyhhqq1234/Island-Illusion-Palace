using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using Unliving.Plot.Milestones;

namespace Unliving.Cutscenes
{
	// Token: 0x02000322 RID: 802
	public class HomespaceMilestoneManager : GameBehaviourBase
	{
		// Token: 0x06001AE1 RID: 6881 RVA: 0x00054890 File Offset: 0x00052A90
		public void UpdateMilestones()
		{
			PlotMilestoneManager plotMilestoneManager;
			if (!base.CurrentGame.Services.TryGet<PlotMilestoneManager>(out plotMilestoneManager))
			{
				return;
			}
			foreach (StringTuple stringTuple in this.watchedMilestones)
			{
				if (stringTuple != null && !string.IsNullOrEmpty(stringTuple.watchedMilestoneID) && !string.IsNullOrEmpty(stringTuple.flagMilestoneID))
				{
					bool flag = plotMilestoneManager.IsMilestoneReached(stringTuple.watchedMilestoneID);
					bool flag2 = !plotMilestoneManager.IsMilestoneReached(stringTuple.flagMilestoneID);
					if (flag && flag2)
					{
						plotMilestoneManager.SetMilestoneReached(stringTuple.flagMilestoneID);
						if (this.doOneStep)
						{
							break;
						}
					}
				}
			}
		}

		// Token: 0x06001AE2 RID: 6882 RVA: 0x00054944 File Offset: 0x00052B44
		public void OnValidate()
		{
			if (this.validator == null)
			{
				this.validator = new PlotMilestoneInEditorValidator();
			}
			if (this.watchedMilestones == null || this.watchedMilestones.Count == 0)
			{
				return;
			}
			this.errorMessage = "";
			foreach (StringTuple stringTuple in this.watchedMilestones)
			{
				if (stringTuple != null)
				{
					if (string.IsNullOrEmpty(stringTuple.watchedMilestoneID) || string.IsNullOrEmpty(stringTuple.flagMilestoneID))
					{
						this.errorMessage = string.Concat(new string[]
						{
							this.errorMessage,
							"Empty milestone ID(s): '",
							stringTuple.watchedMilestoneID,
							"'-'",
							stringTuple.flagMilestoneID,
							"'"
						});
					}
					else
					{
						if (!this.validator.IsMilestoneIDValid(stringTuple.watchedMilestoneID))
						{
							this.errorMessage = this.errorMessage + "Wrong milestone ID: '" + stringTuple.watchedMilestoneID + "'";
						}
						if (!this.validator.IsMilestoneIDValid(stringTuple.flagMilestoneID))
						{
							this.errorMessage = this.errorMessage + "There's no milestone named: '" + stringTuple.flagMilestoneID + "'";
						}
					}
				}
			}
		}

		// Token: 0x04000F09 RID: 3849
		[SerializeField]
		[Tooltip("The manager will quit after first successful update – so you cannot slip through multiple stages in one go")]
		private bool doOneStep;

		// Token: 0x04000F0A RID: 3850
		[SerializeField]
		[Tooltip("Used in logging message")]
		public string location = "Homespace";

		// Token: 0x04000F0B RID: 3851
		[SerializeField]
		private List<StringTuple> watchedMilestones;

		// Token: 0x04000F0C RID: 3852
		private PlotMilestoneInEditorValidator validator;

		// Token: 0x04000F0D RID: 3853
		[HideInInspector]
		public string errorMessage;
	}
}
