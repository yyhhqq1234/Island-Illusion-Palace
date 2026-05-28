using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Unliving.Cutscenes
{
	// Token: 0x02000325 RID: 805
	public class TimeMarkingsInteractible : GameBehaviourBase
	{
		// Token: 0x06001AEB RID: 6891 RVA: 0x00054B7C File Offset: 0x00052D7C
		private void OnDrawGizmosSelected()
		{
			foreach (TimeMarkingsSection timeMarkingsSection in this.zones)
			{
				Gizmos.color = Color.yellow;
				Vector3 center = new Vector3((timeMarkingsSection.start.position.x + timeMarkingsSection.end.position.x) / 2f, (timeMarkingsSection.start.position.y + timeMarkingsSection.end.position.y) / 2f, 0f);
				Vector3 size = new Vector3(timeMarkingsSection.start.position.x - timeMarkingsSection.end.position.x, timeMarkingsSection.start.position.y - timeMarkingsSection.end.position.y, 1f);
				Gizmos.DrawWireCube(center, size);
			}
		}

		// Token: 0x06001AEC RID: 6892 RVA: 0x00054C84 File Offset: 0x00052E84
		public void GenerateMarkings()
		{
			Debug.Log("Begun generating markisgs");
			GameObject gameObject = base.transform.Find("row_template").gameObject;
			Transform parent = base.transform.Find("Rows");
			int num = 1;
			int num2 = 1;
			foreach (TimeMarkingsSection timeMarkingsSection in this.zones)
			{
				Texture2D texture2D = this.PickRandomRowTexture();
				float num3 = (float)(texture2D.width / this.rowCapacity) / 32f / 2f;
				float num4 = (float)texture2D.height / 32f / 2f;
				float num5 = timeMarkingsSection.start.position.x + num3 + this.horizontalPadding;
				float num6 = timeMarkingsSection.start.position.y + num4 + this.verticalPadding;
				float num7 = (float)texture2D.height / 32f + this.betweenRowPadding;
				float num8 = timeMarkingsSection.end.position.y - this.verticalPadding - num6;
				int num9 = (int)(num8 / num7);
				Debug.Log(string.Format("Zone: ({0}; {1}), step: {2}, yHeight: {3}, i: {4}", new object[]
				{
					num5,
					num6,
					num7,
					num8,
					num9
				}));
				GameObject gameObject2 = new GameObject(string.Format("zone {0}", num2));
				gameObject2.transform.SetParent(parent);
				for (int i = 0; i < num9; i++)
				{
					Vector3 position = new Vector3(num5, num6 + num7 * (float)i, 0f);
					this.DuplicateRow(gameObject, position, texture2D, num, gameObject2.transform);
					num++;
				}
				num2++;
			}
			this.counterMaxCapacity = num * this.rowCapacity;
		}

		// Token: 0x06001AED RID: 6893 RVA: 0x00054E80 File Offset: 0x00053080
		private void DuplicateRow(GameObject template, Vector3 position, Texture2D texture, int rowCounter, Transform rowsParent)
		{
			Debug.Log(string.Format("\tRow {0}", rowCounter));
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(template);
			gameObject.name = string.Format("row{0}", rowCounter);
			gameObject.transform.SetParent(rowsParent);
			gameObject.transform.position = position;
			gameObject.GetComponent<SpriteRenderer>();
			gameObject.gameObject.SetActive(true);
		}

		// Token: 0x06001AEE RID: 6894 RVA: 0x00054EEB File Offset: 0x000530EB
		private Texture2D PickRandomRowTexture()
		{
			return this.markingsRow[0];
		}

		// Token: 0x06001AEF RID: 6895 RVA: 0x00054EF9 File Offset: 0x000530F9
		public void IncrementCounter()
		{
			this.markingsCounter++;
			this.RedrawMarkings(true);
		}

		// Token: 0x06001AF0 RID: 6896 RVA: 0x00054F10 File Offset: 0x00053110
		public void HideAllMarkings()
		{
			SpriteRenderer[] componentsInChildren = base.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = new Color(1f, 1f, 1f, 0f);
			}
		}

		// Token: 0x06001AF1 RID: 6897 RVA: 0x00054F54 File Offset: 0x00053154
		public void RedrawMarkings(bool quickRedraw = true)
		{
			if (this.markingsCounter < 0)
			{
				this.markingsCounter = 0;
			}
			if (this.markingsCounter == 0)
			{
				return;
			}
			int num = this.markingsCounter - 1;
			int num2 = (int)Mathf.Floor((float)(num / this.rowCapacity));
			int markingsInRow = num - num2 * this.rowCapacity;
			int num3 = -1;
			foreach (SpriteRenderer spriteRenderer in base.GetComponentsInChildren<SpriteRenderer>())
			{
				num3++;
				if (num3 == num2)
				{
					this.AssignSpriteToRow(spriteRenderer, markingsInRow);
					if (quickRedraw)
					{
					}
				}
				else if (num3 < num2)
				{
					this.AssignSpriteToRow(spriteRenderer, this.rowCapacity - 1);
				}
				else
				{
					spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
				}
			}
		}

		// Token: 0x06001AF2 RID: 6898 RVA: 0x0005500C File Offset: 0x0005320C
		private void AssignSpriteToRow(SpriteRenderer rowRenderer, int markingsInRow)
		{
			rowRenderer.color = new Color(1f, 1f, 1f, 1f);
			string empty = string.Empty;
			Sprite[] array = new Sprite[0];
			if (array.Length != this.rowCapacity)
			{
				Debug.LogError(string.Format("Something is wrong: the texture {0} has {1} sprites – but the compnent expects {2}. Have you changed the texture art and forget to update how high it counts?", empty, array.Length, this.rowCapacity));
				return;
			}
			rowRenderer.sprite = array[markingsInRow];
		}

		// Token: 0x06001AF3 RID: 6899 RVA: 0x0005507D File Offset: 0x0005327D
		public void Start()
		{
			this.HideAllMarkings();
			this.RedrawMarkings(true);
		}

		// Token: 0x04000F14 RID: 3860
		public List<TimeMarkingsSection> zones;

		// Token: 0x04000F15 RID: 3861
		public List<Texture2D> markingsRow;

		// Token: 0x04000F16 RID: 3862
		public int rowCapacity = 15;

		// Token: 0x04000F17 RID: 3863
		public float verticalPadding;

		// Token: 0x04000F18 RID: 3864
		public float horizontalPadding;

		// Token: 0x04000F19 RID: 3865
		public float betweenRowPadding;

		// Token: 0x04000F1A RID: 3866
		[Space]
		public int counterMaxCapacity;

		// Token: 0x04000F1B RID: 3867
		public int markingsCounter;
	}
}
