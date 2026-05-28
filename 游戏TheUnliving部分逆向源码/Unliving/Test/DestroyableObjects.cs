using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Damage;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x02000037 RID: 55
	public class DestroyableObjects : GameBehaviourBase, IDamageable, IHitPointsSource
	{
		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060001DC RID: 476 RVA: 0x000079CC File Offset: 0x00005BCC
		// (set) Token: 0x060001DD RID: 477 RVA: 0x000079CF File Offset: 0x00005BCF
		public MonoBehaviour Behaviour
		{
			get
			{
				return this;
			}
			set
			{
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060001DE RID: 478 RVA: 0x000079D1 File Offset: 0x00005BD1
		public Transform Transform
		{
			get
			{
				return base.transform;
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060001DF RID: 479 RVA: 0x000079D9 File Offset: 0x00005BD9
		public bool IsAlive
		{
			get
			{
				return this.isAlive;
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060001E0 RID: 480 RVA: 0x000079E1 File Offset: 0x00005BE1
		public float HitPointsLack
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060001E1 RID: 481 RVA: 0x000079E8 File Offset: 0x00005BE8
		public float MaxHitPoints
		{
			get
			{
				return this.InitialHitPoints;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060001E2 RID: 482 RVA: 0x000079F0 File Offset: 0x00005BF0
		// (set) Token: 0x060001E3 RID: 483 RVA: 0x000079F7 File Offset: 0x00005BF7
		public float InitialHitPoints
		{
			get
			{
				return 100f;
			}
			set
			{
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x060001E4 RID: 484 RVA: 0x000079F9 File Offset: 0x00005BF9
		public float CurrentHitPoints
		{
			get
			{
				if (!this.isAlive)
				{
					return -1f;
				}
				return 100f;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x060001E5 RID: 485 RVA: 0x00007A0E File Offset: 0x00005C0E
		IDamageable IDamageable.ParentDamageReceiver
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x060001E6 RID: 486 RVA: 0x00007A11 File Offset: 0x00005C11
		object IDamageable.LastDamageSender
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x060001E7 RID: 487 RVA: 0x00007A14 File Offset: 0x00005C14
		object IDamageable.LastDamageSource
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060001E8 RID: 488 RVA: 0x00007A17 File Offset: 0x00005C17
		IReadOnlyList<IHitPointsSource> IDamageable.AdditionalHitPointsSources
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x060001E9 RID: 489 RVA: 0x00007A1A File Offset: 0x00005C1A
		// (set) Token: 0x060001EA RID: 490 RVA: 0x00007A1D File Offset: 0x00005C1D
		IHitPointsSource IHitPointsSource.ParentHitPointsSource
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x060001EB RID: 491 RVA: 0x00007A20 File Offset: 0x00005C20
		// (remove) Token: 0x060001EC RID: 492 RVA: 0x00007A58 File Offset: 0x00005C58
		public event Action<float> InitialHitPointsChanged;

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x060001ED RID: 493 RVA: 0x00007A90 File Offset: 0x00005C90
		// (remove) Token: 0x060001EE RID: 494 RVA: 0x00007AC8 File Offset: 0x00005CC8
		public event Action<IHitPointsSource, object, IHitPointsChangingArgs> BeforeHitPointsChanged;

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x060001EF RID: 495 RVA: 0x00007B00 File Offset: 0x00005D00
		// (remove) Token: 0x060001F0 RID: 496 RVA: 0x00007B38 File Offset: 0x00005D38
		public event Action<IHitPointsSource, object, IHitPointsChangingArgs> HitPointsChanged;

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x060001F1 RID: 497 RVA: 0x00007B70 File Offset: 0x00005D70
		// (remove) Token: 0x060001F2 RID: 498 RVA: 0x00007BA8 File Offset: 0x00005DA8
		public event Action<IDamageable, object, IHitPointsChangingArgs> HitReceived;

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x060001F3 RID: 499 RVA: 0x00007BE0 File Offset: 0x00005DE0
		// (remove) Token: 0x060001F4 RID: 500 RVA: 0x00007C18 File Offset: 0x00005E18
		public event Action<IDamageable> TotallyDestroyed;

		// Token: 0x060001F5 RID: 501 RVA: 0x00007C4D File Offset: 0x00005E4D
		private void Start()
		{
			this.isAlive = this.canBeDestroyed;
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x00007C5B File Offset: 0x00005E5B
		public float ModifyHitPoints(object sender, IHitPointsChangingArgs args)
		{
			if (args.IsDamage)
			{
				if (this.isAlive)
				{
					this.DestroyObject();
				}
				return -1f;
			}
			if (!this.isAlive)
			{
				return -1f;
			}
			return 100f;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x00007C8C File Offset: 0x00005E8C
		public void ApplyLethalDamage(object damageSender = null)
		{
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00007C8E File Offset: 0x00005E8E
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (!this.isAlive)
			{
				return;
			}
			if (this.layerMask == (this.layerMask | 1 << collider.gameObject.layer))
			{
				this.DestroyObject();
			}
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x00007CC8 File Offset: 0x00005EC8
		private void DestroyObject()
		{
			this.isAlive = true;
			GameObject[] array = this.particleSystems;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
			SpriteRenderer[] array2 = this.renderers;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = false;
			}
			base.GetComponent<Collider2D>().enabled = false;
			UnityEngine.Object.Destroy(base.gameObject, 0.5f + this.particleSystems.Max((GameObject ps) => ps.GetComponent<ParticleSystem>().main.duration));
		}

		// Token: 0x060001FA RID: 506 RVA: 0x00007D5F File Offset: 0x00005F5F
		bool IDamageable.AddAdditionalHitPointsSource(IHitPointsSource hitPointsSource)
		{
			return false;
		}

		// Token: 0x060001FB RID: 507 RVA: 0x00007D62 File Offset: 0x00005F62
		bool IDamageable.RemoveAdditionalHitPointsSource(IHitPointsSource hitPointsSource)
		{
			return false;
		}

		// Token: 0x060001FC RID: 508 RVA: 0x00007D65 File Offset: 0x00005F65
		public bool HasEnoughEnergy(float energyAmount)
		{
			return this.CurrentHitPoints > 0f;
		}

		// Token: 0x040000FD RID: 253
		[SerializeField]
		private bool canBeDestroyed = true;

		// Token: 0x040000FE RID: 254
		[SerializeField]
		private LayerMask layerMask;

		// Token: 0x040000FF RID: 255
		[SerializeField]
		private SpriteRenderer[] renderers;

		// Token: 0x04000100 RID: 256
		[SerializeField]
		private GameObject[] particleSystems;

		// Token: 0x04000101 RID: 257
		private bool isAlive;
	}
}
