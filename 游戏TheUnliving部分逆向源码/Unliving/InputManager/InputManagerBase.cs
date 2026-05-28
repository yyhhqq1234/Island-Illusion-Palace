using System;
using Game.Core;
using Game.InputManager;
using UnityEngine;

namespace Unliving.InputManager
{
	// Token: 0x0200029F RID: 671
	public abstract class InputManagerBase : GlobalSceneManagerBase, IInputManager
	{
		// Token: 0x140000D6 RID: 214
		// (add) Token: 0x06001722 RID: 5922
		// (remove) Token: 0x06001723 RID: 5923
		public abstract event Action ActiveControllerChanged;

		// Token: 0x140000D7 RID: 215
		// (add) Token: 0x06001724 RID: 5924
		// (remove) Token: 0x06001725 RID: 5925
		public abstract event Action<bool> PointerActiveStateChanged;

		// Token: 0x140000D8 RID: 216
		// (add) Token: 0x06001726 RID: 5926
		// (remove) Token: 0x06001727 RID: 5927
		public abstract event Action<Vector2> PointerPositionChanged;

		// Token: 0x140000D9 RID: 217
		// (add) Token: 0x06001728 RID: 5928
		// (remove) Token: 0x06001729 RID: 5929
		public abstract event Action<int, float> AxisValueChanged;

		// Token: 0x140000DA RID: 218
		// (add) Token: 0x0600172A RID: 5930
		// (remove) Token: 0x0600172B RID: 5931
		public abstract event Action<int> ButtonDown;

		// Token: 0x140000DB RID: 219
		// (add) Token: 0x0600172C RID: 5932
		// (remove) Token: 0x0600172D RID: 5933
		public abstract event Action<int> ButtonUp;

		// Token: 0x140000DC RID: 220
		// (add) Token: 0x0600172E RID: 5934
		// (remove) Token: 0x0600172F RID: 5935
		public abstract event Action<int> ButtonClicked;

		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x06001730 RID: 5936
		public abstract InputControllerType ActiveControllerType { get; }

		// Token: 0x06001731 RID: 5937
		public abstract bool GetPointerActiveState();

		// Token: 0x06001732 RID: 5938
		public abstract void SetPointerActiveState(bool isActive, bool hideFromScreenBounds = false);

		// Token: 0x06001733 RID: 5939
		public abstract void ChangePointerScreenPosition(Vector2 position);

		// Token: 0x06001734 RID: 5940
		public abstract bool GetButton(int actionId);

		// Token: 0x06001735 RID: 5941
		public abstract bool GetButtonDoublePressDown(int actionId);

		// Token: 0x06001736 RID: 5942
		public abstract bool GetButtonClicked(int actionId);

		// Token: 0x06001737 RID: 5943
		public abstract bool GetButtonDown(int actionId);

		// Token: 0x06001738 RID: 5944
		public abstract bool GetButtonTimedPress(int actionId, float time);

		// Token: 0x06001739 RID: 5945
		public abstract bool GetButtonTimedPressUp(int actionId, float time, float expireIn);

		// Token: 0x0600173A RID: 5946
		public abstract bool GetButtonUp(int actionId);

		// Token: 0x0600173B RID: 5947
		public abstract float GetCursorHorizontalAxisInput();

		// Token: 0x0600173C RID: 5948
		public abstract float GetCursorVerticalAxisInput();

		// Token: 0x0600173D RID: 5949
		public abstract float GetHorizontalAxisInput();

		// Token: 0x0600173E RID: 5950
		public abstract Vector2 GetScreenCursorPosition();

		// Token: 0x0600173F RID: 5951
		public abstract Ray GetScreenPointRay();

		// Token: 0x06001740 RID: 5952
		public abstract float GetVerticalAxisInput();

		// Token: 0x06001741 RID: 5953
		public abstract Vector2 GetWorldCursorPosition();

		// Token: 0x06001742 RID: 5954
		public abstract void SetPointerSpeed(float speed);

		// Token: 0x06001743 RID: 5955
		public abstract void UpdateInputElementControllerType(ref InputElement inputElement);

		// Token: 0x06001744 RID: 5956
		public abstract bool TryGetActionElementID(int playerAction, InputAxisContribution axisContribution, out int elementID);

		// Token: 0x06001745 RID: 5957
		public abstract bool TryGetActionElementID(int playerAction, InputAxisContribution axisContribution, out int elementID, out int controllerID, out InputElementType elementType, out KeyCode keyCode);

		// Token: 0x06001746 RID: 5958
		public abstract bool TryGetElementName(InputElement inputElement, out string positiveName, out string negativeName);

		// Token: 0x06001747 RID: 5959
		public abstract bool TryGetControllerName(InputControllerType controllerType, out string controllerName);

		// Token: 0x02000515 RID: 1301
		[Serializable]
		public class Data
		{
			// Token: 0x06002621 RID: 9761 RVA: 0x00077680 File Offset: 0x00075880
			public bool TryGetControllerSettings(InputControllerType controllerType, out ControllerTypeSettings controllerSettings)
			{
				foreach (ControllerTypeSettings controllerTypeSettings in this.controllerTypeSettings)
				{
					if (controllerTypeSettings != null && controllerTypeSettings.controllerType == controllerType)
					{
						controllerSettings = controllerTypeSettings;
						return true;
					}
				}
				controllerSettings = new ControllerTypeSettings
				{
					controllerType = controllerType,
					cursorSpeedMultiplier = 1f
				};
				return false;
			}

			// Token: 0x04001B03 RID: 6915
			public int rewiredPlayerID;

			// Token: 0x04001B04 RID: 6916
			public string horizontalAxisName = "Horizontal";

			// Token: 0x04001B05 RID: 6917
			public string verticalAxisName = "Vertical";

			// Token: 0x04001B06 RID: 6918
			public string horizontalCursorAxisName = "MouseX";

			// Token: 0x04001B07 RID: 6919
			public string verticalCursorAxisName = "MouseY";

			// Token: 0x04001B08 RID: 6920
			public string lmbActionName = "MouseLeftButton";

			// Token: 0x04001B09 RID: 6921
			public string rmbActionName = "MouseRightButton";

			// Token: 0x04001B0A RID: 6922
			public string mmbActionName = "MouseMiddleButton";

			// Token: 0x04001B0B RID: 6923
			public ControllerTypeSettings[] controllerTypeSettings;

			// Token: 0x04001B0C RID: 6924
			public string[] nonUserAssignableKeys;
		}
	}
}
