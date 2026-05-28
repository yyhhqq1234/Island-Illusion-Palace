using System;
using System.Collections.Generic;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x02000043 RID: 67
	public sealed class TestBuffArea : MonoBehaviour
	{
		// Token: 0x0600023A RID: 570 RVA: 0x000095B0 File Offset: 0x000077B0
		private static void DrawDebugLine(object obj0, object obj1, Color color, float duration, bool animate)
		{
			Color color2 = color;
			if (animate)
			{
				color2.a *= Mathf.Lerp(0.25f, 1f, Mathf.PingPong(Time.time * 5f, 1f));
			}
			Debug.DrawLine((obj0 as MonoBehaviour).transform.position, (obj1 as MonoBehaviour).transform.position, color2, duration);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x00009619 File Offset: 0x00007819
		private void Awake()
		{
			this.collider = base.GetComponent<Collider2D>();
			if (this.temporaryBuffsGenerationDelay > 0f)
			{
				this.temporaryBuffsGenerationDelay += UnityEngine.Random.value * 0.2f;
			}
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0000964C File Offset: 0x0000784C
		private void Update()
		{
			if (this.temporaryBuffsGenerationDelay > 0f && Time.time > this.nextTemporaryBuffTime)
			{
				for (int i = 0; i < this.objectsInArea.Count; i++)
				{
					IBuffsController objectBuffsController = this.objectsInArea[i].objectBuffsController;
					if (objectBuffsController != null)
					{
						TestBuffArea.TestTemporaryBuff buff = new TestBuffArea.TestTemporaryBuff(this, Mathf.Max(this.temporaryBuffsDuration, 0.1f), this.generateAdditiveBuffs, this.generateContinuousBuffs)
						{
							color = this.temporaryBuffColor
						};
						objectBuffsController.AddBuff(buff);
					}
				}
				this.nextTemporaryBuffTime = Time.time + this.temporaryBuffsGenerationDelay;
			}
		}

		// Token: 0x0600023D RID: 573 RVA: 0x000096EC File Offset: 0x000078EC
		private void OnTriggerEnter2D(Collider2D collider)
		{
			IBuffableObject component = collider.GetComponent<IBuffableObject>();
			if (component != null && component.BuffsController != null)
			{
				object obj;
				if (!this.generateConstantBuff)
				{
					obj = null;
				}
				else
				{
					(obj = new TestBuffArea.TestConstantBuff(this, this.generateAdditiveConstantBuff, this.generateContinuousConstantBuff)).color = this.constantBuffColor;
				}
				TestBuffArea.TestConstantBuff testConstantBuff = obj;
				TestBuffArea.ObjectNode item = new TestBuffArea.ObjectNode
				{
					gameObject = collider.gameObject,
					objectBuffsController = component.BuffsController,
					constantBuff = testConstantBuff
				};
				this.objectsInArea.Add(item);
				component.BuffsController.AddBuff(testConstantBuff);
			}
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0000977C File Offset: 0x0000797C
		private void OnTriggerExit2D(Collider2D collider)
		{
			int num = this.objectsInArea.FindIndex((TestBuffArea.ObjectNode node) => node.gameObject == collider.gameObject);
			if (num != -1)
			{
				IBuff constantBuff = this.objectsInArea[num].constantBuff;
				if (constantBuff != null)
				{
					constantBuff.Complete();
				}
				this.objectsInArea.RemoveAt(num);
			}
		}

		// Token: 0x0600023F RID: 575 RVA: 0x000097DC File Offset: 0x000079DC
		private void OnDestroy()
		{
			for (int i = 0; i < this.objectsInArea.Count; i++)
			{
				IBuff constantBuff = this.objectsInArea[i].constantBuff;
				if (constantBuff != null)
				{
					constantBuff.Complete();
				}
			}
		}

		// Token: 0x06000240 RID: 576 RVA: 0x0000981C File Offset: 0x00007A1C
		private void OnDrawGizmos()
		{
			if (this.collider != null)
			{
				Gizmos.color = Color.red;
				float x = this.collider.bounds.extents.x;
				Gizmos.DrawWireSphere(base.transform.position, x);
			}
		}

		// Token: 0x04000163 RID: 355
		public bool generateConstantBuff;

		// Token: 0x04000164 RID: 356
		public bool generateAdditiveConstantBuff;

		// Token: 0x04000165 RID: 357
		public bool generateContinuousConstantBuff;

		// Token: 0x04000166 RID: 358
		[Header("Temporary Buffs")]
		public float temporaryBuffsGenerationDelay;

		// Token: 0x04000167 RID: 359
		public float temporaryBuffsDuration;

		// Token: 0x04000168 RID: 360
		public bool generateAdditiveBuffs;

		// Token: 0x04000169 RID: 361
		public bool generateContinuousBuffs;

		// Token: 0x0400016A RID: 362
		[Header("Gizmos")]
		public Color constantBuffColor = new Color(0f, 0f, 1f, 0.5f);

		// Token: 0x0400016B RID: 363
		public Color temporaryBuffColor = new Color(1f, 0f, 0f, 0.5f);

		// Token: 0x0400016C RID: 364
		private Collider2D collider;

		// Token: 0x0400016D RID: 365
		private List<TestBuffArea.ObjectNode> objectsInArea = new List<TestBuffArea.ObjectNode>();

		// Token: 0x0400016E RID: 366
		private float nextTemporaryBuffTime;

		// Token: 0x02000415 RID: 1045
		private sealed class TestConstantBuff : BuffBase
		{
			// Token: 0x170006F5 RID: 1781
			// (get) Token: 0x0600226E RID: 8814 RVA: 0x0006AF2F File Offset: 0x0006912F
			protected override bool IsContinuous
			{
				get
				{
					return this.isContinuous;
				}
			}

			// Token: 0x0600226F RID: 8815 RVA: 0x0006AF37 File Offset: 0x00069137
			protected override void CancelBuff()
			{
			}

			// Token: 0x06002270 RID: 8816 RVA: 0x0006AF39 File Offset: 0x00069139
			protected override void UseBuff(float updateStep)
			{
				TestBuffArea.DrawDebugLine(base.Sender, base.TargetObject, this.color, base.UsingLoopStep, true);
			}

			// Token: 0x06002271 RID: 8817 RVA: 0x0006AF5C File Offset: 0x0006915C
			public TestConstantBuff(object buffSender, bool isAdditive, bool isContinuous) : base(isContinuous ? 1001 : 1000, buffSender, new BuffBase.InitializationArgs
			{
				isConstant = true,
				isAdditive = isAdditive,
				usingLoopStep = 0.1f
			})
			{
				this.isContinuous = isContinuous;
			}

			// Token: 0x06002272 RID: 8818 RVA: 0x0006AFAB File Offset: 0x000691AB
			protected override void OnUpdate()
			{
				base.OnUpdate();
				if (!this.isContinuous)
				{
					TestBuffArea.DrawDebugLine(base.Sender, base.TargetObject, this.color, 0f, false);
				}
			}

			// Token: 0x040015CA RID: 5578
			public Color color;

			// Token: 0x040015CB RID: 5579
			private bool isContinuous;
		}

		// Token: 0x02000416 RID: 1046
		private sealed class TestTemporaryBuff : BuffBase
		{
			// Token: 0x170006F6 RID: 1782
			// (get) Token: 0x06002273 RID: 8819 RVA: 0x0006AFD8 File Offset: 0x000691D8
			protected override bool IsContinuous
			{
				get
				{
					return this._isContinuous;
				}
			}

			// Token: 0x06002274 RID: 8820 RVA: 0x0006AFE0 File Offset: 0x000691E0
			protected override void CancelBuff()
			{
			}

			// Token: 0x06002275 RID: 8821 RVA: 0x0006AFE2 File Offset: 0x000691E2
			protected override void UseBuff(float updateStep)
			{
				TestBuffArea.DrawDebugLine(base.Sender, base.TargetObject, this.color, base.UsingLoopStep, this._isContinuous);
			}

			// Token: 0x06002276 RID: 8822 RVA: 0x0006B008 File Offset: 0x00069208
			public TestTemporaryBuff(object buffSender, float duartion, bool isAdditive, bool isContinuous) : base(isContinuous ? 1101 : 1100, buffSender, new BuffBase.InitializationArgs
			{
				isConstant = false,
				isAdditive = isAdditive,
				duration = duartion,
				usingLoopStep = 0.1f
			})
			{
				this._isContinuous = isContinuous;
			}

			// Token: 0x040015CC RID: 5580
			public Color color;

			// Token: 0x040015CD RID: 5581
			private bool _isContinuous;
		}

		// Token: 0x02000417 RID: 1047
		private struct ObjectNode
		{
			// Token: 0x040015CE RID: 5582
			public GameObject gameObject;

			// Token: 0x040015CF RID: 5583
			public IBuffsController objectBuffsController;

			// Token: 0x040015D0 RID: 5584
			public IBuff constantBuff;
		}
	}
}
