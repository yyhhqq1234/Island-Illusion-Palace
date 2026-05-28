using System;
using System.IO;
using Common;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using Game.InputManager;
using Game.Localization;
using Game.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving.GameSettings
{
	// Token: 0x020002A6 RID: 678
	[DefaultExecutionOrder(-100)]
	[CreateAssetMenu(fileName = "GameSettingsManager", menuName = "Game/Global/Game Settings Manager")]
	[Service(typeof(GameSettingsManager), new Type[]
	{
		typeof(IGameSettingsManager)
	})]
	public sealed class GameSettingsManager : GlobalManagerBase, IGameSettingsManager
	{
		// Token: 0x17000511 RID: 1297
		// (get) Token: 0x06001797 RID: 6039 RVA: 0x0004AFFF File Offset: 0x000491FF
		public GameSettingsState CurrentState
		{
			get
			{
				return this.currentState;
			}
		}

		// Token: 0x17000512 RID: 1298
		// (get) Token: 0x06001798 RID: 6040 RVA: 0x0004B007 File Offset: 0x00049207
		public GameSettingsState DefaultState
		{
			get
			{
				return this.defaultState;
			}
		}

		// Token: 0x17000513 RID: 1299
		// (get) Token: 0x06001799 RID: 6041 RVA: 0x0004B010 File Offset: 0x00049210
		public int CurrentSaveSlot
		{
			get
			{
				int? num = this.saveSlotOverride;
				if (num == null)
				{
					return this.currentState.saveSlotIndex;
				}
				return num.GetValueOrDefault();
			}
		}

		// Token: 0x17000514 RID: 1300
		// (get) Token: 0x0600179A RID: 6042 RVA: 0x0004B040 File Offset: 0x00049240
		public string CurrentPlayerProfileName
		{
			get
			{
				return this.GetSlotName(this.CurrentSaveSlot);
			}
		}

		// Token: 0x140000E4 RID: 228
		// (add) Token: 0x0600179B RID: 6043 RVA: 0x0004B050 File Offset: 0x00049250
		// (remove) Token: 0x0600179C RID: 6044 RVA: 0x0004B088 File Offset: 0x00049288
		public event Action<GameSettingsState> CurrentSettingsStateChanged;

		// Token: 0x0600179D RID: 6045 RVA: 0x0004B0C0 File Offset: 0x000492C0
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.currentState = this.LoadCurrentGameSettings();
			if (this.defaultState.screenData.width == 0)
			{
				this.defaultState.screenData.SetCurrentResolution();
				this.SaveDefaultGameSettings();
			}
			else
			{
				this.defaultState = this.LoadDefaultGameSettings();
			}
			if (this.currentState == null)
			{
				PlayerPrefs.DeleteAll();
				this.currentState = this.defaultState.Clone();
				this.currentState.screenData.SetCurrentResolution();
				this.SetVSync(this.currentState.screenData.vSync);
				this.SetFullscreen(this.currentState.screenData.fullscreen);
				this.SetCameraStretchFill(this.currentState.screenData.stretchFill);
			}
			else
			{
				this.SetScreenData(this.currentState.screenData);
			}
			if (base.CurrentGame.Services.TryGet<GameManager>(out this.gameManager))
			{
				this.SetFPSLimit();
			}
		}

		// Token: 0x0600179E RID: 6046 RVA: 0x0004B1B8 File Offset: 0x000493B8
		protected override void OnSceneLoaded(Scene scene)
		{
			base.OnSceneLoaded(scene);
			this.SetWWiseMasterVolume();
			this.SetCursorSensitivity(this.currentState.inputData.cursorSensitivity);
			this.SetFPSLimit();
			this.SetCameraStretchFill(this.currentState.screenData.stretchFill);
		}

		// Token: 0x0600179F RID: 6047 RVA: 0x0004B204 File Offset: 0x00049404
		private void ApplyPixelPerfectCameraSettings()
		{
			int width = this.currentState.screenData.width;
			int height = this.currentState.screenData.height;
			PixelPerfectCamera camera;
			if (this.TryGetPixelPerfectCamera(out camera))
			{
				for (int i = 0; i < this.pixelPerfectCameraSettings.Length; i++)
				{
					GameSettingsManager.PixelPerfectCameraSettings pixelPerfectCameraSettings = this.pixelPerfectCameraSettings[i];
					if (pixelPerfectCameraSettings.IsValidResolution(width, height))
					{
						pixelPerfectCameraSettings.ApplySettings(camera);
						return;
					}
				}
				this.defaultPixelPerfectCameraSettings.ApplySettings(camera);
			}
		}

		// Token: 0x060017A0 RID: 6048 RVA: 0x0004B280 File Offset: 0x00049480
		private bool TryGetPixelPerfectCamera(out PixelPerfectCamera pixelPerfectCamera)
		{
			Camera main = Camera.main;
			if (!main.IsNull() && main.TryGetComponent<PixelPerfectCamera>(out pixelPerfectCamera))
			{
				return true;
			}
			pixelPerfectCamera = null;
			return false;
		}

		// Token: 0x060017A1 RID: 6049 RVA: 0x0004B2AA File Offset: 0x000494AA
		private void SetFPSLimit()
		{
			if (this.gameManager.IsMainMenuLoaded)
			{
				Application.targetFrameRate = this.currentState.screenData.maxMenuFPS;
				return;
			}
			Application.targetFrameRate = this.currentState.screenData.maxGameplayFPS;
		}

		// Token: 0x060017A2 RID: 6050 RVA: 0x0004B2E4 File Offset: 0x000494E4
		private void SetWWiseMasterVolume()
		{
			this.SetMusicVolume(this.currentState.audioData.musicVolume);
			this.SetSFXVolume(this.currentState.audioData.sfxVolume);
		}

		// Token: 0x060017A3 RID: 6051 RVA: 0x0004B312 File Offset: 0x00049512
		public string GetSlotName(int index)
		{
			return string.Format("{0}{1}", "save_slot_", index);
		}

		// Token: 0x060017A4 RID: 6052 RVA: 0x0004B32C File Offset: 0x0004952C
		public void ClearSaveSlotData(int index)
		{
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				playerProfileManager.DeleteProfile(this.GetSlotName(index));
				if (index == this.CurrentSaveSlot)
				{
					playerProfileManager.LoadCurrentPlayerProfile();
				}
			}
		}

		// Token: 0x060017A5 RID: 6053 RVA: 0x0004B36C File Offset: 0x0004956C
		public void SelectSaveSlot(int index)
		{
			this.currentState.saveSlotIndex = index;
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				playerProfileManager.LoadCurrentPlayerProfile();
			}
		}

		// Token: 0x060017A6 RID: 6054 RVA: 0x0004B39F File Offset: 0x0004959F
		public void SelectOverrideSaveSlot(int index)
		{
			this.saveSlotOverride = new int?(index);
		}

		// Token: 0x060017A7 RID: 6055 RVA: 0x0004B3AD File Offset: 0x000495AD
		public void ResetOverrideSaveSlot()
		{
			this.saveSlotOverride = null;
		}

		// Token: 0x060017A8 RID: 6056 RVA: 0x0004B3BC File Offset: 0x000495BC
		public void RestoreDefaultAudioSettings()
		{
			this.currentState.audioData.SetData(this.defaultState.audioData);
			this.SetSFXVolume(this.currentState.audioData.sfxVolume);
			this.SetMusicVolume(this.currentState.audioData.musicVolume);
		}

		// Token: 0x060017A9 RID: 6057 RVA: 0x0004B410 File Offset: 0x00049610
		public void RestoreDefaultScreenSettings()
		{
			this.SetScreenData(this.defaultState.screenData);
		}

		// Token: 0x060017AA RID: 6058 RVA: 0x0004B424 File Offset: 0x00049624
		public void SetScreenData(GameSettingsState.ScreenData screenData)
		{
			this.currentState.screenData.SetData(screenData);
			this.SetVSync(this.currentState.screenData.vSync);
			this.SetResolution(screenData.width, screenData.height);
			this.SetCameraStretchFill(screenData.stretchFill);
		}

		// Token: 0x060017AB RID: 6059 RVA: 0x0004B478 File Offset: 0x00049678
		public void SetCameraStretchFill(bool stretchFill)
		{
			this.currentState.screenData.stretchFill = stretchFill;
			Camera main = Camera.main;
			PixelPerfectCamera pixelPerfectCamera;
			if (!main.IsNull() && main.TryGetComponent<PixelPerfectCamera>(out pixelPerfectCamera))
			{
				pixelPerfectCamera.stretchFill = stretchFill;
			}
			this.SetCameraSettingsDirty();
		}

		// Token: 0x060017AC RID: 6060 RVA: 0x0004B4BB File Offset: 0x000496BB
		public void SetFullscreen(bool fullscreen)
		{
			this.currentState.screenData.fullscreen = fullscreen;
			this.SetResolution(this.currentState.screenData.width, this.currentState.screenData.height);
			this.SetCameraSettingsDirty();
		}

		// Token: 0x060017AD RID: 6061 RVA: 0x0004B4FA File Offset: 0x000496FA
		public void SetVSync(bool vSync)
		{
			this.currentState.screenData.vSync = vSync;
			QualitySettings.vSyncCount = (vSync ? 1 : 0);
		}

		// Token: 0x060017AE RID: 6062 RVA: 0x0004B51C File Offset: 0x0004971C
		public void SetResolution(int width, int height)
		{
			this.currentState.screenData.width = width;
			this.currentState.screenData.height = height;
			Screen.SetResolution(width, height, this.currentState.screenData.fullscreen);
			this.ApplyPixelPerfectCameraSettings();
			this.SetCameraSettingsDirty();
		}

		// Token: 0x060017AF RID: 6063 RVA: 0x0004B56E File Offset: 0x0004976E
		public void SetSFXVolume(float volume)
		{
			this.currentState.audioData.sfxVolume = volume;
			AkSoundEngine.SetRTPCValue("volume_sfx", volume * 100f);
		}

		// Token: 0x060017B0 RID: 6064 RVA: 0x0004B593 File Offset: 0x00049793
		public void SetMusicVolume(float volume)
		{
			this.currentState.audioData.musicVolume = volume;
			AkSoundEngine.SetRTPCValue("volume_music", volume * 100f);
		}

		// Token: 0x060017B1 RID: 6065 RVA: 0x0004B5B8 File Offset: 0x000497B8
		public void RestoreDefaultInputSettings()
		{
			this.currentState.inputData.SetData(this.defaultState.inputData);
		}

		// Token: 0x060017B2 RID: 6066 RVA: 0x0004B5D5 File Offset: 0x000497D5
		public void SetAimAssistType(int aimAssistType)
		{
			this.currentState.inputData.aimAssistType = aimAssistType;
		}

		// Token: 0x060017B3 RID: 6067 RVA: 0x0004B5E8 File Offset: 0x000497E8
		public void SwitchAimAssistType(bool saveSettings)
		{
			int num = this.currentState.inputData.aimAssistType + 1;
			if (num >= Enum.GetValues(typeof(AimAssistType)).Length)
			{
				num = 0;
			}
			this.SetAimAssistType(num);
			if (saveSettings)
			{
				this.SaveCurrentGameSettings();
			}
		}

		// Token: 0x060017B4 RID: 6068 RVA: 0x0004B631 File Offset: 0x00049831
		public void SetSmartDashState(bool value)
		{
			this.currentState.inputData.smartDash = value;
		}

		// Token: 0x060017B5 RID: 6069 RVA: 0x0004B644 File Offset: 0x00049844
		public void SetCameraSpeed(float value)
		{
			this.currentState.inputData.dynamicCameraSpeed = value;
		}

		// Token: 0x060017B6 RID: 6070 RVA: 0x0004B658 File Offset: 0x00049858
		public void SetCursorSensitivity(float value)
		{
			value = Mathf.Max(value, 0.05f);
			this.currentState.inputData.cursorSensitivity = value;
			IInputManager inputManager;
			if (base.CurrentGame.Services.TryGet<IInputManager>(out inputManager))
			{
				inputManager.SetPointerSpeed(value);
			}
		}

		// Token: 0x060017B7 RID: 6071 RVA: 0x0004B6A0 File Offset: 0x000498A0
		public void ChangeLanguage(SystemLanguage language)
		{
			if (this.localizationManager == null)
			{
				this.localizationManager = base.CurrentGame.Services.Get<LocalizationManager>();
			}
			if (this.localizationManager == null)
			{
				Debug.LogWarning("There's no Localization Manager founded. Can't change language.");
				return;
			}
			this.localizationManager.ChangeLanguage(language);
		}

		// Token: 0x060017B8 RID: 6072 RVA: 0x0004B6F6 File Offset: 0x000498F6
		public void SaveCurrentGameSettings()
		{
			this.SaveGameSettings(this.GetSettingsSavePath(), this.currentState);
			Action<GameSettingsState> currentSettingsStateChanged = this.CurrentSettingsStateChanged;
			if (currentSettingsStateChanged == null)
			{
				return;
			}
			currentSettingsStateChanged(this.currentState);
		}

		// Token: 0x060017B9 RID: 6073 RVA: 0x0004B720 File Offset: 0x00049920
		private void SaveDefaultGameSettings()
		{
			this.SaveGameSettings(this.GetDefaultSettingsSavePath(), this.defaultState);
		}

		// Token: 0x060017BA RID: 6074 RVA: 0x0004B734 File Offset: 0x00049934
		private void SaveGameSettings(string savePath, GameSettingsState settingsState)
		{
			string directoryName = Path.GetDirectoryName(savePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (GameApplicationSettings.UseBinarySerialization())
			{
				UnitySerializationUtility.ToBsonFile(settingsState, savePath);
				return;
			}
			UnitySerializationUtility.ToJsonFile(settingsState, savePath);
		}

		// Token: 0x060017BB RID: 6075 RVA: 0x0004B76D File Offset: 0x0004996D
		private GameSettingsState LoadCurrentGameSettings()
		{
			return this.LoadGameSettings(this.GetSettingsSavePath());
		}

		// Token: 0x060017BC RID: 6076 RVA: 0x0004B77B File Offset: 0x0004997B
		private GameSettingsState LoadDefaultGameSettings()
		{
			return this.LoadGameSettings(this.GetDefaultSettingsSavePath());
		}

		// Token: 0x060017BD RID: 6077 RVA: 0x0004B78C File Offset: 0x0004998C
		private GameSettingsState LoadGameSettings(string savePath)
		{
			if (!File.Exists(savePath))
			{
				return null;
			}
			GameSettingsState result;
			try
			{
				if (GameApplicationSettings.UseBinarySerialization())
				{
					result = UnitySerializationUtility.FromBsonFile<GameSettingsState>(savePath);
				}
				else
				{
					result = UnitySerializationUtility.FromJsonFile<GameSettingsState>(savePath);
				}
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		// Token: 0x060017BE RID: 6078 RVA: 0x0004B7D4 File Offset: 0x000499D4
		private string GetSettingsSavePath()
		{
			return GameApplication.GetDataPath(Path.Combine("Saves/Options", "options" + GameApplicationSettings.GetSerializationFileExtension()));
		}

		// Token: 0x060017BF RID: 6079 RVA: 0x0004B7F4 File Offset: 0x000499F4
		private string GetDefaultSettingsSavePath()
		{
			return GameApplication.GetDataPath(Path.Combine("Saves/Options", "options_default" + GameApplicationSettings.GetSerializationFileExtension()));
		}

		// Token: 0x060017C0 RID: 6080 RVA: 0x0004B814 File Offset: 0x00049A14
		private void SetCameraSettingsDirty()
		{
			Camera main = Camera.main;
			if (!main.IsNull())
			{
				main.TryGetComponent<PixelPerfectCameraEventsHandler>(out this.cameraEventsHandler);
			}
			if (!this.cameraEventsHandler.IsNull())
			{
				this.cameraEventsHandler.SetCameraSettingsDirty();
			}
		}

		// Token: 0x060017C1 RID: 6081 RVA: 0x0004B854 File Offset: 0x00049A54
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.SaveCurrentGameSettings();
		}

		// Token: 0x04000D8A RID: 3466
		private const string SaveSlotNamePrefix = "save_slot_";

		// Token: 0x04000D8C RID: 3468
		[SerializeField]
		private GameSettingsState defaultState;

		// Token: 0x04000D8D RID: 3469
		[SerializeField]
		private GameSettingsManager.PixelPerfectCameraSettings defaultPixelPerfectCameraSettings;

		// Token: 0x04000D8E RID: 3470
		[SerializeField]
		private GameSettingsManager.PixelPerfectCameraSettings[] pixelPerfectCameraSettings;

		// Token: 0x04000D8F RID: 3471
		private LocalizationManager localizationManager;

		// Token: 0x04000D90 RID: 3472
		private GameSettingsState currentState;

		// Token: 0x04000D91 RID: 3473
		private int? saveSlotOverride;

		// Token: 0x04000D92 RID: 3474
		private GameManager gameManager;

		// Token: 0x04000D93 RID: 3475
		private PixelPerfectCameraEventsHandler cameraEventsHandler;

		// Token: 0x0200051F RID: 1311
		[Serializable]
		public struct PixelPerfectCameraSettings
		{
			// Token: 0x06002634 RID: 9780 RVA: 0x00077B93 File Offset: 0x00075D93
			public void ApplySettings(PixelPerfectCamera camera)
			{
				camera.upscaleRT = this.upscaleRT;
				camera.cropFrameX = this.cropFrameX;
				camera.cropFrameY = this.cropFrameY;
				camera.stretchFill = this.stretchFill;
			}

			// Token: 0x06002635 RID: 9781 RVA: 0x00077BC5 File Offset: 0x00075DC5
			public bool IsValidResolution(int width, int height)
			{
				return this.resolution.x == width && this.resolution.y == height;
			}

			// Token: 0x04001B26 RID: 6950
			public Vector2Int resolution;

			// Token: 0x04001B27 RID: 6951
			public bool upscaleRT;

			// Token: 0x04001B28 RID: 6952
			public bool cropFrameX;

			// Token: 0x04001B29 RID: 6953
			public bool cropFrameY;

			// Token: 0x04001B2A RID: 6954
			public bool stretchFill;
		}
	}
}
