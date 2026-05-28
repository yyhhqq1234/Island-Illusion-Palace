using System;
using GraphProcessor;
using UnityEngine;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000300 RID: 768
	[CreateAssetMenu(fileName = "CharactersNamesNodeGraph", menuName = "Game/Plot/Character Plot Node Graph")]
	public class CharactersNamesNodeGraph : BaseGraph
	{
		// Token: 0x06001A1A RID: 6682 RVA: 0x00051C28 File Offset: 0x0004FE28
		public CharacterMetadata GetActualCharacterMetadata(CharacterPlotContext context, string characterID)
		{
			CharacterMetadata result = default(CharacterMetadata);
			int i = 0;
			while (i < this.nodes.Count)
			{
				CharacterNameNode characterNameNode = this.nodes[i] as CharacterNameNode;
				if (characterNameNode != null && string.Equals(characterID, characterNameNode.CharacterID))
				{
					result = characterNameNode.GetCurrentCharacterMetadata(context);
					if (result.HasVoiceoverEvent())
					{
						return result;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			for (int j = 0; j < this.nodes.Count; j++)
			{
				CharacterNameNode characterNameNode2 = this.nodes[j] as CharacterNameNode;
				if (characterNameNode2 != null && !string.Equals(characterID, characterNameNode2.CharacterID) && characterID.Contains(characterNameNode2.CharacterID))
				{
					result.voiceoverEvents = characterNameNode2.GetCurrentCharacterMetadata(context).voiceoverEvents;
					return result;
				}
			}
			return result;
		}
	}
}
