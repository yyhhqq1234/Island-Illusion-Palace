using System;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000088 RID: 136
	[Name("Apotheosis Dead Fullscreen Effect", 0)]
	[Category("Unliving/Misc")]
	public sealed class ApotheosisDeadFullscreenEffectNode : GameContextDependentNodeBase
	{
		// Token: 0x060003C1 RID: 961 RVA: 0x0000CF50 File Offset: 0x0000B150
		private void ActivateEffect(Flow flow)
		{
			IPlayerCamera playerCamera;
			if (base.CurrentGame.Services.TryGet<IPlayerCamera>(out playerCamera))
			{
				this.fxInstance = UnityEngine.Object.Instantiate<GameObject>(this.fullscreenEffectPrefab.value, playerCamera.CameraComponent.transform);
				this.fxInstance.transform.localPosition = new Vector3(0f, 0f, 1f);
				this.apotheosisSprite.value.sortingLayerName = "UI";
				this.apotheosisSprite.value.sortingOrder = 10001;
				this.flowOut.Call(flow);
			}
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x0000CFEC File Offset: 0x0000B1EC
		protected override void RegisterPorts()
		{
			base.AddFlowInput("Activate FX", new FlowHandler(this.ActivateEffect), "");
			base.AddValueOutput<GameObject>("FX instance", () => this.fxInstance, "");
			this.fullscreenEffectPrefab = base.AddValueInput<GameObject>("fullscreenEffectPrefab", "");
			this.apotheosisSprite = base.AddValueInput<SpriteRenderer>("apotheosisSprite", "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000246 RID: 582
		private ValueInput<GameObject> fullscreenEffectPrefab;

		// Token: 0x04000247 RID: 583
		private ValueInput<SpriteRenderer> apotheosisSprite;

		// Token: 0x04000248 RID: 584
		private FlowOutput flowOut;

		// Token: 0x04000249 RID: 585
		private GameObject fxInstance;
	}
}
