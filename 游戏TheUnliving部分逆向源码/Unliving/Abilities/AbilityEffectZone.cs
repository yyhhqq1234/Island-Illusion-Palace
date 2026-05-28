using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using Game.Buffs;
using Game.Core;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000369 RID: 873
	public sealed class AbilityEffectZone : GameBehaviourBase
	{
		// Token: 0x06001CB2 RID: 7346 RVA: 0x0005A8D1 File Offset: 0x00058AD1
		private static int GetObjectID(GameObject obj)
		{
			return obj.GetInstanceID();
		}

		// Token: 0x06001CB3 RID: 7347 RVA: 0x0005A8DC File Offset: 0x00058ADC
		public static AbilityEffectZone Create(IBuffsGenerator[] buffsGenerators, Vector3 position, MonoBehaviour owner, float lifetime, float range)
		{
			AbilityEffectZone abilityEffectZone = new GameObject("EffectZone", new Type[]
			{
				typeof(CircleCollider2D)
			}).AddComponent<AbilityEffectZone>();
			abilityEffectZone.BuffsGenerators = buffsGenerators;
			abilityEffectZone.owner = owner;
			abilityEffectZone.areaLifetime = lifetime;
			abilityEffectZone.SetRange(range);
			abilityEffectZone.transform.position = position;
			return abilityEffectZone;
		}

		// Token: 0x170005F8 RID: 1528
		// (get) Token: 0x06001CB4 RID: 7348 RVA: 0x0005A934 File Offset: 0x00058B34
		// (set) Token: 0x06001CB5 RID: 7349 RVA: 0x0005A93C File Offset: 0x00058B3C
		public IBuffsGenerator[] BuffsGenerators
		{
			get
			{
				return this.buffsGenerators;
			}
			set
			{
				if (this.buffsGenerators == value)
				{
					return;
				}
				this.buffsGenerators = value;
			}
		}

		// Token: 0x170005F9 RID: 1529
		// (get) Token: 0x06001CB6 RID: 7350 RVA: 0x0005A94F File Offset: 0x00058B4F
		// (set) Token: 0x06001CB7 RID: 7351 RVA: 0x0005A958 File Offset: 0x00058B58
		public ParticleSystem VisualEffect
		{
			get
			{
				return this.visualEffect;
			}
			set
			{
				if (this.visualEffect == null && this.visualEffect != value)
				{
					this.visualEffect = value;
					float areaWorldSize = this.GetAreaWorldSize();
					if (areaWorldSize != 0f)
					{
						this.visualEffect.ScaleToFit(areaWorldSize, false);
					}
					this.UpdateVisualEffectTransform(false);
					ParticleSystem.MainModule main = this.visualEffect.main;
					main.playOnAwake = false;
					main.loop = true;
					this.visualEffect.Stop();
				}
			}
		}

		// Token: 0x170005FA RID: 1530
		// (get) Token: 0x06001CB8 RID: 7352 RVA: 0x0005A9D2 File Offset: 0x00058BD2
		public float RemainingLifetime
		{
			get
			{
				return this.remainingLifetime;
			}
		}

		// Token: 0x06001CB9 RID: 7353 RVA: 0x0005A9DC File Offset: 0x00058BDC
		private float GetAreaWorldSize()
		{
			if (this.areaCollider == null)
			{
				return 0f;
			}
			this.areaCollider.enabled = false;
			this.areaCollider.enabled = true;
			Vector2 vector = this.areaCollider.bounds.size;
			return Mathf.Max(vector.x, vector.y);
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x0005AA3F File Offset: 0x00058C3F
		private bool IsValidObject(GameObject obj)
		{
			return obj.InLayerMask(this.affectableObjectsLayers);
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x0005AA54 File Offset: 0x00058C54
		private bool IsValidBuffsReceiver(IBuffableObject buffsReceiver)
		{
			BaseGameMob baseGameMob = buffsReceiver as BaseGameMob;
			return baseGameMob == null || this.affectableMobsDescription.IsBlank() || this.affectableMobsDescription.IsMatch(baseGameMob);
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x0005AA88 File Offset: 0x00058C88
		private void ApplyTemporaryBuffs(IBuffableObject obj)
		{
			if (GameApplication.IsGameStateChanging)
			{
				return;
			}
			IBuffsController buffsController = (obj != null) ? obj.BuffsController : null;
			if (buffsController == null)
			{
				return;
			}
			for (int i = 0; i < this.buffsGenerators.Length; i++)
			{
				IBuffsGenerator buffsGenerator = this.buffsGenerators[i];
				if (buffsGenerator.BuffDuration >= 1f)
				{
					buffsController.AddBuff(buffsGenerator.GenerateBuff(this.owner, false));
				}
			}
		}

		// Token: 0x06001CBD RID: 7357 RVA: 0x0005AAEC File Offset: 0x00058CEC
		private void RegisterObject(IBuffableObject obj, int objectID)
		{
			if (this.buffsGenerators == null || this.buffsGenerators.Length == 0 || obj.IsNull() || obj.BuffsController == null)
			{
				return;
			}
			IBuffsController buffsController = obj.BuffsController;
			foreach (IBuff buff in buffsController.GetCurrentBuffs())
			{
				if (this.<RegisterObject>g__ShouldBeCompleted|29_0(buff))
				{
					buff.Complete();
				}
			}
			List<IBuff> list;
			if (!this.appliedConstantBuffs.TryGetValue(objectID, out list))
			{
				list = new List<IBuff>(this.buffsGenerators.Length);
				this.appliedConstantBuffs.Add(objectID, list);
			}
			for (int i = 0; i < this.buffsGenerators.Length; i++)
			{
				IBuff buff2 = this.buffsGenerators[i].GenerateBuff((this.owner != null) ? this.owner : this, true);
				if (buffsController.AddBuff(buff2))
				{
					list.Add(buff2);
					buff2.Completed += this.OnAppliedBuffCompleted;
				}
			}
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x0005ABFC File Offset: 0x00058DFC
		private void UnregisterObject(IBuffableObject obj, int objectID)
		{
			List<IBuff> list;
			if (this.appliedConstantBuffs.TryGetValue(objectID, out list))
			{
				this.ApplyTemporaryBuffs(obj);
				for (int i = list.Count - 1; i >= 0; i--)
				{
					IBuff buff = list[i];
					buff.Complete();
					buff.Invalidate();
				}
			}
		}

		// Token: 0x06001CBF RID: 7359 RVA: 0x0005AC48 File Offset: 0x00058E48
		private void UpdateVisualEffectTransform(bool force = false)
		{
			if (this.visualEffect.IsNull() || (!force && !base.transform.hasChanged))
			{
				return;
			}
			this.visualEffect.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
			base.transform.hasChanged = false;
		}

		// Token: 0x06001CC0 RID: 7360 RVA: 0x0005ACA5 File Offset: 0x00058EA5
		private void CreateVisualEffect()
		{
			if (!this.visualEffect.IsNull() || this.visualEffectPrefab == null)
			{
				return;
			}
			this.VisualEffect = UnityEngine.Object.Instantiate<GameObject>(this.visualEffectPrefab).GetComponentOrDestroy<ParticleSystem>();
		}

		// Token: 0x06001CC1 RID: 7361 RVA: 0x0005ACD9 File Offset: 0x00058ED9
		private void SetVisualEffectActive(bool isActive)
		{
			if (this.visualEffect.IsNull())
			{
				return;
			}
			if (isActive)
			{
				this.visualEffect.Play();
				return;
			}
			this.visualEffect.Stop();
		}

		// Token: 0x06001CC2 RID: 7362 RVA: 0x0005AD04 File Offset: 0x00058F04
		private void FinalizeZone()
		{
			this.isFinalized = true;
			this.visualEffect.DestroyAfterEmission(true, false);
			foreach (List<IBuff> list in this.appliedConstantBuffs.Values)
			{
				object obj = null;
				for (int i = 0; i < list.Count; i++)
				{
					IBuff buff = list[i];
					obj = buff.TargetObject;
					buff.Completed -= this.OnAppliedBuffCompleted;
					buff.Complete();
				}
				list.Clear();
				this.ApplyTemporaryBuffs((IBuffableObject)obj);
			}
			this.appliedConstantBuffs.Clear();
		}

		// Token: 0x06001CC3 RID: 7363 RVA: 0x0005ADC0 File Offset: 0x00058FC0
		public void SetRange(float newRange)
		{
			if (this.areaCollider == null || newRange <= 0f)
			{
				return;
			}
			float areaWorldSize = this.GetAreaWorldSize();
			if (areaWorldSize != 0f)
			{
				float num = newRange * 2f / areaWorldSize;
				base.transform.localScale *= num;
				this.visualEffect.ScaleToFit(areaWorldSize * num, false);
			}
		}

		// Token: 0x06001CC4 RID: 7364 RVA: 0x0005AE24 File Offset: 0x00059024
		public void Activate()
		{
			if (this.isActivated)
			{
				return;
			}
			if (this.buffsGenerators == null)
			{
				BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.buffsGeneratorsBuilders;
				generatorsBuilders.Instantiate(out this.buffsGenerators);
			}
			this.SetVisualEffectActive(true);
			this.isActivated = true;
			this.remainingLifetime = this.areaLifetime;
		}

		// Token: 0x06001CC5 RID: 7365 RVA: 0x0005AE6F File Offset: 0x0005906F
		public ParticleSystem ForceGetVisualEffect()
		{
			if (!this.isFinalized)
			{
				this.CreateVisualEffect();
			}
			return this.visualEffect;
		}

		// Token: 0x06001CC6 RID: 7366 RVA: 0x0005AE88 File Offset: 0x00059088
		private void OnAppliedBuffCompleted(IBuff buff)
		{
			List<IBuff> list;
			if (this.appliedConstantBuffs.TryGetValue(AbilityEffectZone.GetObjectID(((Component)buff.TargetObject).gameObject), out list))
			{
				list.Remove(buff);
			}
			buff.Completed -= this.OnAppliedBuffCompleted;
		}

		// Token: 0x06001CC7 RID: 7367 RVA: 0x0005AED3 File Offset: 0x000590D3
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (base.TryGetComponent<Collider2D>(out this.areaCollider))
			{
				this.areaCollider.isTrigger = true;
			}
			this.isFinalized = false;
		}

		// Token: 0x06001CC8 RID: 7368 RVA: 0x0005AEFD File Offset: 0x000590FD
		private void Start()
		{
			this.CreateVisualEffect();
			if (this.areaActivationDelay <= 0f)
			{
				this.Activate();
				return;
			}
			this.SetVisualEffectActive(false);
			this.targetActivationTime = Time.time + this.areaActivationDelay;
		}

		// Token: 0x06001CC9 RID: 7369 RVA: 0x0005AF34 File Offset: 0x00059134
		private void LateUpdate()
		{
			if (!this.isActivated)
			{
				if (Time.time > this.targetActivationTime)
				{
					this.Activate();
				}
				return;
			}
			this.UpdateVisualEffectTransform(false);
			if (this.areaLifetime > 0f)
			{
				this.remainingLifetime -= Time.deltaTime;
				if (this.remainingLifetime < 0f)
				{
					this.visualEffect.DestroyAfterEmission(true, false);
					this.visualEffect = null;
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		// Token: 0x06001CCA RID: 7370 RVA: 0x0005AFB0 File Offset: 0x000591B0
		private void OnTriggerEnter2D(Collider2D collider)
		{
			IBuffableObject buffableObject;
			if (this.IsValidObject(collider.gameObject) && collider.TryGetComponent<IBuffableObject>(out buffableObject) && this.IsValidBuffsReceiver(buffableObject))
			{
				this.RegisterObject(buffableObject, AbilityEffectZone.GetObjectID(collider.gameObject));
			}
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x0005AFF0 File Offset: 0x000591F0
		private void OnTriggerExit2D(Collider2D collider)
		{
			IBuffableObject obj;
			if (collider.TryGetComponent<IBuffableObject>(out obj))
			{
				this.UnregisterObject(obj, AbilityEffectZone.GetObjectID(collider.gameObject));
			}
		}

		// Token: 0x06001CCC RID: 7372 RVA: 0x0005B019 File Offset: 0x00059219
		private void OnDisable()
		{
			if (this.isActivated)
			{
				this.FinalizeZone();
			}
		}

		// Token: 0x06001CCE RID: 7374 RVA: 0x0005B054 File Offset: 0x00059254
		[CompilerGenerated]
		private bool <RegisterObject>g__ShouldBeCompleted|29_0(IBuff buff)
		{
			if (!buff.IsConstant && !buff.IsAdditive)
			{
				for (int i = 0; i < this.buffsGenerators.Length; i++)
				{
					if (buff.ID == this.buffsGenerators[i].BuffID)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0400103B RID: 4155
		public MonoBehaviour owner;

		// Token: 0x0400103C RID: 4156
		public LayerMask affectableObjectsLayers = -1;

		// Token: 0x0400103D RID: 4157
		[SerializeField]
		private BuffsGeneratorBuilderAsset.Reference[] buffsGeneratorsBuilders;

		// Token: 0x0400103E RID: 4158
		public float areaActivationDelay;

		// Token: 0x0400103F RID: 4159
		public float areaLifetime;

		// Token: 0x04001040 RID: 4160
		public GameMobDescription affectableMobsDescription = GameMobDescription.BlankDescription;

		// Token: 0x04001041 RID: 4161
		[Space]
		public GameObject visualEffectPrefab;

		// Token: 0x04001042 RID: 4162
		private readonly Dictionary<int, List<IBuff>> appliedConstantBuffs = new Dictionary<int, List<IBuff>>();

		// Token: 0x04001043 RID: 4163
		private Collider2D areaCollider;

		// Token: 0x04001044 RID: 4164
		private IBuffsGenerator[] buffsGenerators;

		// Token: 0x04001045 RID: 4165
		private ParticleSystem visualEffect;

		// Token: 0x04001046 RID: 4166
		private float targetActivationTime;

		// Token: 0x04001047 RID: 4167
		private float remainingLifetime;

		// Token: 0x04001048 RID: 4168
		private bool isActivated;

		// Token: 0x04001049 RID: 4169
		private bool isFinalized;
	}
}
