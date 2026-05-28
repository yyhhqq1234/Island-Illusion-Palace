using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.ServiceRegistry;
using Game.Core;
using Game.InputManager;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using Unliving.GameSettings;

namespace Unliving.InputManager
{
	// Token: 0x020002A1 RID: 673
	[DefaultExecutionOrder(-100)]
	[Service(typeof(RewiredInputManager), new Type[]
	{
		typeof(IInputManager),
		typeof(InputManagerBase)
	})]
	public sealed class RewiredInputManager : InputManagerBase
	{
		// Token: 0x140000DD RID: 221
		// (add) Token: 0x0600174A RID: 5962 RVA: 0x00049BA8 File Offset: 0x00047DA8
		// (remove) Token: 0x0600174B RID: 5963 RVA: 0x00049BE0 File Offset: 0x00047DE0
		public override event Action ActiveControllerChanged;

		// Token: 0x140000DE RID: 222
		// (add) Token: 0x0600174C RID: 5964 RVA: 0x00049C18 File Offset: 0x00047E18
		// (remove) Token: 0x0600174D RID: 5965 RVA: 0x00049C50 File Offset: 0x00047E50
		public override event Action<bool> PointerActiveStateChanged;

		// Token: 0x140000DF RID: 223
		// (add) Token: 0x0600174E RID: 5966 RVA: 0x00049C88 File Offset: 0x00047E88
		// (remove) Token: 0x0600174F RID: 5967 RVA: 0x00049CC0 File Offset: 0x00047EC0
		public override event Action<Vector2> PointerPositionChanged;

		// Token: 0x140000E0 RID: 224
		// (add) Token: 0x06001750 RID: 5968 RVA: 0x00049CF8 File Offset: 0x00047EF8
		// (remove) Token: 0x06001751 RID: 5969 RVA: 0x00049D30 File Offset: 0x00047F30
		public override event Action<int, float> AxisValueChanged;

		// Token: 0x140000E1 RID: 225
		// (add) Token: 0x06001752 RID: 5970 RVA: 0x00049D68 File Offset: 0x00047F68
		// (remove) Token: 0x06001753 RID: 5971 RVA: 0x00049DA0 File Offset: 0x00047FA0
		public override event Action<int> ButtonDown;

		// Token: 0x140000E2 RID: 226
		// (add) Token: 0x06001754 RID: 5972 RVA: 0x00049DD8 File Offset: 0x00047FD8
		// (remove) Token: 0x06001755 RID: 5973 RVA: 0x00049E10 File Offset: 0x00048010
		public override event Action<int> ButtonUp;

		// Token: 0x140000E3 RID: 227
		// (add) Token: 0x06001756 RID: 5974 RVA: 0x00049E48 File Offset: 0x00048048
		// (remove) Token: 0x06001757 RID: 5975 RVA: 0x00049E80 File Offset: 0x00048080
		public override event Action<int> ButtonClicked;

		// Token: 0x1700050D RID: 1293
		// (get) Token: 0x06001758 RID: 5976 RVA: 0x00049EB5 File Offset: 0x000480B5
		public override InputControllerType ActiveControllerType
		{
			get
			{
				return this.lastActiveControllerType;
			}
		}

		// Token: 0x1700050E RID: 1294
		// (get) Token: 0x06001759 RID: 5977 RVA: 0x00049EC0 File Offset: 0x000480C0
		public Player RewiredPlayer
		{
			get
			{
				if (this == null || this.rewiredPlayerUnsafe == null || !this.rewiredPlayerUnsafe.isPlaying)
				{
					this.rewiredPlayerUnsafe = ReInput.players.GetPlayer(this.data.rewiredPlayerID);
				}
				return this.rewiredPlayerUnsafe;
			}
		}

