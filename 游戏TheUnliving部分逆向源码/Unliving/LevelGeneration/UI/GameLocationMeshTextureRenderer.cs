using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x0200027A RID: 634
	public sealed class GameLocationMeshTextureRenderer : IDisposable
	{
		// Token: 0x060015A1 RID: 5537 RVA: 0x00045488 File Offset: 0x00043688
		private void CreateCamera()
		{
		}

		// Token: 0x060015A2 RID: 5538 RVA: 0x0004548A File Offset: 0x0004368A
		public GameLocationMeshTextureRenderer(GameLocationMeshGenerator locationMeshGenerator)
		{
			this.locationMeshGenerator = locationMeshGenerator;
			RenderPipelineManager.beginCameraRendering += this.OnBeginCameraRendering;
		}

		// Token: 0x060015A3 RID: 5539 RVA: 0x000454AA File Offset: 0x000436AA
		public void Update()
		{
			this.CreateCamera();
			this.camera.Render();
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x000454BD File Offset: 0x000436BD
		public void Dispose()
		{
			RenderPipelineManager.beginCameraRendering -= this.OnBeginCameraRendering;
			this.locationMeshGenerator.Dispose();
			if (this.camera != null)
			{
				UnityEngine.Object.Destroy(this.camera.gameObject);
			}
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x000454FC File Offset: 0x000436FC
		private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
		{
			if (camera == this.camera)
			{
				Material material;
				this.locationMeshGenerator.GetLocationMesh(out material);
			}
		}

		// Token: 0x04000C8D RID: 3213
		private readonly GameLocationMeshGenerator locationMeshGenerator;

		// Token: 0x04000C8E RID: 3214
		private Camera camera;

		// Token: 0x04000C8F RID: 3215
		private bool isDirty;
	}
}
