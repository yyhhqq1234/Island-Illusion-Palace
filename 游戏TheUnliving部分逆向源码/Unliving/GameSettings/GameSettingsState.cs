using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common;
using UnityEngine;

namespace Unliving.GameSettings
{
	// Token: 0x020002A7 RID: 679
	[Serializable]
	public sealed class GameSettingsState : ICloneable<GameSettingsState>
	{
		// Token: 0x060017C3 RID: 6083 RVA: 0x0004B86C File Offset: 0x00049A6C
		public GameSettingsState Clone()
		{
			return new GameSettingsState
			{
				saveSlotIndex = this.saveSlotIndex,
				screenData = this.screenData.Clone(),
				audioData = this.audioData.Clone(),
				inputData = this.inputData.Clone()
			};
		}

		// Token: 0x04000D94 RID: 3476
		public int saveSlotIndex;

		// Token: 0x04000D95 RID: 3477
		public GameSettingsState.ScreenData screenData;

		// Token: 0x04000D96 RID: 3478
		public GameSettingsState.AudioData audioData;

		// Token: 0x04000D97 RID: 3479
		[OptionalField]
		public GameSettingsState.InputData inputData;

		// Token: 0x02000520 RID: 1312
		[Serializable]
		public class ScreenData : ICloneable<GameSettingsState.ScreenData>
		{
			// Token: 0x06002636 RID: 9782 RVA: 0x00077BE8 File Offset: 0x00075DE8
			public void SetData(GameSettingsState.ScreenData data)
			{
				this.maxMenuFPS = data.maxMenuFPS;
				this.maxGameplayFPS = data.maxGameplayFPS;
				this.minResolutionWidth = data.minResolutionWidth;
				this.minResolutionHeight = data.minResolutionHeight;
				this.width = data.width;
				this.height = data.height;
				this.fullscreen = data.fullscreen;
				this.vSync = data.vSync;
				this.stretchFill = data.stretchFill;
			}

			// Token: 0x06002637 RID: 9783 RVA: 0x00077C61 File Offset: 0x00075E61
			public void SetCurrentResolution()
			{
				this.width = Screen.width;
				this.height = Screen.height;
			}

			// Token: 0x06002638 RID: 9784 RVA: 0x00077C79 File Offset: 0x00075E79
			public GameSettingsState.ScreenData Clone()
			{
				return (GameSettingsState.ScreenData)base.MemberwiseClone();
			}

			// Token: 0x04001B2B RID: 6955
			[Tooltip("Максимум FPS в меню (-1 - без ограничений)")]
			public int maxMenuFPS;

			// Token: 0x04001B2C RID: 6956
			[Tooltip("Максимум FPS в игровых сценах (-1 - без ограничений)")]
			public int maxGameplayFPS;

			// Token: 0x04001B2D RID: 6957
			[Tooltip("Минимальное разрешение для запуска игры")]
			public int minResolutionWidth;

			// Token: 0x04001B2E RID: 6958
			public int minResolutionHeight;

			// Token: 0x04001B2F RID: 6959
			public int width;

			// Token: 0x04001B30 RID: 6960
			public int height;

			// Token: 0x04001B31 RID: 6961
			public bool fullscreen;

			// Token: 0x04001B32 RID: 6962
			public bool vSync;

			// Token: 0x04001B33 RID: 6963
			public bool stretchFill;
		}

		// Token: 0x02000521 RID: 1313
		[Serializable]
		public class AudioData : ICloneable<GameSettingsState.AudioData>
		{
			// Token: 0x0600263A RID: 9786 RVA: 0x00077C8E File Offset: 0x00075E8E
			public void SetData(GameSettingsState.AudioData data)
			{
				this.musicVolume = data.musicVolume;
				this.sfxVolume = data.sfxVolume;
			}

			// Token: 0x0600263B RID: 9787 RVA: 0x00077CA8 File Offset: 0x00075EA8
			public GameSettingsState.AudioData Clone()
			{
				return (GameSettingsState.AudioData)base.MemberwiseClone();
			}

			// Token: 0x04001B34 RID: 6964
			public float musicVolume;

