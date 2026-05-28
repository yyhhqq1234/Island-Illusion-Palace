using System;
using System.Collections.Generic;
using System.Linq;
using Common.Factories;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Factories;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs.VFX
{
	// Token: 0x02000203 RID: 515
	[DefaultExecutionOrder(10)]
	public sealed class GameMobAreaObserverVFX : GameBehaviourBase
	{
		// Token: 0x06001145 RID: 4421 RVA: 0x00035D78 File Offset: 0x00033F78
		private void Start()
		{
			this.targetsProvider = base.GetComponent<GameMobAreaObserver>();
			if (this.AbilityID != AbilityID.None)
			{
				PrototypeBasedFactory<AbilityFactoryPrototype, BaseAbility> prototypeBasedFactory = base.CurrentGame.Services.Get<AbilitiesFactory>();
				AbilityFactoryArgs args = new AbilityFactoryArgs
				{
					abilityID = this.AbilityID
				};
				AbilityFactoryPrototype objectPrototype = prototypeBasedFactory.GetObjectPrototype(args);
				BaseAbility baseAbility = (objectPrototype != null) ? objectPrototype.abilityPrototype : null;
				if (baseAbility != null)
				{
					CircleCollider2D circleCollider2D = this.targetsProvider.AreaCollider as CircleCollider2D;
					if (circleCollider2D != null)
					{
						circleCollider2D.radius = baseAbility.Range;
					}
				}
			}
			this.targetsProvider.ObjectEnteredArea += this.OnMobEnteredRange;
			this.targetsProvider.ObjectExitedArea += this.OnMobExitedRange;
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x00035E28 File Offset: 0x00034028
		private void ActiveEffects(BaseGameMob mob)
		{
			if (this.fxDict.ContainsKey(mob))
			{
				return;
			}
			GameMobVFXController.VisualEffect[] array = this.VisualEffects.ToArray<GameMobVFXController.VisualEffect>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].fxTransform = mob.transform;
				array[i].animatorTransform = mob.transform;
				array[i].Activate(mob);
			}
			this.fxDict.Add(mob, array);
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x00035EA0 File Offset: 0x000340A0
		private void DeactiveEffects(BaseGameMob mob)
		{
			if (!this.fxDict.ContainsKey(mob))
			{
				return;
			}
			GameMobVFXController.VisualEffect[] array = this.fxDict[mob];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Destroy(mob);
			}
			this.fxDict.Remove(mob);
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x00035EF1 File Offset: 0x000340F1
		private void OnMobExitedRange(BaseGameMob mob)
		{
			this.DeactiveEffects(mob);
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x00035EFA File Offset: 0x000340FA
		private void OnMobEnteredRange(BaseGameMob mob)
		{
			this.ActiveEffects(mob);
		}

		// Token: 0x0600114A RID: 4426 RVA: 0x00035F04 File Offset: 0x00034104
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.targetsProvider.IsNull())
			{
				this.targetsProvider.ObjectEnteredArea -= this.OnMobEnteredRange;
				this.targetsProvider.ObjectExitedArea -= this.OnMobExitedRange;
			}
		}

		// Token: 0x040009C4 RID: 2500
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID AbilityID;

		// Token: 0x040009C5 RID: 2501
		public GameMobVFXController.VisualEffect[] VisualEffects;

		// Token: 0x040009C6 RID: 2502
		private readonly Dictionary<BaseGameMob, GameMobVFXController.VisualEffect[]> fxDict = new Dictionary<BaseGameMob, GameMobVFXController.VisualEffect[]>();

		// Token: 0x040009C7 RID: 2503
		private GameMobAreaObserver targetsProvider;
	}
}
