using System;
using Game.Core;

namespace Unliving.Tutorial
{
	// Token: 0x0200002D RID: 45
	public interface ITutorialHint
	{
		// Token: 0x1400001B RID: 27
		// (add) Token: 0x0600018B RID: 395
		// (remove) Token: 0x0600018C RID: 396
		event Action<ITutorialHint> HintTriggersReached;

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x0600018D RID: 397
		string ID { get; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600018E RID: 398
		int Priority { get; }

		// Token: 0x0600018F RID: 399
		void OnSceneLoaded(IGame game);

		// Token: 0x06000190 RID: 400
		void UpdateState();

		// Token: 0x06000191 RID: 401
		TutorialHintSerializationData GetSerializationData();

		// Token: 0x06000192 RID: 402
		void SetSerializationData(TutorialHintSerializationData data);
	}
}
