using System;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000AB RID: 171
	[Name("Comment", 0)]
	[Category("Unliving/Misc")]
	public sealed class CommentNode : FlowNode
	{
		// Token: 0x06000463 RID: 1123 RVA: 0x0000F578 File Offset: 0x0000D778
		protected override void RegisterPorts()
		{
		}

		// Token: 0x040002C9 RID: 713
		[HideInInspector]
		public string text;
	}
}
