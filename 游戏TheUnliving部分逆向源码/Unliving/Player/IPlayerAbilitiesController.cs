using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Player
{
	// Token: 0x02000145 RID: 325
	public interface IPlayerAbilitiesController : IGameAbilitiesController, IAbilitiesController, ISlotsBasedAbilitiesController<AbilityInfo>, ISlotsBasedAbilitiesController
	{
		// Token: 0x17000161 RID: 353
		// (get) Token: 0x0600086E RID: 2158
		int NonNativeAbilitiesStartSlotIndex { get; }

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x0600086F RID: 2159
		int NonNativeAbilitiesCount { get; }

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000870 RID: 2160
		int NativeAbilitiesCount { get; }

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000871 RID: 2161
		bool IsSuspended { get; }

		// Token: 0x1400004E RID: 78
		// (add) Token: 0x06000872 RID: 2162
		// (remove) Token: 0x06000873 RID: 2163
		event Action<IPlayerAbilitiesController, int, BaseAbility.UsingArgs, BaseAbility.ActivationErrorType> ActiveAbilitySlotSelected;

		// Token: 0x1400004F RID: 79
		// (add) Token: 0x06000874 RID: 2164
		// (remove) Token: 0x06000875 RID: 2165
		event Action<BaseAbility, BaseAbility.ActivationError> AbilityActivationFailed;

		// Token: 0x06000876 RID: 2166
		void Initialize(IList<AbilityInfo> nativeAbilities, IList<AbilityTypes> nativeAbilityTypes, IList<AbilityInfo> abilities, IList<AbilityTypes> abilityTypes);

		// Token: 0x06000877 RID: 2167
		bool IsNativeAbilitySlot(int abilitySlot);

		// Token: 0x06000878 RID: 2168
		int GetAbilitySlotType(int abilitySlot);

		// Token: 0x06000879 RID: 2169
		bool SwapAbilities(int slot0, int slot1);

		// Token: 0x0600087A RID: 2170
		void UseExternalAbility(AbilityInfo abilityInfo, BaseAbility.UsingArgs usingArgs);

		// Token: 0x0600087B RID: 2171
		GameObject DropAbility(int abilitySlot, Vector2? targetPosition = null, float dropRadius = 0f, float maxRandomRotation = 0f);

		// Token: 0x0600087C RID: 2172
		void SetSuspended();

		// Token: 0x0600087D RID: 2173
		void OnLateUpdate();
	}
}