		// Token: 0x0600175A RID: 5978 RVA: 0x00049F0C File Offset: 0x0004810C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.BindDataDirectly(ref this.data);
			currentGame.Services.TryGet<IGameSettingsManager>(out this.settingsManager);
			this.mainCamera = Camera.main;
			this.inputModule = base.GetComponentInChildren<RewiredStandaloneInputModule>();
			this.activeController = this.RewiredPlayer.controllers.GetLastActiveController();
			this.RewiredPlayer.controllers.AddLastActiveControllerChangedDelegate(new PlayerActiveControllerChangedDelegate(this.OnPlayerActiveControllerChanged));
			this.RewiredPlayer.AddInputEventDelegate(new Action<InputActionEventData>(this.OnRewiredPlayerAxisValueChanged), UpdateLoopType.Update, InputActionEventType.AxisActive);
			this.RewiredPlayer.AddInputEventDelegate(new Action<InputActionEventData>(this.OnRewiredPlayerButtonPressed), UpdateLoopType.Update, InputActionEventType.ButtonJustPressed);
			this.RewiredPlayer.AddInputEventDelegate(new Action<InputActionEventData>(this.OnRewiredPlayerButtonReleased), UpdateLoopType.Update, InputActionEventType.ButtonJustReleased);
			this.RewiredPlayer.AddInputEventDelegate(new Action<InputActionEventData>(this.OnRewiredPlayerButtonClicked), UpdateLoopType.Update, InputActionEventType.ButtonJustSinglePressed);
			this.UpdateActiveControllerType();
			this.CreatePlayerMouse();
			this.controllerMapStore = new RewiredUserDataStore(currentGame);
			foreach (Controller controller in this.RewiredPlayer.controllers.Controllers)
			{
				this.LoadControllerData(controller);
			}
		}

		// Token: 0x0600175B RID: 5979 RVA: 0x0004A050 File Offset: 0x00048250
		private void OnRewiredPlayerButtonClicked(InputActionEventData eventData)
		{
			Action<int> buttonClicked = this.ButtonClicked;
			if (buttonClicked == null)
			{
				return;
			}
			buttonClicked(eventData.actionId);
		}

		// Token: 0x0600175C RID: 5980 RVA: 0x0004A068 File Offset: 0x00048268
		private void OnRewiredPlayerButtonPressed(InputActionEventData eventData)
		{
			Action<int> buttonDown = this.ButtonDown;
			if (buttonDown == null)
			{
				return;
			}
			buttonDown(eventData.actionId);
		}

		// Token: 0x0600175D RID: 5981 RVA: 0x0004A080 File Offset: 0x00048280
		private void OnRewiredPlayerButtonReleased(InputActionEventData eventData)
		{
			Action<int> buttonUp = this.ButtonUp;
			if (buttonUp == null)
			{
				return;
			}
			buttonUp(eventData.actionId);
		}

		// Token: 0x0600175E RID: 5982 RVA: 0x0004A098 File Offset: 0x00048298
		private void OnRewiredPlayerAxisValueChanged(InputActionEventData eventData)
		{
			Action<int, float> axisValueChanged = this.AxisValueChanged;
			if (axisValueChanged == null)
			{
				return;
			}
			axisValueChanged(eventData.actionId, eventData.GetAxis());
		}

		// Token: 0x0600175F RID: 5983 RVA: 0x0004A0B7 File Offset: 0x000482B7
		public Controller GetActiveController()
		{
			return this.activeController;
		}

		// Token: 0x06001760 RID: 5984 RVA: 0x0004A0C0 File Offset: 0x000482C0
		public override void SetPointerSpeed(float speed)
		{
			this.currentPointerSpeed = speed;
			if (this.playerMouse == null)
			{
				return;
			}
			ControllerTypeSettings controllerTypeSettings;
			if (this.data.TryGetControllerSettings(this.ActiveControllerType, out controllerTypeSettings))
			{
				this.playerMouse.pointerSpeed = speed * controllerTypeSettings.cursorSpeedMultiplier;
			}
		}

		// Token: 0x06001761 RID: 5985 RVA: 0x0004A105 File Offset: 0x00048305
		public override void ChangePointerScreenPosition(Vector2 position)
		{
			this.OnCursorScreenPositionChanged(position);
		}

		// Token: 0x06001762 RID: 5986 RVA: 0x0004A10E File Offset: 0x0004830E
		public override Ray GetScreenPointRay()
		{
			return this.mainCamera.ScreenPointToRay(this.playerMouse.screenPosition);
		}

		// Token: 0x06001763 RID: 5987 RVA: 0x0004A12C File Offset: 0x0004832C
		public override Vector2 GetScreenCursorPosition()
		{
			if (this.playerMouse == null)
			{
				return default(Vector2);
			}
			return this.playerMouse.screenPosition;
		}

		// Token: 0x06001764 RID: 5988 RVA: 0x0004A158 File Offset: 0x00048358
		public override Vector2 GetWorldCursorPosition()
		{
			if (this.playerMouse == null)
			{
				return default(Vector2);
			}
			return this.mainCamera.ScreenToWorldPoint(this.playerMouse.screenPosition);
		}

