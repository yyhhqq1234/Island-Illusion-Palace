using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000378 RID: 888
	[CreateAssetMenu(fileName = "AbilityShootingPointsSetter", menuName = "Abilities/Controllers/Shooting Points Setter")]
	public sealed class AbilityShootingPointsSetter : AbilityExtensionAssetBase
	{
		// Token: 0x1700060C RID: 1548
		// (get) Token: 0x06001D39 RID: 7481 RVA: 0x0005C632 File Offset: 0x0005A832
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D3A RID: 7482 RVA: 0x0005C638 File Offset: 0x0005A838
		private void GetShootingPointsTransform(ProjectileAbilityBase ability, out Transform targetTransform, out Vector3 localPosition)
		{
			BaseGameMob baseGameMob = (BaseGameMob)ability.Owner;
			localPosition = default(Vector3);
			if (this.attachToBaseShootingPoints)
			{
				ProjectileAbilityBase.ProjectileLaunchPoint[] array = baseGameMob.ShootingPoints;
				targetTransform = null;
				if (array != null && array.Length != 0)
				{
					foreach (ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint in array)
					{
						if (targetTransform == null)
						{
							targetTransform = projectileLaunchPoint.Transform;
						}
						localPosition += projectileLaunchPoint.LocalPosition;
					}
					localPosition /= (float)array.Length;
					return;
				}
			}
			targetTransform = baseGameMob.transform;
		}

		// Token: 0x06001D3B RID: 7483 RVA: 0x0005C6D0 File Offset: 0x0005A8D0
		private ProjectileAbilityBase.ProjectileLaunchPoint[] CreateShootingPoints(ProjectileAbilityBase ability)
		{
			Transform transform = UnityEngine.Object.Instantiate<GameObject>(this.shootingPointsPrefab).transform;
			ProjectileAbilityBase.ProjectileLaunchPoint[] array = new ProjectileAbilityBase.ProjectileLaunchPoint[transform.childCount];
			Transform transform2;
			Vector3 a;
			this.GetShootingPointsTransform(ability, out transform2, out a);
			UnityEngine.Object x = transform2;
			MonoBehaviour ownerBehaviour = ability.OwnerBehaviour;
			bool flag = x == ((ownerBehaviour != null) ? ownerBehaviour.transform : null);
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Transform child = transform.GetChild(i);
				ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint = new ProjectileAbilityBase.ProjectileLaunchPoint
				{
					LocalPosition = a + child.localPosition,
					Direction = (this.ignoreShootingPointDirection ? default(Vector3) : (child.localRotation * new Vector3
					{
						x = 1f
					})),
					rotateTowardsTarget = true,
					Transform = (flag ? child : transform2),
					pointObject = child.gameObject
				};
				child.parent = transform2;
				child.localPosition = projectileLaunchPoint.LocalPosition;
				array[i] = projectileLaunchPoint;
			}
			ability.ProjectileLaunchPoints = array;
			UnityEngine.Object.Destroy(transform.gameObject);
			return array;
		}

		// Token: 0x06001D3C RID: 7484 RVA: 0x0005C7F8 File Offset: 0x0005A9F8
		protected override void OnAbilityOwnerChanged(BaseAbility ability, object lastOwner, object newOwner)
		{
			if (newOwner != null && this.shootingPointsPrefab != null && this.shootingPointsPrefab.transform.childCount != 0)
			{
				ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
				if (projectileAbilityBase != null)
				{
					this.shootingPoints[AbilityExtensionAssetBase.GetAbilityInstanceID(ability)] = this.CreateShootingPoints(projectileAbilityBase);
				}
			}
		}

		// Token: 0x06001D3D RID: 7485 RVA: 0x0005C84C File Offset: 0x0005AA4C
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			int abilityInstanceID = AbilityExtensionAssetBase.GetAbilityInstanceID(ability);
			ProjectileAbilityBase.ProjectileLaunchPoint[] array;
			if (this.shootingPoints.TryGetValue(abilityInstanceID, out array))
			{
				ProjectileAbilityBase projectileAbilityBase = (ProjectileAbilityBase)ability;
				if (array == projectileAbilityBase.ProjectileLaunchPoints)
				{
					for (int i = 0; i < array.Length; i++)
					{
						GameObject pointObject = array[i].pointObject;
						if (pointObject != null)
						{
							UnityEngine.Object.Destroy(pointObject);
						}
					}
					ProjectileAbilityBase projectileAbilityBase2 = projectileAbilityBase;
					BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
					projectileAbilityBase2.ProjectileLaunchPoints = ((baseGameMob != null) ? baseGameMob.ShootingPoints : null);
				}
				this.shootingPoints.Remove(abilityInstanceID);
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x04001082 RID: 4226
		public GameObject shootingPointsPrefab;

		// Token: 0x04001083 RID: 4227
		public bool attachToBaseShootingPoints = true;

		// Token: 0x04001084 RID: 4228
		public bool ignoreShootingPointDirection;

		// Token: 0x04001085 RID: 4229
		private readonly Dictionary<int, ProjectileAbilityBase.ProjectileLaunchPoint[]> shootingPoints = new Dictionary<int, ProjectileAbilityBase.ProjectileLaunchPoint[]>(8);
	}
}
