using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage.Projectiles;
using Game.VFX;
using UnityEngine;
using Unliving.Mobs.Motion;

namespace Unliving.Abilities.VFX
{
	// Token: 0x020003B9 RID: 953
	[CreateAssetMenu(fileName = "PullingProjectileAbilityVFXController", menuName = "Abilities/Controllers/Pulling Projectile Ability VFX Controller")]
	public sealed class PullingProjectileAbilityVFXController : AbilityExtensionAssetBase, IPullingProjectileAbilityRenderer
	{
		// Token: 0x17000694 RID: 1684
		// (get) Token: 0x0600203A RID: 8250 RVA: 0x00065849 File Offset: 0x00063A49
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600203B RID: 8251 RVA: 0x0006584C File Offset: 0x00063A4C
		private bool DestroyVFXRenderer(BaseAbility ability)
		{
			int abilityInstanceID = AbilityExtensionAssetBase.GetAbilityInstanceID(ability);
			PullingProjectileAbilityVFXController.VFXRenderer vfxrenderer;
			if (!this.vfxRenderers.TryGetValue(abilityInstanceID, out vfxrenderer))
			{
				return false;
			}
			((PullingProjectileAbility)ability).PullingPointUpdated -= this.OnPullingPointUpdated;
			vfxrenderer.Destroy();
			this.vfxRenderers.Remove(abilityInstanceID);
			return true;
		}

		// Token: 0x0600203C RID: 8252 RVA: 0x000658A0 File Offset: 0x00063AA0
		public void GetRenderers(PullingProjectileAbility ability, out GameObject hookRenderer, out GameObject ropeRenderer)
		{
			hookRenderer = null;
			ropeRenderer = null;
			PullingProjectileAbilityVFXController.VFXRenderer vfxrenderer;
			if (this.vfxRenderers.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out vfxrenderer))
			{
				SpriteRenderer hookRenderer2 = vfxrenderer.HookRenderer;
				GameObject gameObject;
				if ((gameObject = ((hookRenderer2 != null) ? hookRenderer2.gameObject : null)) == null)
				{
					ProjectileRendererComponent projectileRenderer = vfxrenderer.ProjectileRenderer;
					gameObject = ((projectileRenderer != null) ? projectileRenderer.gameObject : null);
				}
				hookRenderer = gameObject;
				LineRenderer ropeRenderer2 = vfxrenderer.RopeRenderer;
				ropeRenderer = ((ropeRenderer2 != null) ? ropeRenderer2.gameObject : null);
			}
		}

