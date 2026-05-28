using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.AbilityResources;

namespace Unliving.Abilities
{
	// Token: 0x02000376 RID: 886
	[CreateAssetMenu(fileName = "AbilityResourceAsProjectileRendererSetter", menuName = "Abilities/Controllers/Resource As Projectile Renderer Setter")]
	public sealed class AbilityResourceAsProjectileRendererSetter : AbilityExtensionAssetBase
	{
		// Token: 0x17000606 RID: 1542
		// (get) Token: 0x06001D20 RID: 7456 RVA: 0x0005C041 File Offset: 0x0005A241
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D21 RID: 7457 RVA: 0x0005C044 File Offset: 0x0005A244
		public override void OnAddedToAbility(BaseAbility ability)
		{
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			IAbilityResourcesConsumer resourcesConsumer;
			if (projectileAbilityBase != null && ability.TryGetExtension(out resourcesConsumer))
			{
				this.abilitiesData.Add(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), new AbilityResourceAsProjectileRendererSetter.PerAbilityData
				{
					resourcesConsumer = resourcesConsumer,
					resourcesSprites = new List<Sprite>(32)
				});
				projectileAbilityBase.Activated += this.OnAbilityActivated;
				projectileAbilityBase.ProjectileLaunched += this.OnAbilityProjectileLaunched;
				projectileAbilityBase.Completed += this.OnAbilityCompleted;
			}
			base.OnAddedToAbility(ability);
		}

		// Token: 0x06001D22 RID: 7458 RVA: 0x0005C0CC File Offset: 0x0005A2CC
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			base.OnRemovedFromAbility(ability);
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				int abilityInstanceID = AbilityExtensionAssetBase.GetAbilityInstanceID(ability);
				AbilityResourceAsProjectileRendererSetter.PerAbilityData perAbilityData;
				if (this.abilitiesData.TryGetValue(abilityInstanceID, out perAbilityData))
				{
					perAbilityData.Invalidate();
					this.abilitiesData.Remove(abilityInstanceID);
				}
				projectileAbilityBase.Activated -= this.OnAbilityActivated;
				projectileAbilityBase.ProjectileLaunched -= this.OnAbilityProjectileLaunched;
				projectileAbilityBase.Completed -= this.OnAbilityCompleted;
			}
		}

		// Token: 0x06001D23 RID: 7459 RVA: 0x0005C14C File Offset: 0x0005A34C
		private void OnAbilityActivated(IAbility ability, object args)
		{
			AbilityResourceAsProjectileRendererSetter.PerAbilityData perAbilityData;
			if (this.abilitiesData.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out perAbilityData))
			{
				List<Sprite> resourcesSprites = perAbilityData.resourcesSprites;
				IReadOnlyList<CollectableAbilityResource> collectedResources = perAbilityData.resourcesConsumer.GetCollectedResources(ability);
				if (collectedResources != null)
				{
					for (int i = 0; i < collectedResources.Count; i++)
					{
						Sprite sprite = collectedResources[i].ResourceRenderer.sprite;
						if (sprite != null)
						{
							int instanceID = sprite.GetInstanceID();
							if (!this.spriteCenters.ContainsKey(instanceID))
							{
								Vector2[] vertices = sprite.vertices;
								Vector2 vector = default(Vector2);
								for (int j = 0; j < vertices.Length; j++)
								{
									vector += vertices[j];
								}
								vector /= (float)vertices.Length;
								this.spriteCenters.Add(instanceID, vector);
							}
							resourcesSprites.Add(sprite);
						}
					}
				}
			}
		}

		// Token: 0x06001D24 RID: 7460 RVA: 0x0005C234 File Offset: 0x0005A434
		private void OnAbilityProjectileLaunched(ProjectileAbilityBase.LaunchEventArgs args)
		{
			Component component = args.launchedProjectile.AttachedObject as Component;
			GameObject gameObject = (component != null) ? component.gameObject : null;
			AbilityResourceAsProjectileRendererSetter.PerAbilityData perAbilityData;
			if (gameObject != null && this.abilitiesData.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(args.ability), out perAbilityData))
			{
				List<Sprite> resourcesSprites = perAbilityData.resourcesSprites;
				if (resourcesSprites.Count != 0)
				{
					SpriteRenderer spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
					spriteRenderer.sprite = resourcesSprites.GetRandomItem(0, -1);
					spriteRenderer.transform.parent = gameObject.transform;
					spriteRenderer.transform.localPosition = -this.spriteCenters[spriteRenderer.sprite.GetInstanceID()];
					SpriteRenderer spriteRenderer2;
					if (gameObject.TryGetComponent<SpriteRenderer>(out spriteRenderer2))
					{
						spriteRenderer.sortingLayerID = spriteRenderer2.sortingLayerID;
						spriteRenderer.sortingOrder = spriteRenderer2.sortingOrder;
						spriteRenderer.sharedMaterial = spriteRenderer2.sharedMaterial;
						if (this.copyProjectileRendererColor)
						{
							spriteRenderer.color = spriteRenderer2.color;
						}
						spriteRenderer2.enabled = false;
					}
				}
			}
		}

		// Token: 0x06001D25 RID: 7461 RVA: 0x0005C33C File Offset: 0x0005A53C
		private void OnAbilityCompleted(IAbility ability, object args)
		{
			AbilityResourceAsProjectileRendererSetter.PerAbilityData perAbilityData;
			if (this.abilitiesData.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out perAbilityData))
			{
				perAbilityData.resourcesSprites.Clear();
			}
		}

		// Token: 0x0400107C RID: 4220
		public bool copyProjectileRendererColor;

		// Token: 0x0400107D RID: 4221
		private readonly Dictionary<int, AbilityResourceAsProjectileRendererSetter.PerAbilityData> abilitiesData = new Dictionary<int, AbilityResourceAsProjectileRendererSetter.PerAbilityData>(16);

		// Token: 0x0400107E RID: 4222
		private readonly Dictionary<int, Vector2> spriteCenters = new Dictionary<int, Vector2>(16);

		// Token: 0x02000567 RID: 1383
		private sealed class PerAbilityData
		{
			// Token: 0x0600270E RID: 9998 RVA: 0x00079972 File Offset: 0x00077B72
			public void Invalidate()
			{
				this.resourcesConsumer = null;
				this.resourcesSprites.Clear();
				this.resourcesSprites.TrimExcess();
				this.resourcesSprites = null;
			}

			// Token: 0x04001C19 RID: 7193
			public IAbilityResourcesConsumer resourcesConsumer;

			// Token: 0x04001C1A RID: 7194
			public List<Sprite> resourcesSprites;
		}
	}
}
