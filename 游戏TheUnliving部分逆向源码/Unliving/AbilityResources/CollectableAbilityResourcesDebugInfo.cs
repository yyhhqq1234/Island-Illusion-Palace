using System;
using Game.Core;
using UnityEngine;

namespace Unliving.AbilityResources
{
	// Token: 0x0200035D RID: 861
	[DisallowMultipleComponent]
	public sealed class CollectableAbilityResourcesDebugInfo : GlobalSceneManagerBase
	{
		// Token: 0x06001C49 RID: 7241 RVA: 0x0005959C File Offset: 0x0005779C
		static CollectableAbilityResourcesDebugInfo()
		{
			Array values = Enum.GetValues(typeof(AbilityResourceType));
			CollectableAbilityResourcesDebugInfo.ResourcesTypes = new AbilityResourceType[values.Length - 2];
			int num = 0;
			int num2 = 0;
			while (num2 < values.Length && num < CollectableAbilityResourcesDebugInfo.ResourcesTypes.Length)
			{
				AbilityResourceType abilityResourceType = (AbilityResourceType)values.GetValue(num2);
				if (abilityResourceType != AbilityResourceType.Undefined && abilityResourceType != AbilityResourceType.Corpse)
				{
					CollectableAbilityResourcesDebugInfo.ResourcesTypes[num++] = abilityResourceType;
				}
				num2++;
			}
		}

		// Token: 0x06001C4A RID: 7242 RVA: 0x00059609 File Offset: 0x00057809
		private void CountResource(CollectableAbilityResource resource, bool add)
		{
			if (add)
			{
				this.resourcesCounts[(int)resource.type]++;
				return;
			}
			this.resourcesCounts[(int)resource.type]--;
		}

		// Token: 0x06001C4B RID: 7243 RVA: 0x0005963B File Offset: 0x0005783B
		private void OnResourceCreated(CollectableAbilityResourcesFactoryArgs args, CollectableAbilityResource resource)
		{
			this.CountResource(resource, true);
			resource.Destroyed += this.OnResourceDestroyed;
		}

		// Token: 0x06001C4C RID: 7244 RVA: 0x00059658 File Offset: 0x00057858
		private void OnResourceDestroyed(object obj)
		{
			CollectableAbilityResource collectableAbilityResource = (CollectableAbilityResource)obj;
			this.CountResource(collectableAbilityResource, false);
			collectableAbilityResource.Destroyed -= this.OnResourceDestroyed;
		}

		// Token: 0x06001C4D RID: 7245 RVA: 0x00059686 File Offset: 0x00057886
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (currentGame.Services.TryGet<ICollectableAbilityResourcesFactory>(out this.resourcesFactory))
			{
				this.resourcesFactory.ResourceCreated += this.OnResourceCreated;
			}
		}

		// Token: 0x06001C4E RID: 7246 RVA: 0x000596BC File Offset: 0x000578BC
		private void OnGUI()
		{
			GUILayout.Space(this.debugTextVerticalOffset);
			for (int i = 0; i < CollectableAbilityResourcesDebugInfo.ResourcesTypes.Length; i++)
			{
				AbilityResourceType abilityResourceType = CollectableAbilityResourcesDebugInfo.ResourcesTypes[i];
				int num = this.resourcesCounts[(int)abilityResourceType];
				GUILayout.Label(string.Format("{0}: {1}", abilityResourceType, num), Array.Empty<GUILayoutOption>());
			}
		}

		// Token: 0x06001C4F RID: 7247 RVA: 0x00059717 File Offset: 0x00057917
		protected override void OnDestroy()
		{
			if (this.resourcesFactory != null)
			{
				this.resourcesFactory.ResourceCreated -= this.OnResourceCreated;
			}
			base.OnDestroy();
		}

		// Token: 0x04000FFD RID: 4093
		private static readonly AbilityResourceType[] ResourcesTypes;

		// Token: 0x04000FFE RID: 4094
		public float debugTextVerticalOffset = 150f;

		// Token: 0x04000FFF RID: 4095
		private readonly int[] resourcesCounts = new int[8];

		// Token: 0x04001000 RID: 4096
		private ICollectableAbilityResourcesFactory resourcesFactory;
	}
}
