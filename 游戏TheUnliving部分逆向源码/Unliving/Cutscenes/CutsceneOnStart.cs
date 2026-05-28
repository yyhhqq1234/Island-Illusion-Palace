using System;

namespace Unliving.Cutscenes
{
	// Token: 0x0200031F RID: 799
	public class CutsceneOnStart : CutsceneBase
	{
		// Token: 0x06001AD6 RID: 6870 RVA: 0x0005463C File Offset: 0x0005283C
		public override void Start()
		{
			base.Start();
			base.StartInteractiveScene();
		}
	}
}
