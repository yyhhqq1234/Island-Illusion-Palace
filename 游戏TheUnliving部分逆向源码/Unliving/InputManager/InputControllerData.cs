using System;
using System.Collections.Generic;
using Game.InputManager;
using TMPro;
using UnityEngine;

namespace Unliving.InputManager
{
	// Token: 0x0200029E RID: 670
	[Serializable]
	public class InputControllerData
	{
		// Token: 0x0600171F RID: 5919 RVA: 0x00049AF6 File Offset: 0x00047CF6
		public bool IsValidController(string name)
		{
			return string.Equals(this.name, name, StringComparison.InvariantCultureIgnoreCase) || string.Equals(this.alternativeName, name, StringComparison.InvariantCultureIgnoreCase);
		}

		// Token: 0x06001720 RID: 5920 RVA: 0x00049B1C File Offset: 0x00047D1C
		public InputControllerData.ButtonGlyph GetButtonGlyph(string buttonName, InputAxisContribution axisContribution)
		{
			buttonName = this.glyphPrefix + buttonName;
			List<TMP_SpriteCharacter> spriteCharacterTable = this.glyphsAtlas.spriteCharacterTable;
			for (int i = 0; i < spriteCharacterTable.Count; i++)
			{
				TMP_SpriteCharacter tmp_SpriteCharacter = spriteCharacterTable[i];
				if (string.Equals(tmp_SpriteCharacter.name, buttonName, StringComparison.InvariantCultureIgnoreCase))
				{
					return new InputControllerData.ButtonGlyph
					{
						name = buttonName,
						axisContribution = axisContribution,
						sprite = (tmp_SpriteCharacter.glyph as TMP_SpriteGlyph).sprite
					};
				}
			}
			return null;
		}

		// Token: 0x04000D67 RID: 3431
		public InputControllerType controllerType;

		// Token: 0x04000D68 RID: 3432
		[SerializeField]
		private string name;

		// Token: 0x04000D69 RID: 3433
		[SerializeField]
		private string alternativeName;

		// Token: 0x04000D6A RID: 3434
		[SerializeField]
		private string glyphPrefix;

		// Token: 0x04000D6B RID: 3435
		[SerializeField]
		private TMP_SpriteAsset glyphsAtlas;

		// Token: 0x02000514 RID: 1300
		[Serializable]
		public class ButtonGlyph
		{
			// Token: 0x04001B00 RID: 6912
			public string name;

			// Token: 0x04001B01 RID: 6913
			public InputAxisContribution axisContribution;

			// Token: 0x04001B02 RID: 6914
			public Sprite sprite;
		}
	}
}
