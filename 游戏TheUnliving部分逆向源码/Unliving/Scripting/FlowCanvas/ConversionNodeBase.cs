using System;
using FlowCanvas;
using FlowCanvas.Nodes;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000090 RID: 144
	public abstract class ConversionNodeBase<T> : FlowControlNode
	{
		// Token: 0x060003E0 RID: 992 RVA: 0x0000D768 File Offset: 0x0000B968
		private T Convert()
		{
			string text = this.value.value;
			if (!this.isInitialized || !string.Equals(text, this.lastInput, StringComparison.OrdinalIgnoreCase))
			{
				this.currentValue = this.Convert(text);
				this.lastInput = text;
				this.isInitialized = true;
			}
			return this.currentValue;
		}

		// Token: 0x060003E1 RID: 993
		protected abstract T Convert(string inputValue);

		// Token: 0x060003E2 RID: 994 RVA: 0x0000D7B9 File Offset: 0x0000B9B9
		protected override void RegisterPorts()
		{
			this.value = base.AddValueInput<string>("value", "");
			this.convertedValue = base.AddValueOutput<T>("convertedValue", new ValueHandler<T>(this.Convert), "");
		}

		// Token: 0x04000261 RID: 609
		private ValueInput<string> value;

		// Token: 0x04000262 RID: 610
		private ValueOutput<T> convertedValue;

		// Token: 0x04000263 RID: 611
		private string lastInput;

		// Token: 0x04000264 RID: 612
		private T currentValue;

		// Token: 0x04000265 RID: 613
		private bool isInitialized;
	}
}