		// Token: 0x0600203D RID: 8253 RVA: 0x00065908 File Offset: 0x00063B08
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			PullingProjectileAbility pullingProjectileAbility = ability as PullingProjectileAbility;
			if (pullingProjectileAbility != null)
			{
				pullingProjectileAbility.ProjectileLaunched += this.OnAbilityProjectileLaunched;
				pullingProjectileAbility.PullingAttemptTaken += this.OnAbilityPullingAttemptTaken;
				pullingProjectileAbility.Completed += this.OnAbilityCompleted;
			}
		}

		// Token: 0x0600203E RID: 8254 RVA: 0x0006595C File Offset: 0x00063B5C
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			PullingProjectileAbility pullingProjectileAbility = ability as PullingProjectileAbility;
			if (pullingProjectileAbility != null)
			{
				pullingProjectileAbility.ProjectileLaunched -= this.OnAbilityProjectileLaunched;
				pullingProjectileAbility.PullingAttemptTaken -= this.OnAbilityPullingAttemptTaken;
				pullingProjectileAbility.Completed -= this.OnAbilityCompleted;
				this.DestroyVFXRenderer(ability);
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x0600203F RID: 8255 RVA: 0x000659B8 File Offset: 0x00063BB8
		private void OnAbilityProjectileLaunched(ProjectileAbilityBase.LaunchEventArgs launchArgs)
		{
			if (this.pullingRopeRendererPrefab == null)
			{
				return;
			}
			LineRenderer componentOrDestroy = UnityEngine.Object.Instantiate<GameObject>(this.pullingRopeRendererPrefab).GetComponentOrDestroy<LineRenderer>();
			if (componentOrDestroy == null)
			{
				return;
			}
			this.vfxRenderers.Add(AbilityExtensionAssetBase.GetAbilityInstanceID(launchArgs.ability), new PullingProjectileAbilityVFXController.VFXRenderer(launchArgs, componentOrDestroy));
			((PullingProjectileAbility)launchArgs.ability).PullingPointUpdated += this.OnPullingPointUpdated;
		}

		// Token: 0x06002040 RID: 8256 RVA: 0x00065A28 File Offset: 0x00063C28
		private void OnAbilityPullingAttemptTaken(PullingProjectileAbility ability, GameMobKinematicMotionBase pullingMotion)
		{
			PullingProjectileAbilityVFXController.VFXRenderer vfxrenderer;
			if (this.vfxRenderers.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out vfxrenderer))
			{
				vfxrenderer.ActivateHookRenderer();
			}
		}

		// Token: 0x06002041 RID: 8257 RVA: 0x00065A50 File Offset: 0x00063C50
		private void OnPullingPointUpdated(PullingProjectileAbility ability, Vector2 pullingPoint)
		{
			this.vfxRenderers[AbilityExtensionAssetBase.GetAbilityInstanceID(ability)].Update(pullingPoint);
		}

		// Token: 0x06002042 RID: 8258 RVA: 0x00065A69 File Offset: 0x00063C69
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.DestroyVFXRenderer((BaseAbility)ability);
		}

		// Token: 0x0400144A RID: 5194
		public GameObject pullingRopeRendererPrefab;

		// Token: 0x0400144B RID: 5195
		private readonly Dictionary<int, PullingProjectileAbilityVFXController.VFXRenderer> vfxRenderers = new Dictionary<int, PullingProjectileAbilityVFXController.VFXRenderer>(32);

		// Token: 0x02000586 RID: 1414
		private sealed class VFXRenderer
		{
			// Token: 0x06002773 RID: 10099 RVA: 0x0007B4A8 File Offset: 0x000796A8
			private void InitializeRopeRenderer(string name, Vector3 initialPosition)
			{
				this.RopeRenderer.name = name;
				this.RopeRenderer.useWorldSpace = true;
				this.RopeRenderer.positionCount = 2;
				this.RopeRenderer.SetPosition(0, initialPosition);
			}

			// Token: 0x06002774 RID: 10100 RVA: 0x0007B4DC File Offset: 0x000796DC
			private SpriteRenderer CreateHookRenderer(string name)
			{
				ProjectileRendererComponent projectileRenderer = this.ProjectileRenderer;
				SpriteRenderer spriteRenderer = (projectileRenderer != null) ? projectileRenderer.SpriteRenderer : null;
				if (spriteRenderer == null)
				{
					return null;
				}
				SpriteRenderer spriteRenderer2 = new GameObject(name).AddComponent<SpriteRenderer>();
				spriteRenderer2.sortingLayerID = spriteRenderer.sortingLayerID;
				spriteRenderer2.sortingOrder = spriteRenderer.sortingOrder + 1;
				spriteRenderer2.sharedMaterial = spriteRenderer.sharedMaterial;
				spriteRenderer2.drawMode = spriteRenderer.drawMode;
				spriteRenderer2.enabled = false;
				return spriteRenderer2;
			}

			// Token: 0x06002775 RID: 10101 RVA: 0x0007B54C File Offset: 0x0007974C
			public VFXRenderer(ProjectileAbilityBase.LaunchEventArgs projectileLaunchArgs, LineRenderer ropeRenderer)
			{
				ProjectileAbilityBase ability = projectileLaunchArgs.ability;
				IProjectile launchedProjectile = projectileLaunchArgs.launchedProjectile;
				this.ProjectileRenderer = (launchedProjectile.AttachedObject as ProjectileRendererComponent);
				this.RopeRenderer = ropeRenderer;
				this.InitializeRopeRenderer(string.Format("ropeRenderer_{0}", launchedProjectile.InstanceID), launchedProjectile.Position);
				this.HookRenderer = this.CreateHookRenderer(string.Format("hookRenderer_{0}", launchedProjectile.InstanceID));
				Vector3 vector = launchedProjectile.Position;
				if (projectileLaunchArgs.launchPointIndex >= 0)
				{
					ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint = ability.ProjectileLaunchPoints[projectileLaunchArgs.launchPointIndex];
					this.ropeStartPivot = projectileLaunchPoint.Transform;
					this.ropeStartPointOffset = projectileLaunchPoint.LocalPosition;
				}
				else if (ability.OwnerBehaviour != null)
				{
					this.ropeStartPivot = ability.OwnerBehaviour.transform;
					this.ropeStartPointOffset = vector - this.ropeStartPivot.position;
					this.ropeStartPointOffset.z = 0f;
				}
				this.Update(vector);
			}

			// Token: 0x06002776 RID: 10102 RVA: 0x0007B664 File Offset: 0x00079864
			public void ActivateHookRenderer()
			{
				if (this.ProjectileRenderer != null && this.HookRenderer != null && !this.HookRenderer.enabled)
				{
					SpriteRenderer spriteRenderer = this.ProjectileRenderer.SpriteRenderer;
					AngleBasedSpriteSelector.SpriteData spriteData;
					if (this.ProjectileRenderer.LaunchDirectionSprites.TryGetCurrentSpriteData(out spriteData))
					{
						spriteData.Apply(this.HookRenderer);
					}
					else
					{
						Sprite sprite = (spriteRenderer != null) ? spriteRenderer.sprite : null;
						if (sprite != null)
						{
							this.HookRenderer.sprite = sprite;
						}
					}
					Transform transform = this.ProjectileRenderer.transform;
					if (spriteRenderer != null)
					{
						this.HookRenderer.size = spriteRenderer.size;
					}
					this.HookRenderer.transform.SetPositionAndRotation(transform.position, transform.rotation);
					this.HookRenderer.enabled = true;
				}
			}

			// Token: 0x06002777 RID: 10103 RVA: 0x0007B744 File Offset: 0x00079944
			public void Update(Vector2 pullingPoint)
			{
				if (this.HookRenderer != null && this.HookRenderer.enabled)
				{
					this.HookRenderer.transform.position = pullingPoint;
				}
				if (this.ropeStartPivot != null)
				{
					this.RopeRenderer.SetPosition(0, this.ropeStartPivot.position + this.ropeStartPointOffset);
				}
				this.RopeRenderer.SetPosition(1, pullingPoint);
			}

			// Token: 0x06002778 RID: 10104 RVA: 0x0007B7C4 File Offset: 0x000799C4
			public void Destroy()
			{
				if (GameApplication.IsGameStateChanging)
				{
					return;
				}
				if (this.HookRenderer != null)
				{
					UnityEngine.Object.Destroy(this.HookRenderer.gameObject);
				}
				if (this.RopeRenderer != null)
				{
					UnityEngine.Object.Destroy(this.RopeRenderer.gameObject);
				}
			}

			// Token: 0x04001CB1 RID: 7345
			public readonly LineRenderer RopeRenderer;

			// Token: 0x04001CB2 RID: 7346
			public readonly SpriteRenderer HookRenderer;

			// Token: 0x04001CB3 RID: 7347
			public readonly ProjectileRendererComponent ProjectileRenderer;

			// Token: 0x04001CB4 RID: 7348
			private readonly Transform ropeStartPivot;

			// Token: 0x04001CB5 RID: 7349
			private readonly Vector3 ropeStartPointOffset;
		}
	}
}
