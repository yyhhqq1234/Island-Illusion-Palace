using System;
using Game.LevelGeneration;
using UnityEngine;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000264 RID: 612
	public sealed class ChildChunkGatewayLocker : MonoBehaviour
	{
		// Token: 0x06001458 RID: 5208 RVA: 0x00040267 File Offset: 0x0003E467
		private void UpdateLockState(bool newLockState)
		{
			if (this.currentGateway == null)
			{
				return;
			}
			this.currentGateway.IsLocked = newLockState;
		}

		// Token: 0x06001459 RID: 5209 RVA: 0x0004027E File Offset: 0x0003E47E
		private void OnConnectedGatewayLockStateChanged(bool isLocked)
		{
			this.UpdateLockState(isLocked);
		}

		// Token: 0x0600145A RID: 5210 RVA: 0x00040288 File Offset: 0x0003E488
		private void Start()
		{
			LocationGate locationGate;
			if (!base.TryGetComponent<ILocationChunkGateway>(out this.currentGateway) && base.TryGetComponent<LocationGate>(out locationGate))
			{
				this.currentGateway = locationGate.CurrentChunkGateway;
			}
			ILocationChunkGateway locationChunkGateway = this.currentGateway;
			ILocationChunkGateway locationChunkGateway2 = (locationChunkGateway != null) ? locationChunkGateway.NextChunkGateway : null;
			if (locationChunkGateway2 != null)
			{
				this.UpdateLockState(locationChunkGateway2.IsLocked);
				locationChunkGateway2.LockStateChanged += this.OnConnectedGatewayLockStateChanged;
			}
		}

		// Token: 0x0600145B RID: 5211 RVA: 0x000402ED File Offset: 0x0003E4ED
		private void OnDestroy()
		{
			ILocationChunkGateway locationChunkGateway = this.currentGateway;
			if (((locationChunkGateway != null) ? locationChunkGateway.NextChunkGateway : null) != null)
			{
				this.currentGateway.NextChunkGateway.LockStateChanged -= this.OnConnectedGatewayLockStateChanged;
			}
		}

		// Token: 0x04000BDB RID: 3035
		private ILocationChunkGateway currentGateway;
	}
}
