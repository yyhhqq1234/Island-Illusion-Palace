using System;
using Game.Buffs;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000CD RID: 205
	public static class ScriptingUtility
	{
		// Token: 0x06000506 RID: 1286 RVA: 0x00012630 File Offset: 0x00010830
		public static IGame GetCurrentGame(this GameObject sourceObject, out GameBehaviourBase gameContextProvider)
		{
			if (sourceObject != null)
			{
				if (!sourceObject.TryGetComponent<GameBehaviourBase>(out gameContextProvider))
				{
					gameContextProvider = sourceObject.AddComponent<GameContextProvider>();
				}
				return gameContextProvider.CurrentGame;
			}
			gameContextProvider = null;
			return null;
		}

		// Token: 0x06000507 RID: 1287 RVA: 0x00012658 File Offset: 0x00010858
		public static IGame GetCurrentGame(this Component sourceComponent, out GameBehaviourBase gameContextProvider)
		{
			if (sourceComponent != null)
			{
				return sourceComponent.gameObject.GetCurrentGame(out gameContextProvider);
			}
			gameContextProvider = null;
			return null;
		}

		// Token: 0x06000508 RID: 1288 RVA: 0x00012674 File Offset: 0x00010874
		public static bool TryGetAbilitiesController(BaseGameMob targetMob, out GameAbilitiesController abilitiesController)
		{
			abilitiesController = targetMob.AbilitiesController;
			return abilitiesController != null;
		}

		// Token: 0x06000509 RID: 1289 RVA: 0x00012685 File Offset: 0x00010885
		public static bool TryGetBuffsController(BaseGameMob targetMob, out IBuffsController buffsController)
		{
			buffsController = targetMob.BuffsController;
			return buffsController != null;
		}
	}
}
