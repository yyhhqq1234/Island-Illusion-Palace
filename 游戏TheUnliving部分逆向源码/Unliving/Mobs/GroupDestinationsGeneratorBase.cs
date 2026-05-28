using System;
using UnityEngine;
using Unliving.Mobs.Motion;

namespace Unliving.Mobs
{
	// Token: 0x020001F6 RID: 502
	[RequireComponent(typeof(MobBehaviourSpawner))]
	public abstract class GroupDestinationsGeneratorBase : MonoBehaviour, IGroupDestinationsGenerator
	{
		// Token: 0x17000375 RID: 885
		// (get) Token: 0x060010C6 RID: 4294 RVA: 0x00034C57 File Offset: 0x00032E57
		// (set) Token: 0x060010C7 RID: 4295 RVA: 0x00034C5F File Offset: 0x00032E5F
		public bool IsActive
		{
			get
			{
				return base.enabled;
			}
			set
			{
				base.enabled = value;
			}
		}

		// Token: 0x17000376 RID: 886
		// (get) Token: 0x060010C8 RID: 4296
		public abstract bool GenerateIndividualMobDestinations { get; }

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x060010C9 RID: 4297 RVA: 0x00034C68 File Offset: 0x00032E68
		public bool PassToSumoningGroups
		{
			get
			{
				return this._passToSumoningGroups;
			}
		}

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x060010CA RID: 4298 RVA: 0x00034C70 File Offset: 0x00032E70
		// (set) Token: 0x060010CB RID: 4299 RVA: 0x00034C78 File Offset: 0x00032E78
		public GameMobsGroupControllerBase CurrentGroup { get; set; }

		// Token: 0x060010CC RID: 4300 RVA: 0x00034C81 File Offset: 0x00032E81
		public IGroupDestinationsGenerator Clone(GameMobsGroupControllerBase targetGroup)
		{
			if (targetGroup.GroupHolder != null)
			{
				GroupDestinationsGeneratorBase groupDestinationsGeneratorBase = (GroupDestinationsGeneratorBase)targetGroup.GroupHolder.AddComponent(base.GetType());
				groupDestinationsGeneratorBase.CurrentGroup = targetGroup;
				groupDestinationsGeneratorBase.OnCloned(this, this.passCurrentStateToClone);
				return groupDestinationsGeneratorBase;
			}
			return null;
		}

		// Token: 0x060010CD RID: 4301 RVA: 0x00034CBD File Offset: 0x00032EBD
		public virtual bool CanGenerateNewIndividualDestination(GameMobMotionController mobMotionController)
		{
			return mobMotionController != null && (this.lastIndividualDestination == null || mobMotionController.IsPointReached(this.lastIndividualDestination.Value, 0f));
		}

		// Token: 0x060010CE RID: 4302 RVA: 0x00034CEE File Offset: 0x00032EEE
		public virtual bool TryGetNewDestination(out Vector2 destination, out bool isForcedPosition)
		{
			destination = default(Vector2);
			isForcedPosition = this.generateForcedDestinations;
			return this.IsActive;
		}

		// Token: 0x060010CF RID: 4303 RVA: 0x00034D08 File Offset: 0x00032F08
		public bool TryGetIndividualDestination(GameMobMotionController mobMotionController, out Vector2 destination)
		{
			if (this.CanGenerateNewIndividualDestination(mobMotionController))
			{
				bool flag;
				if (this.TryGetNewDestination(out destination, out flag))
				{
					this.lastIndividualDestination = new Vector3?(destination);
					return true;
				}
			}
			else if (this.lastIndividualDestination != null)
			{
				destination = this.lastIndividualDestination.Value;
				return true;
			}
			destination = default(Vector2);
			return false;
		}

		// Token: 0x060010D0 RID: 4304 RVA: 0x00034D6E File Offset: 0x00032F6E
		public virtual bool TryGetAdditionalVelocity(Vector2 mobPosition, float mobSpeed, out Vector2 additionalVelocity)
		{
			additionalVelocity = default(Vector2);
			return false;
		}

		// Token: 0x060010D1 RID: 4305 RVA: 0x00034D78 File Offset: 0x00032F78
		public virtual void OnCloned(GroupDestinationsGeneratorBase originalDestinationsGenerator, bool keepState)
		{
			this.generateForcedDestinations = originalDestinationsGenerator.generateForcedDestinations;
			this._passToSumoningGroups = originalDestinationsGenerator.PassToSumoningGroups;
			this.passCurrentStateToClone = keepState;
		}

		// Token: 0x060010D2 RID: 4306 RVA: 0x00034D99 File Offset: 0x00032F99
		protected virtual void OnEnable()
		{
		}

		// Token: 0x0400098B RID: 2443
		public bool generateForcedDestinations;

		// Token: 0x0400098C RID: 2444
		[SerializeField]
		private bool _passToSumoningGroups;

		// Token: 0x0400098D RID: 2445
		[SerializeField]
		private bool passCurrentStateToClone;

		// Token: 0x0400098E RID: 2446
		private Vector3? lastIndividualDestination;
	}
}
