using System;

namespace Unliving.GameSettings
{
	// Token: 0x020002A8 RID: 680
	public interface IGameSettingsManager
	{
		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x060017C5 RID: 6085
		GameSettingsState CurrentState { get; }

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x060017C6 RID: 6086
		GameSettingsState DefaultState { get; }

		// Token: 0x140000E5 RID: 229
		// (add) Token: 0x060017C7 RID: 6087
		// (remove) Token: 0x060017C8 RID: 6088
		event Action<GameSettingsState> CurrentSettingsStateChanged;

		// Token: 0x060017C9 RID: 6089
		void SaveCurrentGameSettings();

		// Token: 0x060017CA RID: 6090
		void SetResolution(int width, int height);

		// Token: 0x060017CB RID: 6091
		void SetFullscreen(bool fullscreen);

		// Token: 0x060017CC RID: 6092
		void SetVSync(bool vSync);

		// Token: 0x060017CD RID: 6093
		void SetCameraStretchFill(bool stretchFill);

		// Token: 0x060017CE RID: 6094
		void RestoreDefaultScreenSettings();

		// Token: 0x060017CF RID: 6095
		void SetMusicVolume(float volume);

		// Token: 0x060017D0 RID: 6096
		void SetSFXVolume(float volume);

		// Token: 0x060017D1 RID: 6097
		void RestoreDefaultAudioSettings();

		// Token: 0x060017D2 RID: 6098
		void SetCameraSpeed(float value);

		// Token: 0x060017D3 RID: 6099
		void SetSmartDashState(bool value);

		// Token: 0x060017D4 RID: 6100
		void SetCursorSensitivity(float value);

		// Token: 0x060017D5 RID: 6101
		void SetAimAssistType(int aimAssistMode);

		// Token: 0x060017D6 RID: 6102
		void SwitchAimAssistType(bool saveSettings);

		// Token: 0x060017D7 RID: 6103
		void RestoreDefaultInputSettings();
	}
}
