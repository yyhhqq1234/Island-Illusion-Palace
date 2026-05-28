using System;
using Game.LevelGeneration;
using UnityEngine;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000278 RID: 632
	[DefaultExecutionOrder(-110)]
	[SelectionBase]
	public sealed class TypedLocationGate : LocationGate
	{
		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x06001597 RID: 5527 RVA: 0x00045346 File Offset: 0x00043546
		protected override int GatewayConnectionMask
		{
			get
			{
				return (int)this.connectionType;
			}
		}

		// Token: 0x04000C89 RID: 3209
		public TypedLocationGate.ConnectionType connectionType = TypedLocationGate.ConnectionType.Universal;

		// Token: 0x02000501 RID: 1281
		[Flags]
		public enum ConnectionType
		{
			// Token: 0x04001AB2 RID: 6834
			Ground = 1,
			// Token: 0x04001AB3 RID: 6835
			Grass = 2,
			// Token: 0x04001AB4 RID: 6836
			Universal = -1
		}
	}
}
