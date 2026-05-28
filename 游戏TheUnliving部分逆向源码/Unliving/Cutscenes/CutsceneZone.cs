using System;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Player;

namespace Unliving.Cutscenes
{
	// Token: 0x02000321 RID: 801
	public class CutsceneZone : CutsceneBase
	{
		// Token: 0x06001ADC RID: 6876 RVA: 0x000547E0 File Offset: 0x000529E0
		private void OnTriggerEnter2D(Collider2D collider)
		{
			UnityEngine.Object gameObject = collider.gameObject;
			PlayerBehaviour currentPlayer = this.currentPlayer;
			if (gameObject == ((currentPlayer != null) ? currentPlayer.gameObject : null))
			{
				this.havePlayerInside = true;
				base.StartInteractiveScene();
			}
		}

		// Token: 0x06001ADD RID: 6877 RVA: 0x0005480E File Offset: 0x00052A0E
		private void OnTriggerExit2D(Collider2D collider)
		{
			UnityEngine.Object gameObject = collider.gameObject;
			PlayerBehaviour currentPlayer = this.currentPlayer;
			if (gameObject == ((currentPlayer != null) ? currentPlayer.gameObject : null))
			{
				this.havePlayerInside = false;
				UnityEvent onInteractiveZoneExitEvent = this.OnInteractiveZoneExitEvent;
				if (onInteractiveZoneExitEvent == null)
				{
					return;
				}
				onInteractiveZoneExitEvent.Invoke();
			}
		}

		// Token: 0x06001ADE RID: 6878 RVA: 0x00054846 File Offset: 0x00052A46
		public void TriggerExitEventManually(bool forceDisableAfterPlayedOnce)
		{
			if (forceDisableAfterPlayedOnce)
			{
				this.disableAfterPlayedOnce = true;
			}
			UnityEvent onInteractiveZoneExitEvent = this.OnInteractiveZoneExitEvent;
			if (onInteractiveZoneExitEvent == null)
			{
				return;
			}
			onInteractiveZoneExitEvent.Invoke();
		}

		// Token: 0x06001ADF RID: 6879 RVA: 0x00054864 File Offset: 0x00052A64
		public override void Start()
		{
			Collider2D collider2D;
			if (base.TryGetComponent<Collider2D>(out collider2D))
			{
				collider2D.isTrigger = true;
			}
			base.Start();
		}

		// Token: 0x04000F07 RID: 3847
		public UnityEvent OnInteractiveZoneExitEvent;

		// Token: 0x04000F08 RID: 3848
		[HideInInspector]
		public bool havePlayerInside;
	}
}
