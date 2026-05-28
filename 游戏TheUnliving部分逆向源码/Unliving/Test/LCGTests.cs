using System;
using System.Collections;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x0200003C RID: 60
	public class LCGTests : MonoBehaviour
	{
		// Token: 0x0600020E RID: 526 RVA: 0x0000838F File Offset: 0x0000658F
		private IEnumerator Start()
		{
			yield return new WaitForSecondsRealtime(0.1f);
			yield break;
		}

		// Token: 0x04000125 RID: 293
		private static readonly int[] Primes = new int[]
		{
			79139,
			28001,
			95989,
			45389,
			72253,
			43093,
			93893,
			45613,
			94993,
			36973,
			44633,
			79357,
			28927,
			25931,
			4261,
			5039,
			14461,
			37463,
			97511,
			94201,
			37369,
			29959,
			2549,
			44641,
			37379,
			93199,
			44729,
			96643,
			52817,
			30469,
			60617,
			14699,
			25673,
			27011,
			2383,
			10007,
			41999,
			3217,
			95803,
			66179,
			27457,
			8297,
			22859,
			42979,
			44797,
			6029,
			33529,
			2803,
			63113,
			42737,
			35951,
			68399,
			1699,
			30013,
			19891,
			36373,
			55733,
			18149,
			70849,
			93239,
			54163,
			4759,
			61487,
			67757,
			16103,
			42499,
			49031,
			9067,
			93601,
			48799,
			11197,
			45179,
			19417,
			67261,
			70687,
			59453,
			50341,
			93251,
			13241,
			40813,
			3371,
			1217,
			15217,
			60727,
			84239,
			50423,
			7573,
			5479,
			43177,
			11317,
			30119,
			28697,
			89087,
			28751,
			57331,
			95723,
			83663,
			57241,
			7753,
			14519
		};

		// Token: 0x04000126 RID: 294
		private static readonly bool[] UsedIndicesBuffer = new bool[100];
	}
}
