using System;
using Game.Core;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Player;

namespace Unliving.Cutscenes
{
	// Token: 0x02000324 RID: 804
	public class InteractiveZone : GameBehaviourBase
	{
		// Token: 0x06001AE5 RID: 6885 RVA: 0x00054AB3 File Offset: 0x00052CB3
		private void InteractiveZoneStarted()
		{
		}

		// Token: 0x06001AE6 RID: 6886 RVA: 0x00054AB5 File Offset: 0x00052CB5
		private void InteractiveZoneEnded()
		{
			if (this.disableAfterPlayedOnce)
			{
				base.gameObject.SetActive(false);
			}
		}

		// Token: 0x06001AE7 RID: 6887 RVA: 0x00054ACB File Offset: 0x00052CCB
		private void OnTriggerEnter2D(Collider2D collider)
		{
			this.InteractiveZoneStarted();
			UnityEngine.Object gameObject = collider.gameObject;
			PlayerBehaviour playerBehaviour = this.currentPlayer;
			if (gameObject == ((playerBehaviour != null) ? playerBehaviour.gameObject : null))
			{
				UnityEvent onZoneEnterEvent = this.OnZoneEnterEvent;
				if (onZoneEnterEvent == null)
				{
					return;
				}
				onZoneEnterEvent.Invoke();
			}
		}

		// Token: 0x06001AE8 RID: 6888 RVA: 0x00054B02 File Offset: 0x00052D02
		private void OnTriggerExit2D(Collider2D collider)
		{
			this.InteractiveZoneEnded();
			UnityEngine.Object gameObject = collider.gameObject;
			PlayerBehaviour playerBehaviour = this.currentPlayer;
			if (gameObject == ((playerBehaviour != null) ? playerBehaviour.gameObject : null))
			{
				UnityEvent onZoneExitEvent = this.OnZoneExitEvent;
				if (onZoneExitEvent == null)
				{
					return;
				}
				onZoneExitEvent.Invoke();
			}
		}

		// Token: 0x06001AE9 RID: 6889 RVA: 0x00054B3C File Offset: 0x00052D3C
		public void Start()
		{
			base.CurrentGame.TryGetPlayer(out this.currentPlayer);
			Collider2D collider2D;
			if (base.TryGetComponent<Collider2D>(out collider2D))
			{
				collider2D.isTrigger = true;
			}
		}

		// Token: 0x04000F10 RID: 3856
		public bool disableAfterPlayedOnce = true;

		// Token: 0x04000F11 RID: 3857
		[Space]
		public UnityEvent OnZoneEnterEvent;

		// Token: 0x04000F12 RID: 3858
		public UnityEvent OnZoneExitEvent;

		// Token: 0x04000F13 RID: 3859
		protected PlayerBehaviour currentPlayer;
	}
}
