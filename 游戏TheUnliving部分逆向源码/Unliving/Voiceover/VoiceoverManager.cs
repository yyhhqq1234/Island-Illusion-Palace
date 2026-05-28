using System;
using Game.Core;

namespace Unliving.Voiceover
{
	// Token: 0x0200001A RID: 26
	public class VoiceoverManager : GlobalSceneManagerBase
	{
		// Token: 0x0600012C RID: 300 RVA: 0x000051B4 File Offset: 0x000033B4
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.BindDataDirectly(ref this.VoiceoverTriggers);
			BaseVoiceoverTrigger[] voiceoverTriggers = this.VoiceoverTriggers;
			for (int i = 0; i < voiceoverTriggers.Length; i++)
			{
				voiceoverTriggers[i].Initialize(currentGame);
			}
		}

		// Token: 0x04000084 RID: 132
		public BaseVoiceoverTrigger[] VoiceoverTriggers;
	}
}
