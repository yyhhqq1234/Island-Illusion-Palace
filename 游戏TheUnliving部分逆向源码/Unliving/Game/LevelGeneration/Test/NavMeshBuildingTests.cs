using System;
using System.Collections.Generic;
using Game.LevelGeneration.Utility;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace Game.LevelGeneration.Test
{
	// Token: 0x02000005 RID: 5
	[DefaultExecutionOrder(-120)]
	public sealed class NavMeshBuildingTests : MonoBehaviour
	{
		// Token: 0x0600000A RID: 10 RVA: 0x0000213C File Offset: 0x0000033C
		private void GetChildRecursive(GameObject obj, List<GameObject> listOfChildren)
		{
			if (obj != null)
			{
				foreach (object obj2 in obj.transform)
				{
					Transform transform = (Transform)obj2;
					if (!(transform == null) && transform.gameObject.activeInHierarchy)
					{
						listOfChildren.Add(transform.gameObject);
						this.GetChildRecursive(transform.gameObject, listOfChildren);
					}
				}
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000021C8 File Offset: 0x000003C8
		private Bounds GetCurrentGroundBounds()
		{
			Bounds result = this.groundBounds;
			if (this.root != null)
			{
				result.center += this.root.position;
			}
			return result;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002208 File Offset: 0x00000408
		private void Awake()
		{
			List<GameObject> list = new List<GameObject>();
			List<NavMeshBuildSource> list2 = new List<NavMeshBuildSource>();
			if (this.root != null)
			{
				if (!this.keepDefinedGroundBounds)
				{
					Renderer[] array = this.root.GetComponentsInChildren<TilemapRenderer>();
					Renderer[] array2 = array;
					if (array2.Length != 0)
					{
						Vector2 vector = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
						Vector2 vector2 = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
						foreach (TilemapRenderer tilemapRenderer in array2)
						{
							if (this.getExactGroundBounds)
							{
								Tilemap component = tilemapRenderer.GetComponent<Tilemap>();
								if (component != null)
								{
									component.CompressBounds();
								}
							}
							Bounds bounds = tilemapRenderer.bounds;
							vector = Vector2.Max(vector, bounds.max);
							vector2 = Vector2.Min(vector2, bounds.min);
						}
						this.groundBounds.SetMinMax(vector2, vector);
					}
				}
				this.GetChildRecursive(this.root.gameObject, list);
			}
			if (list.Count != 0)
			{
				foreach (GameObject gameObject in list)
				{
					ILocationChunkVisitor locationChunkVisitor;
					NavMeshBuildSource item;
					if (!gameObject.TryGetComponent<ILocationChunkVisitor>(out locationChunkVisitor) && NavMeshUtility.GetNavMeshObstacleBuildSource(gameObject, out item, 1))
					{
						list2.Add(item);
					}
				}
			}
			if (this.groundBounds.size != Vector3.zero)
			{
				Bounds currentGroundBounds = this.GetCurrentGroundBounds();
				list2.Add(NavMeshUtility.GetNavMeshGroundBuildSource(currentGroundBounds.center, currentGroundBounds.size, 0));
			}
			if (list2.Count != 0)
			{
				Vector3 position = (this.root == null) ? this.groundBounds.center : this.root.position;
				Quaternion rotation = Quaternion.AngleAxis(-90f, new Vector3
				{
					x = 1f
				});
				list2.Add(NavMeshUtility.GetNavMeshGroundBuildSource(this.groundBounds.center, this.groundBounds.size, 0));
				for (int j = 0; j < NavMesh.GetSettingsCount(); j++)
				{
					NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(j), list2, new Bounds(default(Vector3), Vector3.one * 1000f), position, rotation);
					this.addedNavmeshData.Add(NavMesh.AddNavMeshData(navMeshData));
				}
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002478 File Offset: 0x00000678
		private void OnDestroy()
		{
			foreach (NavMeshDataInstance navMeshDataInstance in this.addedNavmeshData)
			{
				navMeshDataInstance.Remove();
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000024CC File Offset: 0x000006CC
		private void OnDrawGizmos()
		{
			if (this.root == null)
			{
				return;
			}
			Bounds currentGroundBounds = this.GetCurrentGroundBounds();
			Gizmos.DrawWireCube(currentGroundBounds.center, currentGroundBounds.size);
		}

		// Token: 0x04000007 RID: 7
		public Transform root;

		// Token: 0x04000008 RID: 8
		public Bounds groundBounds;

		// Token: 0x04000009 RID: 9
		public bool keepDefinedGroundBounds;

		// Token: 0x0400000A RID: 10
		public bool getExactGroundBounds;

		// Token: 0x0400000B RID: 11
		[NonSerialized]
		private readonly List<NavMeshDataInstance> addedNavmeshData = new List<NavMeshDataInstance>();
	}
}
