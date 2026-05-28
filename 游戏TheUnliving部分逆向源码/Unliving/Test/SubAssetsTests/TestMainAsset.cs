using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unliving.Test.SubAssetsTests
{
	// Token: 0x02000045 RID: 69
	[CreateAssetMenu(fileName = "TestMainAsset", menuName = "Game/Test/Test Main Asset")]
	public sealed class TestMainAsset : ScriptableObject
	{
		// Token: 0x04000170 RID: 368
		public List<TestSubAsset> subAssets;
	}
}
