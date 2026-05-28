using System;
using System.Collections.Generic;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.AI;

namespace Unliving.LevelGeneration
{
	// Token: 0x0200026A RID: 618
	public sealed class GameLocationMeshGenerator : IDisposable
	{
		// Token: 0x060014CD RID: 5325 RVA: 0x000425EF File Offset: 0x000407EF
		private static void CreateMesh(ref Mesh mesh)
		{
			if (mesh == null)
			{
				mesh = new Mesh();
				mesh.MarkDynamic();
			}
		}

		// Token: 0x060014CE RID: 5326 RVA: 0x00042609 File Offset: 0x00040809
		private static void DisposeObject<T>(ref T obj) where T : UnityEngine.Object
		{
			if (obj == null)
			{
				return;
			}
			UnityEngine.Object.DestroyImmediate(obj);
			obj = default(T);
		}

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x060014CF RID: 5327 RVA: 0x00042638 File Offset: 0x00040838
		public Bounds MeshWorldBounds
		{
			get
			{
				if (!(this.locationMesh != null))
				{
					return default(Bounds);
				}
				return this.locationMesh.bounds;
			}
		}

		// Token: 0x060014D0 RID: 5328 RVA: 0x00042668 File Offset: 0x00040868
		private void GenerateLocationMesh()
		{
			NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
			GameLocationMeshGenerator.CreateMesh(ref this.locationMesh);
			this.locationMesh.Clear();
			this.locationMesh.vertices = navMeshTriangulation.vertices;
			this.locationMesh.triangles = navMeshTriangulation.indices;
			this.locationMesh.RecalculateBounds();
			this.locationMesh.MarkModified();
		}

		// Token: 0x060014D1 RID: 5329 RVA: 0x000426CC File Offset: 0x000408CC
		private void UpdateChunkTransitionsMesh(ILocationChunk chunk)
		{
			GameLocationMeshGenerator.CreateMesh(ref this.chunkTransitionsMesh);
			GameLocationMeshGenerator.CombineMeshesBuffer.Clear();
			IList<ILocationChunkGateway> gateways = chunk.Gateways;
			for (int i = 0; i < gateways.Count; i++)
			{
				ILocationChunkGateway locationChunkGateway = gateways[i];
				ILocationChunkTransitionArea locationChunkTransitionArea = (locationChunkGateway != null) ? locationChunkGateway.TransitionArea : null;
				Mesh mesh;
				if (locationChunkTransitionArea == null)
				{
					mesh = null;
				}
				else
				{
					Collider2D area = locationChunkTransitionArea.Area;
					mesh = ((area != null) ? area.CreateMesh(true, true) : null);
				}
				Mesh mesh2 = mesh;
				if (!(mesh2 == null))
				{
					GameLocationMeshGenerator.CombineMeshesBuffer.Add(new CombineInstance
					{
						mesh = mesh2
					});
				}
			}
			this.chunkTransitionsMesh.Clear();
			this.chunkTransitionsMesh.CombineMeshes(GameLocationMeshGenerator.CombineMeshesBuffer.ToArray(), true, false, false);
			this.chunkTransitionsMesh.MarkModified();
		}

		// Token: 0x060014D2 RID: 5330 RVA: 0x00042786 File Offset: 0x00040986
		public GameLocationMeshGenerator(GameLocation gameLocation, Material meshMaterial)
		{
			this.gameLocation = gameLocation;
			this.meshMaterial = meshMaterial;
			this.isMeshDirty = true;
		}

		// Token: 0x060014D3 RID: 5331 RVA: 0x000427A3 File Offset: 0x000409A3
		public Mesh GetLocationMesh(out Material meshMaterial)
		{
			if (this.isMeshDirty)
			{
				this.GenerateLocationMesh();
			}
			meshMaterial = this.meshMaterial;
			this.isMeshDirty = false;
			return this.locationMesh;
		}

		// Token: 0x060014D4 RID: 5332 RVA: 0x000427C8 File Offset: 0x000409C8
		public Mesh GetLocationMesh()
		{
			Material material;
			return this.GetLocationMesh(out material);
		}

		// Token: 0x060014D5 RID: 5333 RVA: 0x000427E0 File Offset: 0x000409E0
		public GameObject GetLocationMeshObject()
		{
			this.GetLocationMesh();
			if (this.locationMeshObject == null)
			{
				this.locationMeshObject = new GameObject(this.gameLocation.Type.ToString() + "_meshRenderer");
				this.locationMeshObject.AddComponent<MeshRenderer>().sharedMaterial = this.meshMaterial;
				this.locationMeshObject.AddComponent<MeshFilter>().mesh = this.locationMesh;
			}
			return this.locationMeshObject;
		}

		// Token: 0x060014D6 RID: 5334 RVA: 0x00042862 File Offset: 0x00040A62
		public void SetMeshDirty()
		{
			this.isMeshDirty = true;
		}

		// Token: 0x060014D7 RID: 5335 RVA: 0x0004286B File Offset: 0x00040A6B
		public void Dispose()
		{
			GameLocationMeshGenerator.DisposeObject<Mesh>(ref this.locationMesh);
			GameLocationMeshGenerator.DisposeObject<GameObject>(ref this.locationMeshObject);
			GameLocationMeshGenerator.DisposeObject<GameObject>(ref this.locationMeshObject);
		}

		// Token: 0x04000C17 RID: 3095
		private static readonly List<CombineInstance> CombineMeshesBuffer = new List<CombineInstance>(32);

		// Token: 0x04000C18 RID: 3096
		private readonly GameLocation gameLocation;

		// Token: 0x04000C19 RID: 3097
		private readonly Material meshMaterial;

		// Token: 0x04000C1A RID: 3098
		private Mesh locationMesh;

		// Token: 0x04000C1B RID: 3099
		private Mesh chunkTransitionsMesh;

		// Token: 0x04000C1C RID: 3100
		private GameObject locationMeshObject;

		// Token: 0x04000C1D RID: 3101
		private bool isMeshDirty;
	}
}
