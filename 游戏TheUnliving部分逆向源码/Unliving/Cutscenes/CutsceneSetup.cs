using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Unliving.Cutscenes
{
	// Token: 0x02000320 RID: 800
	[DefaultExecutionOrder(-1)]
	public class CutsceneSetup : GameBehaviourBase
	{
		// Token: 0x06001AD8 RID: 6872 RVA: 0x00054654 File Offset: 0x00052854
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			UnityEvent unityEvent = this.onInitializeEvent;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			foreach (GameObject gameObject in this.objectsToEnable)
			{
				if (!gameObject.IsNull())
				{
					gameObject.SetActive(true);
				}
			}
			foreach (GameObject gameObject2 in this.objectsToDisable)
			{
				if (!gameObject2.IsNull())
				{
					gameObject2.SetActive(false);
				}
			}
		}

		// Token: 0x06001AD9 RID: 6873 RVA: 0x00054714 File Offset: 0x00052914
		public void SaveSelectedObjectsState()
		{
			if (this.gameObjectToSaveState == null)
			{
				this.objectsToEnable = new List<GameObject>();
				this.objectsToDisable = new List<GameObject>();
				this.SaveChildrenState(base.transform);
				return;
			}
			this.SaveChildrenState(this.gameObjectToSaveState.transform);
		}

		// Token: 0x06001ADA RID: 6874 RVA: 0x00054764 File Offset: 0x00052964
		private void SaveChildrenState(Transform tr)
		{
			foreach (object obj in tr)
			{
				GameObject gameObject = ((Transform)obj).gameObject;
				if (gameObject.activeSelf)
				{
					this.objectsToEnable.Add(gameObject);
				}
				else
				{
					this.objectsToDisable.Add(gameObject);
				}
			}
		}

		// Token: 0x04000F03 RID: 3843
		public List<GameObject> objectsToEnable;

		// Token: 0x04000F04 RID: 3844
		public List<GameObject> objectsToDisable;

		// Token: 0x04000F05 RID: 3845
		[Space]
		public UnityEvent onInitializeEvent;

		// Token: 0x04000F06 RID: 3846
		[Space]
		[Tooltip("Put in a GameObject in this field and press the button to save Active/Inactive status of its children")]
		public GameObject gameObjectToSaveState;
	}
}