			// Token: 0x04001B35 RID: 6965
			public float sfxVolume;
		}

		// Token: 0x02000522 RID: 1314
		[Serializable]
		public class InputData : ICloneable<GameSettingsState.InputData>
		{
			// Token: 0x0600263D RID: 9789 RVA: 0x00077CBD File Offset: 0x00075EBD
			public void SetData(GameSettingsState.InputData data)
			{
				this.cursorSensitivity = data.cursorSensitivity;
				this.smartDash = data.smartDash;
				this.controllersMappingData = data.controllersMappingData;
				this.dynamicCameraSpeed = data.dynamicCameraSpeed;
				this.aimAssistType = data.aimAssistType;
			}

			// Token: 0x0600263E RID: 9790 RVA: 0x00077CFC File Offset: 0x00075EFC
			public void AddMappingData(string controllerHardwareIdentifier, int categoryID, int layoutID, string mappingData)
			{
				GameSettingsState.InputData.ControllerMappingData controllerMappingData = new GameSettingsState.InputData.ControllerMappingData
				{
					controllerHardwareIdentifier = controllerHardwareIdentifier,
					data = mappingData,
					categoryID = categoryID,
					layoutID = layoutID
				};
				for (int i = 0; i < this.controllersMappingData.Count; i++)
				{
					GameSettingsState.InputData.ControllerMappingData controllerMappingData2 = this.controllersMappingData[i];
					if (string.Equals(controllerMappingData2.controllerHardwareIdentifier, controllerHardwareIdentifier, StringComparison.InvariantCultureIgnoreCase) && controllerMappingData2.categoryID == categoryID && controllerMappingData2.layoutID == layoutID)
					{
						this.controllersMappingData[i] = controllerMappingData;
						return;
					}
				}
				this.controllersMappingData.Add(controllerMappingData);
			}

			// Token: 0x0600263F RID: 9791 RVA: 0x00077D94 File Offset: 0x00075F94
			public string GetMappingData(string controllerHardwareIdentifier, int categoryID, int layoutID)
			{
				for (int i = 0; i < this.controllersMappingData.Count; i++)
				{
					GameSettingsState.InputData.ControllerMappingData controllerMappingData = this.controllersMappingData[i];
					if (string.Equals(controllerMappingData.controllerHardwareIdentifier, controllerHardwareIdentifier, StringComparison.InvariantCultureIgnoreCase) && controllerMappingData.categoryID == categoryID && controllerMappingData.layoutID == layoutID)
					{
						return controllerMappingData.data;
					}
				}
				return string.Empty;
			}

			// Token: 0x06002640 RID: 9792 RVA: 0x00077DF1 File Offset: 0x00075FF1
			public void ResetMappingData()
			{
				this.controllersMappingData.Clear();
			}

			// Token: 0x06002641 RID: 9793 RVA: 0x00077DFE File Offset: 0x00075FFE
			public GameSettingsState.InputData Clone()
			{
				return (GameSettingsState.InputData)base.MemberwiseClone();
			}

			// Token: 0x04001B36 RID: 6966
			public float cursorSensitivity;

			// Token: 0x04001B37 RID: 6967
			public bool smartDash;

			// Token: 0x04001B38 RID: 6968
			public float dynamicCameraSpeed = 2.2f;

			// Token: 0x04001B39 RID: 6969
			[OptionalField]
			public int aimAssistType;

			// Token: 0x04001B3A RID: 6970
			[SerializeField]
			[HideInInspector]
			private List<GameSettingsState.InputData.ControllerMappingData> controllersMappingData = new List<GameSettingsState.InputData.ControllerMappingData>();

			// Token: 0x020005B6 RID: 1462
			[Serializable]
			public struct ControllerMappingData
			{
				// Token: 0x04001D39 RID: 7481
				public string controllerHardwareIdentifier;

				// Token: 0x04001D3A RID: 7482
				public int categoryID;

				// Token: 0x04001D3B RID: 7483
				public int layoutID;

				// Token: 0x04001D3C RID: 7484
				public string data;
			}
		}
	}
}
