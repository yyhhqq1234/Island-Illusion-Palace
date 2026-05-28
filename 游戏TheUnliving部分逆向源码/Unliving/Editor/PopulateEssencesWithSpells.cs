using System;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.DropSystem;
using Unliving.Mobs.ActivationModifiers;

namespace Unliving.Editor
{
	// Token: 0x02000028 RID: 40
	public class PopulateEssencesWithSpells : GameBehaviourBase
	{
		// Token: 0x06000169 RID: 361 RVA: 0x00005EC4 File Offset: 0x000040C4
		private void OnValidate()
		{
			this.abilities = this.abilities.Replace("\n\n", "\n");
			this.sacrificeModifiers = this.sacrificeModifiers.Replace("\n\n", "\n");
			this.baseAttacks = this.baseAttacks.Replace("\n\n", "\n");
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00005F24 File Offset: 0x00004124
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Vector3 vector = new Vector3(this.startPoint.x + base.transform.position.x, this.startPoint.y + base.transform.position.y, 0f);
			Gizmos.DrawSphere(vector, 0.3f);
			Vector3 center = new Vector3(vector.x + this.width / 2f, vector.y - this.width / 2f, 0f);
			Vector3 size = new Vector3(this.width, this.width, 1f);
			Gizmos.DrawWireCube(center, size);
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00005FD8 File Offset: 0x000041D8
		public float CreateEssences(string spells, float offsetY = 0f, bool is_modifier = false)
		{
			Vector3 vector = new Vector3(this.origin.x, this.origin.y + offsetY, 0f);
			foreach (string text in spells.Split(new char[]
			{
				'\n'
			}))
			{
				Debug.Log(string.Format("{0}: {1}; {2}", text, vector.x, vector.y));
				this.CopyEssenceSpawner(text, vector, is_modifier);
				if (vector.x + this.step < this.origin.x + this.width)
				{
					vector.x += this.step;
				}
				else
				{
					vector.x = this.origin.x;
					vector.y -= this.step;
				}
			}
			vector.y -= 2f * this.step;
			return vector.y - this.origin.y;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x000060E4 File Offset: 0x000042E4
		private void CopyEssenceSpawner(string spell_name, Vector3 pos, bool is_modifier = true)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(base.transform.Find("EssenceSpawnerRoot").gameObject);
			gameObject.name = string.Format("{0}", spell_name);
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.position = pos;
			DropSpawner component = gameObject.transform.Find("Slot").GetComponent<DropSpawner>();
			if (is_modifier)
			{
				component.dropItemsPool.abilities = null;
				component.dropItemsPool.activationModifiers = new ActivationModifierDropable[1];
				MobActivationModifierID modifierID = (MobActivationModifierID)Enum.Parse(typeof(MobActivationModifierID), spell_name);
				component.dropItemsPool.activationModifiers[0] = new ActivationModifierDropable();
				component.dropItemsPool.activationModifiers[0].modifierID = modifierID;
				return;
			}
			component.dropItemsPool.abilities[0].abilityID = (AbilityID)Enum.Parse(typeof(AbilityID), spell_name);
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000061D4 File Offset: 0x000043D4
		public void Populate()
		{
			this.ValideSpellNames();
			new Vector3(this.startPoint.x, this.startPoint.y, 0f);
			float offsetY = this.CreateEssences(this.abilities, 0f, false);
			offsetY = this.CreateEssences(this.sacrificeModifiers, offsetY, true);
			offsetY = this.CreateEssences(this.baseAttacks, offsetY, false);
		}

		// Token: 0x0600016E RID: 366 RVA: 0x0000623C File Offset: 0x0000443C
		private void ValideSpellNames()
		{
			foreach (string text in this.abilities.Split(new char[]
			{
				'\n'
			}))
			{
				try
				{
					Enum.Parse(typeof(AbilityID), text);
				}
				catch (Exception ex)
				{
					Debug.LogError(text + ", " + ex.Message);
				}
			}
			foreach (string text2 in this.sacrificeModifiers.Split(new char[]
			{
				'\n'
			}))
			{
				try
				{
					Enum.Parse(typeof(MobActivationModifierID), text2);
				}
				catch (Exception ex2)
				{
					Debug.LogError(text2 + ", " + ex2.Message);
				}
			}
		}

		// Token: 0x040000AE RID: 174
		[TextArea(5, 10)]
		[SerializeField]
		private string abilities = "";

		// Token: 0x040000AF RID: 175
		[TextArea(5, 5)]
		[SerializeField]
		private string sacrificeModifiers = "";

		// Token: 0x040000B0 RID: 176
		[TextArea(3, 5)]
		[SerializeField]
		private string baseAttacks = "";

		// Token: 0x040000B1 RID: 177
		[SerializeField]
		private Vector2 startPoint;

		// Token: 0x040000B2 RID: 178
		[SerializeField]
		private float width;

		// Token: 0x040000B3 RID: 179
		[SerializeField]
		private float step = 3f;

		// Token: 0x040000B4 RID: 180
		private Vector3 origin;
	}
}
