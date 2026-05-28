using System;
using UnityEngine;

namespace Unliving.Misc
{
	// Token: 0x02000248 RID: 584
	public sealed class RandomObjectDestroyer : MonoBehaviour
	{
		// Token: 0x060013A6 RID: 5030 RVA: 0x0003D938 File Offset: 0x0003BB38
		private void Awake()
		{
			float value = UnityEngine.Random.value;
			if (this.destroyChance >= value)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x04000B74 RID: 2932
		[Header("Шанс на уничтожение объекта")]
		public float destroyChance = 0.5f;
	}
}
