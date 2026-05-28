using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Rewired;
using Rewired.Interfaces;
using Unliving.GameSettings;

namespace Unliving.InputManager
{
	// Token: 0x020002A3 RID: 675
	public class RewiredUserDataStore : IResetableControllerMapStore, IControllerMapStore
	{
		// Token: 0x0600178F RID: 6031 RVA: 0x0004ADD8 File Offset: 0x00048FD8
		public RewiredUserDataStore(IGame game)
		{
			game.Services.TryGet<GameSettingsManager>(out this.settingsManager);
		}

		// Token: 0x06001790 RID: 6032 RVA: 0x0004ADF4 File Offset: 0x00048FF4
		public ControllerMap LoadControllerMap(int playerId, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
		{
			string mappingData = this.settingsManager.CurrentState.inputData.GetMappingData(controllerIdentifier.hardwareIdentifier, categoryId, layoutId);
			if (string.IsNullOrEmpty(mappingData))
			{
				return null;
			}
			ControllerMap controllerMap = ControllerMap.CreateFromXml(controllerIdentifier.controllerType, mappingData);
			if (controllerMap == null)
			{
				return null;
			}
			this.AddDefaultMappingsForNewActions(controllerIdentifier, controllerMap);
			return controllerMap;
		}

		// Token: 0x06001791 RID: 6033 RVA: 0x0004AE47 File Offset: 0x00049047
		public void SaveControllerMap(int playerId, ControllerMap map)
		{
			this.settingsManager.CurrentState.inputData.AddMappingData(map.controller.hardwareIdentifier, map.categoryId, map.layoutId, map.ToXmlString());
		}

		// Token: 0x06001792 RID: 6034 RVA: 0x0004AE7B File Offset: 0x0004907B
		public void RestoreDefaults()
		{
			this.settingsManager.CurrentState.inputData.ResetMappingData();
		}

		// Token: 0x06001793 RID: 6035 RVA: 0x0004AE94 File Offset: 0x00049094
		private void AddDefaultMappingsForNewActions(ControllerIdentifier controllerIdentifier, ControllerMap controllerMap)
		{
			if (controllerMap == null)
			{
				return;
			}
			ControllerMap controllerMapInstance = ReInput.mapping.GetControllerMapInstance(controllerIdentifier, controllerMap.categoryId, controllerMap.layoutId);
			if (controllerMapInstance == null)
			{
				return;
			}
			List<int> list = new List<int>();
			using (IEnumerator<InputAction> enumerator = ReInput.mapping.Actions.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					InputAction inputAction = enumerator.Current;
					if (!controllerMap.AllMaps.Any((ActionElementMap aem) => aem.actionId == inputAction.id))
					{
						list.Add(inputAction.id);
					}
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			foreach (ActionElementMap actionElementMap in controllerMapInstance.AllMaps)
			{
				if (list.Contains(actionElementMap.actionId) && !controllerMap.DoesElementAssignmentConflict(actionElementMap))
				{
					ElementAssignment elementAssignment = new ElementAssignment(controllerMap.controllerType, actionElementMap.elementType, actionElementMap.elementIdentifierId, actionElementMap.axisRange, actionElementMap.keyCode, actionElementMap.modifierKeyFlags, actionElementMap.actionId, actionElementMap.axisContribution, actionElementMap.invert);
					controllerMap.CreateElementMap(elementAssignment);
				}
			}
		}

		// Token: 0x04000D88 RID: 3464
		private GameSettingsManager settingsManager;
	}
}
