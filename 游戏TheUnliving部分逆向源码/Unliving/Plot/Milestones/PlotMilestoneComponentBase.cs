using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000310 RID: 784
	public abstract class PlotMilestoneComponentBase<T> : GameBehaviourBase where T : MilestoneDataBase
	{
		// Token: 0x06001A72 RID: 6770 RVA: 0x000527C2 File Offset: 0x000509C2
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (currentGame.Services.TryGet<PlotMilestoneManager>(out this.plotMilestoneManager))
			{
				this.InitializeMilestonesData();
				this.plotMilestoneManager.MilestoneReached += this.OnMilestoneReached;
				this.UpdateMilestonesState();
			}
		}

		// Token: 0x06001A73 RID: 6771 RVA: 0x00052804 File Offset: 0x00050A04
		private bool isDuringMilesoneItegration()
		{
			foreach (string str in new string[]
			{
				"1",
				"2",
				"3_betrayal",
				"3",
				"4"
			})
			{
				bool flag = this.plotMilestoneManager.IsMilestoneReached("m" + str + "_preIntegration");
				bool flag2 = this.plotMilestoneManager.IsMilestoneReached("m" + str + "_done");
				if (flag && !flag2)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001A74 RID: 6772
		protected abstract void InitializeMilestonesData();

		// Token: 0x06001A75 RID: 6773 RVA: 0x00052894 File Offset: 0x00050A94
		private void OnMilestoneReached(PlotMilestoneNode reachedMilestone)
		{
			if (this.haltOnIntegrations && this.isDuringMilesoneItegration())
			{
				return;
			}
			T t = default(T);
			string milestoneID = reachedMilestone.milestoneID;
			foreach (T t2 in this.milestones)
			{
				bool flag = this.beVerbose;
				if (!t2.milestoneID.Equals(milestoneID))
				{
					if (t2.alreadyActivated)
					{
						if (this.beVerbose)
						{
						}
					}
					else
					{
						if (this.beVerbose)
						{
							break;
						}
						break;
					}
				}
				else
				{
					t = t2;
					bool flag2 = this.beVerbose;
					if (!this.invokeOnlyTheLastReachedEvent)
					{
						Action activationEvent = t2.activationEvent;
						if (activationEvent != null)
						{
							activationEvent();
						}
						bool flag3 = this.beVerbose;
						t2.alreadyActivated = true;
					}
				}
			}
			if (this.invokeOnlyTheLastReachedEvent && t != null)
			{
				Action activationEvent2 = t.activationEvent;
				if (activationEvent2 != null)
				{
					activationEvent2();
				}
				t.alreadyActivated = true;
				bool flag4 = this.beVerbose;
			}
		}

		// Token: 0x06001A76 RID: 6774 RVA: 0x00052998 File Offset: 0x00050B98
		protected void UpdateMilestonesState()
		{
			if (this.haltOnIntegrations && this.isDuringMilesoneItegration())
			{
				return;
			}
			T t = default(T);
			foreach (T t2 in this.milestones)
			{
				if (!this.plotMilestoneManager.IsMilestoneReached(t2.milestoneID))
				{
					break;
				}
				if (!t2.alreadyActivated && !t2.oneTimeActivation)
				{
					if (!this.invokeOnlyTheLastReachedEvent)
					{
						Action activationEvent = t2.activationEvent;
						if (activationEvent != null)
						{
							activationEvent();
						}
						t2.alreadyActivated = true;
					}
					else
					{
						t = t2;
					}
				}
			}
			if (this.invokeOnlyTheLastReachedEvent && t != null)
			{
				Action activationEvent2 = t.activationEvent;
				if (activationEvent2 != null)
				{
					activationEvent2();
				}
				t.alreadyActivated = true;
			}
		}

		// Token: 0x06001A77 RID: 6775 RVA: 0x00052A70 File Offset: 0x00050C70
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
			foreach (T t in this.milestones)
			{
				if (!this.validator.IsMilestoneIDValid(t.milestoneID))
				{
					this.errorMessage = this.errorMessage + "Wrong milestone ID: '" + t.milestoneID + "'";
				}
			}
		}

		// Token: 0x06001A78 RID: 6776 RVA: 0x00052B08 File Offset: 0x00050D08
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.plotMilestoneManager.IsNull())
			{
				this.plotMilestoneManager.MilestoneReached -= this.OnMilestoneReached;
			}
		}

		// Token: 0x04000EB3 RID: 3763
		public string eventName;

		// Token: 0x04000EB4 RID: 3764
		[Tooltip("Normal behaviour: invoke all events whose milestones are reached, up to the first unreached")]
		public bool invokeOnlyTheLastReachedEvent;

		// Token: 0x04000EB5 RID: 3765
		[Tooltip("Enables step-by-step debug output for this component")]
		public bool beVerbose;

		// Token: 0x04000EB6 RID: 3766
		[Tooltip("Do not execute this logic, if we're currently in one of the integration stages ('mX_preIntegration' - 'mX_done')")]
		public bool haltOnIntegrations;

		// Token: 0x04000EB7 RID: 3767
		public T[] milestones;

		// Token: 0x04000EB8 RID: 3768
		private PlotMilestoneManager plotMilestoneManager;

		// Token: 0x04000EB9 RID: 3769
		private PlotMilestoneInEditorValidator validator;

		// Token: 0x04000EBA RID: 3770
		[HideInInspector]
		public string errorMessage;
	}
}
