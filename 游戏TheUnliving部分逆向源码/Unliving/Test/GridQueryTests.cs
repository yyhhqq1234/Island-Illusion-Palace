using System;
using Common.Math;
using Game.Utility;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x0200003B RID: 59
	public sealed class GridQueryTests : MonoBehaviour
	{
		// Token: 0x06000209 RID: 521 RVA: 0x00008028 File Offset: 0x00006228
		private void Awake()
		{
			float num = (this.testAgentsCount > 0) ? (this.maxAgentRadius * 4f) : this.cellSize;
			this.currentGrid = new UniformGrid2D(base.transform.position, this.gridSize, num, 1024);
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000807C File Offset: 0x0000627C
		private void Start()
		{
			Vector2 origin = this.currentGrid.Origin;
			Vector2 size = this.currentGrid.Size;
			int num = UnityEngine.Random.Range(0, 1000);
			for (int i = 0; i < this.testAgentsCount; i++)
			{
				Vector2 r2Value = PhiSequence.GetR2Value(i + num, false);
				r2Value.x *= size.x;
				r2Value.y *= size.y;
				GridQueryTests.TestGridAgent agent = new GridQueryTests.TestGridAgent(origin + r2Value, UnityEngine.Random.Range(0.05f, this.maxAgentRadius));
				this.currentGrid.AddAgent(agent);
			}
			this.timeOffsets = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) * 100f;
		}

		// Token: 0x0600020B RID: 523 RVA: 0x00008138 File Offset: 0x00006338
		private void Update()
		{
			Vector2 size = this.currentGrid.Size;
			Vector2 origin = this.currentGrid.Origin;
			Vector2 position = origin + size * 0.5f;
			float num = Time.time + this.timeOffsets.x;
			float num2 = Time.time + this.timeOffsets.y;
			for (int i = 0; i < this.currentGrid.AgentsCount; i++)
			{
				GridQueryTests.TestGridAgent testGridAgent = (GridQueryTests.TestGridAgent)this.currentGrid.GetAgent(i);
				Vector2 position2 = testGridAgent.position;
				float num3 = (position2.x - origin.x) / size.x;
				float num4 = (position2.y - origin.y) / size.y;
				Vector2 a = new Vector2
				{
					x = Mathf.PerlinNoise((num + (float)i) * this.velocityNoiseFrequency, (num2 + (float)i) * this.velocityNoiseFrequency) * 2f - 1f,
					y = Mathf.PerlinNoise((num2 + (float)i) * this.velocityNoiseFrequency, (num + (float)i) * this.velocityNoiseFrequency) * 2f - 1f
				};
				a.Normalize();
				a *= this.agentSpeed;
				testGridAgent.position += a * Time.deltaTime;
				if (num3 < 0f || num3 > 1f || num4 < 0f || num4 > 1f)
				{
					testGridAgent.position = position;
				}
			}
			this.currentGrid.UpdateGrid();
			Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			GridQueryTests.TestGridAgent testGridAgent2 = this.currentGrid.GetAgentAtPoint(worldPoint, -1) as GridQueryTests.TestGridAgent;
			if (testGridAgent2 != null && Input.GetMouseButtonDown(1))
			{
				this.currentGrid.RemoveAgent(testGridAgent2.Index);
			}
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000831D File Offset: 0x0000651D
		private void OnDrawGizmos()
		{
			UniformGrid2D uniformGrid2D = this.currentGrid;
			if (uniformGrid2D == null)
			{
				return;
			}
			uniformGrid2D.DrawGizmos();
		}

		// Token: 0x0400011B RID: 283
		public float cellSize = 1f;

		// Token: 0x0400011C RID: 284
		public Vector2 gridSize = new Vector2(50f, 50f);

		// Token: 0x0400011D RID: 285
		public Transform singleCellTester;

		// Token: 0x0400011E RID: 286
		public float cellTesterRadius;

		// Token: 0x0400011F RID: 287
		public int testAgentsCount = 300;

		// Token: 0x04000120 RID: 288
		public float maxAgentRadius = 1f;

		// Token: 0x04000121 RID: 289
		public float agentSpeed = 5f;

		// Token: 0x04000122 RID: 290
		public float velocityNoiseFrequency = 0.5f;

		// Token: 0x04000123 RID: 291
		private UniformGrid2D currentGrid;

		// Token: 0x04000124 RID: 292
		private Vector2 timeOffsets;

		// Token: 0x02000412 RID: 1042
		private sealed class TestGridAgent : UniformGrid2D.IAgent
		{
			// Token: 0x170006EC RID: 1772
			// (get) Token: 0x0600225A RID: 8794 RVA: 0x0006ACE1 File Offset: 0x00068EE1
			float UniformGrid2D.IAgent.Radius
			{
				get
				{
					return this.radius;
				}
			}

			// Token: 0x170006ED RID: 1773
			// (get) Token: 0x0600225B RID: 8795 RVA: 0x0006ACE9 File Offset: 0x00068EE9
			object UniformGrid2D.IAgent.ParentBehaviour
			{
				get
				{
					return null;
				}
			}

			// Token: 0x170006EE RID: 1774
			// (get) Token: 0x0600225C RID: 8796 RVA: 0x0006ACEC File Offset: 0x00068EEC
			bool UniformGrid2D.IAgent.IsStatic
			{
				get
				{
					return false;
				}
			}

			// Token: 0x170006EF RID: 1775
			// (get) Token: 0x0600225D RID: 8797 RVA: 0x0006ACEF File Offset: 0x00068EEF
			public int Layer
			{
				get
				{
					return 1;
				}
			}

			// Token: 0x170006F0 RID: 1776
			// (get) Token: 0x0600225E RID: 8798 RVA: 0x0006ACF2 File Offset: 0x00068EF2
			public int Index
			{
				get
				{
					return this.index;
				}
			}

			// Token: 0x0600225F RID: 8799 RVA: 0x0006ACFA File Offset: 0x00068EFA
			public TestGridAgent(Vector2 position, float radius)
			{
				this.position = position;
				this.radius = radius;
			}

			// Token: 0x06002260 RID: 8800 RVA: 0x0006AD10 File Offset: 0x00068F10
			void UniformGrid2D.IAgent.UpdateState(ref Vector2 position)
			{
				position = this.position;
			}

			// Token: 0x06002261 RID: 8801 RVA: 0x0006AD1E File Offset: 0x00068F1E
			void UniformGrid2D.IAgent.OnAgentIndexChanged(int newIndex)
			{
				this.index = newIndex;
			}

			// Token: 0x040015C2 RID: 5570
			public float radius;

			// Token: 0x040015C3 RID: 5571
			public Vector2 position;

			// Token: 0x040015C4 RID: 5572
			private int index;
		}
	}
}
