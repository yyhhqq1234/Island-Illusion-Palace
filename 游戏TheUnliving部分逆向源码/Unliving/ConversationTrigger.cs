using System;
using UnityEngine;
using Unliving.Interactables;

namespace Unliving
{
	// Token: 0x0200000A RID: 10
	public class ConversationTrigger : MonoBehaviour
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00002895 File Offset: 0x00000A95
		public void StartConversation()
		{
			this.npcController = base.GetComponent<NPCController>();
			if (this.npcController != null)
			{
				this.npcController.StartConversation();
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000028BD File Offset: 0x00000ABD
		public void SetPause(bool on)
		{
			Time.timeScale = (float)(on ? 0 : 1);
		}

		// Token: 0x04000014 RID: 20
		private NPCController npcController;
	}
}
