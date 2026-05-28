using System;
using System.Collections;
using System.Collections.Generic;
using FlowCanvas;
using NodeCanvas.Framework;
using UnityEngine;
using Unliving.LeveledItems;
using Unliving.Player.Upgrades;

namespace Unliving.Scripting.FlowCanvas.PlayerUpgrades
{
	// Token: 0x020000CE RID: 206
	[CreateAssetMenu(fileName = "ScriptedPlayerUpgrade", menuName = "Game/Player/Player Upgrade/Scripted Player Upgrade")]
	public sealed class ScriptedPlayerUpgrade : FlowScript, IPlayerUpgrade, ILeveledItem, IItemLevelProvider, IScript
	{
		// Token: 0x170000BE RID: 190
		// (get) Token: 0x0600050A RID: 1290 RVA: 0x00012696 File Offset: 0x00010896
		// (set) Token: 0x0600050B RID: 1291 RVA: 0x0001269E File Offset: 0x0001089E
		public IPlayerUpgrade UpgradePrototype { get; private set; }

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x0600050C RID: 1292 RVA: 0x000126A7 File Offset: 0x000108A7
		// (set) Token: 0x0600050D RID: 1293 RVA: 0x000126AF File Offset: 0x000108AF
		public int ItemLevel { get; set; }

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x0600050E RID: 1294 RVA: 0x000126B8 File Offset: 0x000108B8
		// (set) Token: 0x0600050F RID: 1295 RVA: 0x000126C0 File Offset: 0x000108C0
		public bool IsActivated { get; private set; }

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x06000510 RID: 1296 RVA: 0x000126C9 File Offset: 0x000108C9
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.ItemLevel;
			}
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x000126D1 File Offset: 0x000108D1
		public IPlayerUpgrade Clone()
		{
			ScriptedPlayerUpgrade scriptedPlayerUpgrade = Graph.Clone<ScriptedPlayerUpgrade>(this, this);
			scriptedPlayerUpgrade.UpgradePrototype = (this.UpgradePrototype ?? this);
			scriptedPlayerUpgrade.isInstantiated = true;
			return scriptedPlayerUpgrade;
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x000126F2 File Offset: 0x000108F2
		public IEnumerator Activate(IPlayerUpgradesRegistry upgradesRegistry, object activationContext)
		{
			if (this.IsActivated)
			{
				yield break;
			}
			yield return null;
			yield return null;
			((FlowScriptController)activationContext).StartBehaviour();
			this.IsActivated = true;
			yield break;
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x00012708 File Offset: 0x00010908
		public bool TryGetVariableValue<T>(string variableName, out T value)
		{
			IBlackboard blackboard = base.blackboard;
			Variable<T> variable = (blackboard != null) ? blackboard.GetVariable(variableName) : null;
			if (variable != null)
			{
				value = variable.value;
				return true;
			}
			value = default(T);
			return false;
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x00012744 File Offset: 0x00010944
		public bool TrySetVariableValue<T>(string variableName, T value)
		{
			IBlackboard blackboard = base.blackboard;
			Variable<T> variable = (blackboard != null) ? blackboard.GetVariable(variableName) : null;
			if (variable != null)
			{
				variable.value = value;
				return true;
			}
			return false;
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x00012774 File Offset: 0x00010974
		public bool TryGetVariableValue(string variableName, Type variableType, out object value)
		{
			IBlackboard blackboard = base.blackboard;
			Func<object> func;
			if (blackboard == null)
			{
				func = null;
			}
			else
			{
				Variable variable = blackboard.GetVariable(variableName, null);
				func = ((variable != null) ? variable.GetGetConverter(variableType) : null);
			}
			Func<object> func2 = func;
			if (func2 != null)
			{
				value = func2();
				return true;
			}
			value = null;
			return false;
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x000127B4 File Offset: 0x000109B4
		public bool TrySetVariableValue(string variableName, Type variableType, object value)
		{
			IBlackboard blackboard = base.blackboard;
			Action<object> action = (blackboard != null) ? blackboard.GetVariable(variableName, null).GetSetConverter(variableType) : null;
			if (action != null)
			{
				action(value);
				return true;
			}
			return false;
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x000127E9 File Offset: 0x000109E9
		public IEnumerable<ValueTuple<string, object>> GetVariables()
		{
			if (base.blackboard != null)
			{
				foreach (Variable variable in base.blackboard.GetVariables(null))
				{
					yield return new ValueTuple<string, object>(variable.name, variable.value);
				}
				IEnumerator<Variable> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x0400037A RID: 890
		[NonSerialized]
		private bool isInstantiated;
	}
}
