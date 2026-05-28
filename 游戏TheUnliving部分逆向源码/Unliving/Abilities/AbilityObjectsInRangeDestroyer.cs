using System;
using Common;
using Game.Abilities;
using Game.Abilities.TargetsCollection;
using UnityEngine;
using Unliving.AbilityResources;

namespace Unliving.Abilities
{
	// Token: 0x0200036F RID: 879
	[CreateAssetMenu(fileName = "AbilityObjectsInRangeDestroyer", menuName = "Abilities/Controllers/Objects In Range Destroyer")]
	public sealed class AbilityObjectsInRangeDestroyer : AbilityExtensionAssetBase
	{
		// Token: 0x170005FD RID: 1533
		// (get) Token: 0x06001CEC RID: 7404 RVA: 0x0005B5BF File Offset: 0x000597BF
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001CED RID: 7405 RVA: 0x0005B5C2 File Offset: 0x000597C2
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ability.Used += this.OnAbilityUsed;
		}

		// Token: 0x06001CEE RID: 7406 RVA: 0x0005B5DD File Offset: 0x000597DD
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Used -= this.OnAbilityUsed;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x0005B5F8 File Offset: 0x000597F8
		private void OnAbilityUsed(IAbility ability, object args)
		{
			if (this.affectableObjectsLayers.value == 0)
			{
				return;
			}
			float num = (this.customDestructionRange > 0f) ? this.customDestructionRange : ability.GetAbilityEffectRange();
			if (num <= 0f)
			{
				return;
			}
			BaseAbility.UsingArgs usingArgs = (BaseAbility.UsingArgs)args;
			Collider2D[] tempTargetsBuffer = AbilityTargetsCollector<Collider2D>.TempTargetsBuffer;
			int num2 = Physics2D.OverlapCircleNonAlloc(usingArgs.targetPosition, num, tempTargetsBuffer, this.affectableObjectsLayers);
			for (int i = 0; i < num2; i++)
			{
				Collider2D collider2D = tempTargetsBuffer[i];
				CollectableAbilityResource collectableAbilityResource;
				IDestroyable destroyable;
				if (this.tryCollectAbilityResources && collider2D.TryGetComponent<CollectableAbilityResource>(out collectableAbilityResource))
				{
					collectableAbilityResource.Collect(this);
				}
				else if (collider2D.TryGetComponent<IDestroyable>(out destroyable))
				{
					destroyable.Destroy();
				}
				else
				{
					UnityEngine.Object.Destroy(collider2D.gameObject);
				}
			}
		}

		// Token: 0x0400105F RID: 4191
		public LayerMask affectableObjectsLayers;

		// Token: 0x04001060 RID: 4192
		public float customDestructionRange;

		// Token: 0x04001061 RID: 4193
		public bool tryCollectAbilityResources;
	}
}