		// Token: 0x06001765 RID: 5989 RVA: 0x0004A198 File Offset: 0x00048398
		public override bool TryGetControllerName(InputControllerType controllerType, out string controllerName)
		{
			controllerName = string.Empty;
			for (int i = 0; i < ReInput.controllers.Controllers.Count; i++)
			{
				Controller controller = ReInput.controllers.Controllers[i];
				if (controller.type == (ControllerType)controllerType)
				{
					controllerName = controller.name;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001766 RID: 5990 RVA: 0x0004A1EC File Offset: 0x000483EC
		public override bool TryGetElementName(InputElement inputElement, out string positiveName, out string negativeName)
		{
			ActionElementMap actionElementMap;
			ActionElementMap actionElementMap2;
			if (this.TryGetElementMap(inputElement, out actionElementMap, out actionElementMap2))
			{
				positiveName = ((actionElementMap != null) ? actionElementMap.elementIdentifierName : null);
				negativeName = ((actionElementMap2 != null) ? actionElementMap2.elementIdentifierName : null);
				return true;
			}
			positiveName = null;
			negativeName = null;
			return false;
		}

		// Token: 0x06001767 RID: 5991 RVA: 0x0004A22C File Offset: 0x0004842C
		public void LoadControllerData(Controller controller)
		{
			IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
			for (int i = 0; i < mapCategories.Count; i++)
			{
				InputMapCategory inputMapCategory = mapCategories[i];
				if (inputMapCategory.userAssignable)
				{
					IList<InputLayout> list = ReInput.mapping.MapLayouts(controller.type);
					for (int j = 0; j < list.Count; j++)
					{
						InputLayout inputLayout = list[j];
						ControllerMap controllerMap = this.controllerMapStore.LoadControllerMap(this.RewiredPlayer.id, controller.identifier, inputMapCategory.id, inputLayout.id);
						if (controllerMap != null)
						{
							this.RewiredPlayer.controllers.maps.AddMap(controller, controllerMap);
						}
					}
				}
			}
		}

		// Token: 0x06001768 RID: 5992 RVA: 0x0004A2E4 File Offset: 0x000484E4
		public IList<ActionElementMap> GetAllElementMaps()
		{
			List<ActionElementMap> list = new List<ActionElementMap>();
			bool flag = this.IsKeyboardController((ControllerType)this.ActiveControllerType);
			foreach (ControllerMap controllerMap in this.RewiredPlayer.controllers.maps.GetAllMapsInCategory(0))
			{
				if (flag == this.IsKeyboardController(controllerMap.controllerType))
				{
					list.AddRange(controllerMap.AllMaps);
				}
			}
			return list;
		}

		// Token: 0x06001769 RID: 5993 RVA: 0x0004A36C File Offset: 0x0004856C
		public bool TryGetAnyButtonDown(out ControllerPollingInfo buttonInfo, out Controller controller)
		{
			buttonInfo = default(ControllerPollingInfo);
			controller = null;
			if (this.activeController == null)
			{
				return false;
			}
			if (this.activeController.type == ControllerType.Joystick)
			{
				buttonInfo = ReInput.controllers.polling.PollControllerForFirstElementDown(ControllerType.Joystick, this.activeController.id);
				controller = this.activeController;
				return buttonInfo.success;
			}
			for (int i = 0; i < ReInput.controllers.Controllers.Count; i++)
			{
				controller = ReInput.controllers.Controllers[i];
				if (this.IsKeyboardController(controller))
				{
					buttonInfo = controller.PollForFirstElementDown();
					if (buttonInfo.success)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600176A RID: 5994 RVA: 0x0004A419 File Offset: 0x00048619
		public RewiredInputManager.ButtonRemappingResult SwapInputElementsMapping(InputElement inputElement1, InputElement inputElement2)
		{
			return this.SwapInputElementsMapping(new InputElement[]
			{
				inputElement1
			}, new InputElement[]
			{
				inputElement2
			});
		}

		// Token: 0x0600176B RID: 5995 RVA: 0x0004A440 File Offset: 0x00048640
		public RewiredInputManager.ButtonRemappingResult SwapInputElementsMapping(IList<InputElement> inputElements1, IList<InputElement> inputElements2)
		{
			if (inputElements1 == null || inputElements1.Count == 0)
			{
				return RewiredInputManager.ButtonRemappingResult.InvalidKey;
			}
			if (inputElements2 == null || inputElements2.Count == 0)
			{
				return RewiredInputManager.ButtonRemappingResult.InvalidKey;
			}
			InputElement inputElement = inputElements1[0];
			InputElement inputElement2 = inputElements2[0];
			int elementIdentifierId;
			int controllerId;
			InputElementType elementType;
			KeyCode keyCode;
			int elementIdentifierId2;
			int controllerId2;
			InputElementType elementType2;
			KeyCode keyCode2;
			if (this.TryGetActionElementID(inputElement.actionID, inputElement.axisContribution, out elementIdentifierId, out controllerId, out elementType, out keyCode) && this.TryGetActionElementID(inputElement2.actionID, inputElement2.axisContribution, out elementIdentifierId2, out controllerId2, out elementType2, out keyCode2))
			{
				this.DeleteElementsFromMaps(inputElements1);
				this.DeleteElementsFromMaps(inputElements2);
				foreach (InputElement inputElement3 in inputElements1)
				{
					inputElement3.controllerType = inputElement2.controllerType;
					this.AssignElementsToDefaultMap(inputElement3, controllerId2, (ControllerElementType)elementType2, elementIdentifierId2, keyCode2);
				}
				foreach (InputElement inputElement4 in inputElements2)
				{
					inputElement4.controllerType = inputElement.controllerType;
					this.AssignElementsToDefaultMap(inputElement4, controllerId, (ControllerElementType)elementType, elementIdentifierId, keyCode);
				}
				Action activeControllerChanged = this.ActiveControllerChanged;
				if (activeControllerChanged != null)
				{
					activeControllerChanged();
				}
				return RewiredInputManager.ButtonRemappingResult.Success;
			}
			return RewiredInputManager.ButtonRemappingResult.ButtonAssignError;
		}

		// Token: 0x0600176C RID: 5996 RVA: 0x0004A584 File Offset: 0x00048784
		public RewiredInputManager.ButtonRemappingResult TryRemapAction(IList<InputElement> inputElements, ControllerPollingInfo info, Controller controller, out InputElement[] conflictElements)
		{
			if (!this.IsValidButton(info))
			{
				conflictElements = null;
				return RewiredInputManager.ButtonRemappingResult.InvalidKey;
			}
			IList<ActionElementMap> allElementMaps = this.GetAllElementMaps();
			conflictElements = (from m in allElementMaps
			where base.<TryRemapAction>g__IsConflictElement|0(m)
			select new InputElement(m.actionId, (InputAxisContribution)m.axisContribution, (InputControllerType)m.controllerMap.controllerType)).ToArray<InputElement>();
			if (conflictElements.Length != 0)
			{
				return RewiredInputManager.ButtonRemappingResult.AlreadyAssigned;
			}
			this.DeleteElementsFromMaps(inputElements);
			for (int i = 0; i < inputElements.Count; i++)
			{
				InputElement inputElement = inputElements[i];
				inputElement.controllerType = (InputControllerType)controller.type;
				this.AssignElementsToDefaultMap(inputElement, controller.id, info.elementType, info.elementIdentifierId, info.keyboardKey);
			}
			Action activeControllerChanged = this.ActiveControllerChanged;
			if (activeControllerChanged != null)
			{
				activeControllerChanged();
			}
			return RewiredInputManager.ButtonRemappingResult.Success;
		}

		// Token: 0x0600176D RID: 5997 RVA: 0x0004A684 File Offset: 0x00048884
		private bool AssignElementsToDefaultMap(InputElement inputElement, int controllerId, ControllerElementType elementType, int elementIdentifierId, KeyCode keyCode)
		{
			ElementAssignment elementAssignment = default(ElementAssignment);
			elementAssignment.axisContribution = (Pole)inputElement.axisContribution;
			elementAssignment.actionId = inputElement.actionID;
			elementAssignment.type = ((elementType == ControllerElementType.Button) ? ElementAssignmentType.Button : ElementAssignmentType.FullAxis);
			elementAssignment.modifierKeyFlags = ModifierKeyFlags.None;
			elementAssignment.elementIdentifierId = elementIdentifierId;
			if (inputElement.controllerType == InputControllerType.Keyboard)
			{
				elementAssignment.keyboardKey = keyCode;
			}
			ControllerMap firstMapInCategory = this.RewiredPlayer.controllers.maps.GetFirstMapInCategory((ControllerType)inputElement.controllerType, controllerId, 0);
			if (firstMapInCategory.ReplaceOrCreateElementMap(elementAssignment))
			{
				this.controllerMapStore.SaveControllerMap(this.RewiredPlayer.id, firstMapInCategory);
				return true;
			}
			return false;
		}

		// Token: 0x0600176E RID: 5998 RVA: 0x0004A728 File Offset: 0x00048928
		private void DeleteElementsFromMaps(IList<InputElement> inputElements)
		{
			for (int i = 0; i < inputElements.Count; i++)
			{
				InputElement inputElement = inputElements[i];
				int elementMapsWithAction = this.RewiredPlayer.controllers.maps.GetElementMapsWithAction((ControllerType)inputElement.controllerType, inputElement.actionID, true, this.elementMapsBuffer);
				for (int j = 0; j < elementMapsWithAction; j++)
				{
					ActionElementMap actionElementMap = this.elementMapsBuffer[j];
					if (actionElementMap.axisContribution == (Pole)inputElement.axisContribution)
					{
						actionElementMap.controllerMap.DeleteElementMap(actionElementMap.id);
					}
				}
			}
		}

		// Token: 0x0600176F RID: 5999 RVA: 0x0004A7B4 File Offset: 0x000489B4
		private bool IsValidButton(ControllerPollingInfo info)
		{
			return this.data.nonUserAssignableKeys.All((string key) => !string.Equals(key, info.elementIdentifierName, StringComparison.InvariantCultureIgnoreCase));
		}

		// Token: 0x06001770 RID: 6000 RVA: 0x0004A7EC File Offset: 0x000489EC
		public void RestoreDefaultMapping()
		{
			this.controllerMapStore.RestoreDefaults();
			foreach (Controller controller in this.RewiredPlayer.controllers.Controllers)
			{
				this.RewiredPlayer.controllers.maps.LoadDefaultMaps(controller.type);
			}
			Action activeControllerChanged = this.ActiveControllerChanged;
			if (activeControllerChanged == null)
			{
				return;
			}
			activeControllerChanged();
		}

		// Token: 0x06001771 RID: 6001 RVA: 0x0004A874 File Offset: 0x00048A74
		private bool TryGetElementMap(InputElement inputElement, out ActionElementMap positiveElementMap, out ActionElementMap negativeElementMap)
		{
			RewiredInputManager.<>c__DisplayClass61_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.inputElement = inputElement;
			positiveElementMap = null;
			negativeElementMap = null;
			ControllerType controllerType = (ControllerType)CS$<>8__locals1.inputElement.controllerType;
			if (this.IsKeyboardController(controllerType))
			{
				this.<TryGetElementMap>g__CheckControllerType|61_0(ControllerType.Keyboard, out positiveElementMap, out negativeElementMap, ref CS$<>8__locals1);
				ActionElementMap actionElementMap;
				ActionElementMap actionElementMap2;
				this.<TryGetElementMap>g__CheckControllerType|61_0(ControllerType.Mouse, out actionElementMap, out actionElementMap2, ref CS$<>8__locals1);
				if (positiveElementMap == null)
				{
					positiveElementMap = actionElementMap;
				}
				if (negativeElementMap == null)
				{
					negativeElementMap = actionElementMap2;
				}
				return positiveElementMap != null || negativeElementMap != null;
			}
			return this.<TryGetElementMap>g__CheckControllerType|61_0(controllerType, out positiveElementMap, out negativeElementMap, ref CS$<>8__locals1);
		}

		// Token: 0x06001772 RID: 6002 RVA: 0x0004A8F0 File Offset: 0x00048AF0
		public bool GetStickSwapState()
		{
			int num;
			return this.TryGetActionElementID(39, InputAxisContribution.Positive, out num) && num != 0;
		}

		// Token: 0x06001773 RID: 6003 RVA: 0x0004A910 File Offset: 0x00048B10
		public void SwapSticksActions()
		{
			this.SwapInputElementsMapping(this.LeftStickXElement, this.RightStickXElement);
			this.SwapInputElementsMapping(this.LeftStickYElement, this.RightStickYElement);
		}

		// Token: 0x06001774 RID: 6004 RVA: 0x0004A938 File Offset: 0x00048B38
		private async void CreatePlayerMouse()
		{
			await new WaitUntil(() => ReInput.isReady);
			this.playerMouse = PlayerMouse.Factory.Create();
			this.playerMouse.playerId = this.data.rewiredPlayerID;
			this.UpdatePointerPositionMode();
			this.playerMouse.xAxis.actionName = this.data.horizontalCursorAxisName;
			this.playerMouse.yAxis.actionName = this.data.verticalCursorAxisName;
			this.playerMouse.leftButton.actionName = this.data.lmbActionName;
			this.playerMouse.rightButton.actionName = this.data.rmbActionName;
			this.playerMouse.middleButton.actionName = this.data.mmbActionName;
			this.playerMouse.screenPosition = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			this.playerMouse.ScreenPositionChangedEvent += this.OnCursorScreenPositionChanged;
			this.SetPointerSpeed(this.settingsManager.CurrentState.inputData.cursorSensitivity);
			this.inputModule.AddMouseInputSource(this.playerMouse);
		}

		// Token: 0x06001775 RID: 6005 RVA: 0x0004A971 File Offset: 0x00048B71
		private void OnCursorScreenPositionChanged(Vector2 position)
		{
			this.playerMouse.screenPosition = position;
			Action<Vector2> pointerPositionChanged = this.PointerPositionChanged;
			if (pointerPositionChanged == null)
			{
				return;
			}
			pointerPositionChanged(position);
		}

		// Token: 0x06001776 RID: 6006 RVA: 0x0004A990 File Offset: 0x00048B90
		public override bool GetPointerActiveState()
		{
			PlayerMouse playerMouse = this.playerMouse;
			return playerMouse != null && playerMouse.enabled;
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x0004A9A4 File Offset: 0x00048BA4
		public override async void SetPointerActiveState(bool isActive, bool hideFromScreenBounds = false)
		{
			if (this.playerMouse != null && this.playerMouse.enabled != isActive)
			{
				if (hideFromScreenBounds && !isActive)
				{
					this.playerMouse.screenPosition = new Vector2(-1f, -1f);
					await new WaitForEndOfFrame();
				}
				this.playerMouse.enabled = isActive;
				Action<bool> pointerActiveStateChanged = this.PointerActiveStateChanged;
				if (pointerActiveStateChanged != null)
				{
					pointerActiveStateChanged(isActive);
				}
			}
		}

		// Token: 0x06001778 RID: 6008 RVA: 0x0004A9F0 File Offset: 0x00048BF0
		private void OnPlayerActiveControllerChanged(Player player, Controller controller)
		{
			if (this.activeController == controller)
			{
				return;
			}
			if (this.IsKeyboardController(this.activeController) && this.IsKeyboardController(controller))
			{
				this.activeController = controller;
				return;
			}
			this.activeController = controller;
			this.UpdatePointerPositionMode();
			this.LoadControllerData(controller);
			this.UpdateActiveControllerType();
			Action activeControllerChanged = this.ActiveControllerChanged;
			if (activeControllerChanged == null)
			{
				return;
			}
			activeControllerChanged();
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x0004AA50 File Offset: 0x00048C50
		private void UpdateActiveControllerType()
		{
			if (this.activeController == null)
			{
				this.lastActiveControllerType = InputControllerType.Custom;
				return;
			}
			if (this.activeController.type != ControllerType.Joystick)
			{
				this.lastActiveControllerType = InputControllerType.Keyboard;
				return;
			}
			this.lastActiveControllerType = InputControllerType.Joystick;
		}

		// Token: 0x0600177A RID: 6010 RVA: 0x0004AA80 File Offset: 0x00048C80
		private void UpdatePointerPositionMode()
		{
			if (this.playerMouse == null)
			{
				return;
			}
			this.playerMouse.useHardwarePointerPosition = false;
		}

		// Token: 0x0600177B RID: 6011 RVA: 0x0004AA97 File Offset: 0x00048C97
		public override bool GetButtonTimedPressUp(int actionId, float time, float expireIn)
		{
			return this.RewiredPlayer.GetButtonTimedPressUp(actionId, time, expireIn);
		}

		// Token: 0x0600177C RID: 6012 RVA: 0x0004AAA7 File Offset: 0x00048CA7
		public override bool GetButtonTimedPress(int actionId, float time)
		{
			return this.RewiredPlayer.GetButtonTimedPress(actionId, time);
		}

		// Token: 0x0600177D RID: 6013 RVA: 0x0004AAB6 File Offset: 0x00048CB6
		public override bool GetButton(int actionId)
		{
			return this.RewiredPlayer.GetButton(actionId);
		}

		// Token: 0x0600177E RID: 6014 RVA: 0x0004AAC4 File Offset: 0x00048CC4
		public override bool GetButtonUp(int actionId)
		{
			return this.RewiredPlayer.GetButtonUp(actionId);
		}

		// Token: 0x0600177F RID: 6015 RVA: 0x0004AAD2 File Offset: 0x00048CD2
		public override bool GetButtonDown(int actionId)
		{
			return this.RewiredPlayer.GetButtonDown(actionId);
		}

		// Token: 0x06001780 RID: 6016 RVA: 0x0004AAE0 File Offset: 0x00048CE0
		public override bool GetButtonDoublePressDown(int actionId)
		{
			return this.RewiredPlayer.GetButtonDoublePressDown(actionId);
		}

		// Token: 0x06001781 RID: 6017 RVA: 0x0004AAEE File Offset: 0x00048CEE
		public override bool GetButtonClicked(int actionId)
		{
			return this.RewiredPlayer.GetButtonSinglePressUp(actionId);
		}

		// Token: 0x06001782 RID: 6018 RVA: 0x0004AAFC File Offset: 0x00048CFC
		public override float GetHorizontalAxisInput()
		{
			return this.RewiredPlayer.GetAxis(this.data.horizontalAxisName);
		}

		// Token: 0x06001783 RID: 6019 RVA: 0x0004AB14 File Offset: 0x00048D14
		public override float GetVerticalAxisInput()
		{
			return this.RewiredPlayer.GetAxis(this.data.verticalAxisName);
		}

		// Token: 0x06001784 RID: 6020 RVA: 0x0004AB2C File Offset: 0x00048D2C
		public override float GetCursorHorizontalAxisInput()
		{
			return this.RewiredPlayer.GetAxis(this.data.horizontalCursorAxisName);
		}

		// Token: 0x06001785 RID: 6021 RVA: 0x0004AB44 File Offset: 0x00048D44
		public override float GetCursorVerticalAxisInput()
		{
			return this.RewiredPlayer.GetAxis(this.data.verticalCursorAxisName);
		}

		// Token: 0x06001786 RID: 6022 RVA: 0x0004AB5C File Offset: 0x00048D5C
		public override bool TryGetActionElementID(int playerAction, InputAxisContribution axisContribution, out int elementID)
		{
			int num;
			InputElementType inputElementType;
			KeyCode keyCode;
			return this.TryGetActionElementID(playerAction, axisContribution, out elementID, out num, out inputElementType, out keyCode);
		}

		// Token: 0x06001787 RID: 6023 RVA: 0x0004AB78 File Offset: 0x00048D78
		public override bool TryGetActionElementID(int playerAction, InputAxisContribution axisContribution, out int elementID, out int controllerID, out InputElementType elementType, out KeyCode keyCode)
		{
			int elementMapsWithAction = this.RewiredPlayer.controllers.maps.GetElementMapsWithAction(playerAction, true, this.elementMapsBuffer);
			for (int i = 0; i < elementMapsWithAction; i++)
			{
				ActionElementMap actionElementMap = this.elementMapsBuffer[i];
				if (this.IsKeyboardController((ControllerType)this.ActiveControllerType) == this.IsKeyboardController(actionElementMap.controllerMap.controllerType) && actionElementMap.axisContribution == (Pole)axisContribution)
				{
					elementID = actionElementMap.elementIdentifierId;
					controllerID = actionElementMap.controllerMap.controllerId;
					keyCode = actionElementMap.keyCode;
					elementType = (InputElementType)actionElementMap.elementType;
					return true;
				}
			}
			elementID = -1;
			controllerID = -1;
			keyCode = KeyCode.None;
			elementType = InputElementType.Button;
			return false;
		}

		// Token: 0x06001788 RID: 6024 RVA: 0x0004AC20 File Offset: 0x00048E20
		public override void UpdateInputElementControllerType(ref InputElement inputElement)
		{
			if (inputElement.controllerType == InputControllerType.Joystick)
			{
				return;
			}
			int elementMapsWithAction = this.RewiredPlayer.controllers.maps.GetElementMapsWithAction(inputElement.actionID, true, this.elementMapsBuffer);
			for (int i = 0; i < elementMapsWithAction; i++)
			{
				ActionElementMap actionElementMap = this.elementMapsBuffer[i];
				if (actionElementMap.controllerMap.controllerType != ControllerType.Joystick && actionElementMap.axisContribution == (Pole)inputElement.axisContribution)
				{
					inputElement.controllerType = (InputControllerType)actionElementMap.controllerMap.controllerType;
					return;
				}
			}
		}

		// Token: 0x06001789 RID: 6025 RVA: 0x0004ACA1 File Offset: 0x00048EA1
		private bool IsKeyboardController(Controller controller)
		{
			return controller != null && this.IsKeyboardController(controller.type);
		}

		// Token: 0x0600178A RID: 6026 RVA: 0x0004ACB4 File Offset: 0x00048EB4
		private bool IsKeyboardController(ControllerType controllerType)
		{
			return controllerType == ControllerType.Keyboard || controllerType == ControllerType.Mouse;
		}

		// Token: 0x0600178B RID: 6027 RVA: 0x0004ACBF File Offset: 0x00048EBF
		protected override void OnDestroy()
		{
			base.OnDestroy();
			Player player = this.rewiredPlayerUnsafe;
			if (player == null)
			{
				return;
			}
			player.controllers.RemoveLastActiveControllerChangedDelegate(new PlayerActiveControllerChangedDelegate(this.OnPlayerActiveControllerChanged));
		}

		// Token: 0x0600178D RID: 6029 RVA: 0x0004AD44 File Offset: 0x00048F44
		[CompilerGenerated]
		private bool <TryGetElementMap>g__CheckControllerType|61_0(ControllerType controllerType, out ActionElementMap positiveElementMap, out ActionElementMap negativeElementMap, ref RewiredInputManager.<>c__DisplayClass61_0 A_4)
		{
			positiveElementMap = null;
			negativeElementMap = null;
			this.RewiredPlayer.controllers.maps.GetElementMapsWithAction(controllerType, A_4.inputElement.actionID, true, this.elementMapsBuffer);
			if (this.elementMapsBuffer != null)
			{
				for (int i = 0; i < this.elementMapsBuffer.Count; i++)
				{
					ActionElementMap actionElementMap = this.elementMapsBuffer[i];
					if (actionElementMap.axisContribution == Pole.Positive)
					{
						positiveElementMap = actionElementMap;
					}
					else if (actionElementMap.axisContribution == Pole.Negative)
					{
						negativeElementMap = actionElementMap;
					}
				}
			}
			return positiveElementMap != null || negativeElementMap != null;
		}

		// Token: 0x04000D6C RID: 3436
		private const int DefaultMapCategoryId = 0;

		// Token: 0x04000D74 RID: 3444
		[HideInInspector]
		public InputManagerBase.Data data;

		// Token: 0x04000D75 RID: 3445
		private InputControllerType lastActiveControllerType;

		// Token: 0x04000D76 RID: 3446
		private Controller activeController;

		// Token: 0x04000D77 RID: 3447
		private PlayerMouse playerMouse;

		// Token: 0x04000D78 RID: 3448
		private Player rewiredPlayerUnsafe;

		// Token: 0x04000D79 RID: 3449
		private RewiredStandaloneInputModule inputModule;

		// Token: 0x04000D7A RID: 3450
		private Camera mainCamera;

		// Token: 0x04000D7B RID: 3451
		private IGameSettingsManager settingsManager;

		// Token: 0x04000D7C RID: 3452
		private float currentPointerSpeed;

		// Token: 0x04000D7D RID: 3453
		private IResetableControllerMapStore controllerMapStore;

		// Token: 0x04000D7E RID: 3454
		private readonly List<ActionElementMap> elementMapsBuffer = new List<ActionElementMap>(64);

		// Token: 0x04000D7F RID: 3455
		private const int LeftStickDefaultElementID = 0;

		// Token: 0x04000D80 RID: 3456
		private const int LeftStickXAction = 39;

		// Token: 0x04000D81 RID: 3457
		private const int LeftStickYAction = 40;

		// Token: 0x04000D82 RID: 3458
		private const int RightStickXAction = 65;

		// Token: 0x04000D83 RID: 3459
		private const int RightStickYAction = 66;

		// Token: 0x04000D84 RID: 3460
		private readonly InputElement LeftStickXElement = new InputElement(39, InputAxisContribution.Positive, InputControllerType.Joystick);

		// Token: 0x04000D85 RID: 3461
		private readonly InputElement LeftStickYElement = new InputElement(40, InputAxisContribution.Positive, InputControllerType.Joystick);

		// Token: 0x04000D86 RID: 3462
		private readonly InputElement RightStickXElement = new InputElement(65, InputAxisContribution.Positive, InputControllerType.Joystick);

		// Token: 0x04000D87 RID: 3463
		private readonly InputElement RightStickYElement = new InputElement(66, InputAxisContribution.Positive, InputControllerType.Joystick);

		// Token: 0x02000516 RID: 1302
		public enum ButtonRemappingResult
		{
			// Token: 0x04001B0E RID: 6926
			Success,
			// Token: 0x04001B0F RID: 6927
			AlreadyAssigned,
			// Token: 0x04001B10 RID: 6928
			InvalidKey,
			// Token: 0x04001B11 RID: 6929
			ButtonAssignError
		}
	}
}
