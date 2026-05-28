using System;
using Game.InputManager;
using UnityEngine;

namespace Unliving.InputManager
{
	// Token: 0x0200029D RID: 669
	[CreateAssetMenu(fileName = "InputControllersData", menuName = "Game/Input/Input Controllers Settings")]
	public class InputControllersData : ScriptableObject
	{
		// Token: 0x0600171D RID: 5917 RVA: 0x00049A7C File Offset: 0x00047C7C
		public InputControllerData.ButtonGlyph GetButtonGlyph(string controllerName, InputControllerType controllerType, string elementName, InputAxisContribution axisContribution)
		{
			foreach (InputControllerData inputControllerData in this.controllersData)
			{
				if (inputControllerData.IsValidController(controllerName))
				{
					return inputControllerData.GetButtonGlyph(elementName, axisContribution);
				}
			}
			if (controllerType == InputControllerType.Joystick)
			{
				foreach (InputControllerData inputControllerData2 in this.controllersData)
				{
					if (inputControllerData2.controllerType == InputControllerType.Joystick)
					{
						InputControllerData.ButtonGlyph buttonGlyph = inputControllerData2.GetButtonGlyph(elementName, axisContribution);
						if (buttonGlyph != null)
						{
							return buttonGlyph;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x04000D66 RID: 3430
		public InputControllerData[] controllersData;
	}
}
