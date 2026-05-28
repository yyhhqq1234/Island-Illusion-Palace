using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000327 RID: 807
	[Serializable]
	public sealed class CameraShakeBehaviour : PlayableBehaviour
	{
		// Token: 0x06001AF6 RID: 6902 RVA: 0x000550A4 File Offset: 0x000532A4
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (this.firstFrame)
			{
				CinemachineImpulseSource cinemachineImpulseSource = playerData as CinemachineImpulseSource;
				if (cinemachineImpulseSource == null)
				{
					string str = "[failed to grab the name]";
					PlayableDirector playableDirector = playable.GetGraph<Playable>().GetResolver() as PlayableDirector;
					if (playableDirector)
					{
						str = playableDirector.gameObject.name;
					}
					Debug.LogError("Screenshake track has no source bound to it for cutscene '" + str + "'");
				}
				CinemachineImpulseDefinition impulseDefinition = cinemachineImpulseSource.m_ImpulseDefinition;
				impulseDefinition.m_AmplitudeGain = this.amplitudeGain;
				impulseDefinition.m_FrequencyGain = this.frequencyGain;
				impulseDefinition.m_TimeEnvelope.m_AttackTime = this.frameDuration * 2f;
				impulseDefinition.m_TimeEnvelope.m_SustainTime = this.frameDuration * 2f;
				impulseDefinition.m_TimeEnvelope.m_DecayTime = (float)playable.GetDuration<Playable>() - impulseDefinition.m_TimeEnvelope.m_AttackTime - impulseDefinition.m_TimeEnvelope.m_SustainTime;
				cinemachineImpulseSource.GenerateImpulse();
				this.firstFrame = false;
			}
		}

		// Token: 0x04000F1E RID: 3870
		[SerializeField]
		private float amplitudeGain = 1f;

		// Token: 0x04000F1F RID: 3871
		[SerializeField]
		private float frequencyGain = 1f;

		// Token: 0x04000F20 RID: 3872
		public float frameDuration = 0.042f;

		// Token: 0x04000F21 RID: 3873
		private bool firstFrame = true;
	}
}
