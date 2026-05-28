using System;
using System.Collections.Generic;
using Common;
using Common.UnityExtensions;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Core;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000BF RID: 191
	public abstract class ObjectEventBusNodeBase : EventNode
	{
		// Token: 0x060004CD RID: 1229 RVA: 0x00011600 File Offset: 0x0000F800
		private static IDestroyable GetOrAddDestructionEventProvider(GameObject targetObject)
		{
			IDestroyable result;
			if (!targetObject.TryGetComponent<IDestroyable>(out result))
			{
				result = targetObject.AddComponent<DestructionEventComponent>();
			}
			return result;
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x00011620 File Offset: 0x0000F820
		private void Initialize(Flow flow)
		{
			if (this.isInitialized)
			{
				return;
			}
			bool flag = false;
			using (IEnumerator<FlowOutput> enumerator = base.GetOutputFlowPorts().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.isConnected)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				this.isInitialized = true;
				return;
			}
			this.OnInitialize(flow);
			this.isInitialized = true;
			this.flowOut.Call(flow);
			GameObject eventsSourceObject = this.GetEventsSourceObject();
			this.destructibleGraphAgent = ObjectEventBusNodeBase.GetOrAddDestructionEventProvider(base.graphAgent.gameObject);
			this.destructibleGraphAgent.Destroyed += this.OnObjectDestroyed;
			if (eventsSourceObject != null && eventsSourceObject != base.graphAgent.gameObject)
			{
				this.destructibleEventsSource = ObjectEventBusNodeBase.GetOrAddDestructionEventProvider(eventsSourceObject);
				this.destructibleEventsSource.Destroyed += this.OnObjectDestroyed;
			}
			GameApplication.SceneLoadingStarted += this.SetFinalized;
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x00011724 File Offset: 0x0000F924
		private void SetFinalized()
		{
			if (this.isFinalized)
			{
				return;
			}
			this.OnFinalize();
			this.isFinalized = true;
			if (!this.destructibleGraphAgent.IsNull())
			{
				this.destructibleGraphAgent.Destroyed -= this.OnObjectDestroyed;
			}
			if (!this.destructibleEventsSource.IsNull())
			{
				this.destructibleEventsSource.Destroyed -= this.OnObjectDestroyed;
			}
			GameApplication.SceneLoadingStarted -= this.SetFinalized;
		}

		// Token: 0x060004D0 RID: 1232
		protected abstract GameObject GetEventsSourceObject();

		// Token: 0x060004D1 RID: 1233 RVA: 0x000117A0 File Offset: 0x0000F9A0
		protected override void RegisterPorts()
		{
			this.initialize = base.AddFlowInput("initialize", new FlowHandler(this.Initialize), "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x060004D2 RID: 1234 RVA: 0x000117DA File Offset: 0x0000F9DA
		private void OnObjectDestroyed(object destroyable)
		{
			this.SetFinalized();
		}

		// Token: 0x060004D3 RID: 1235
		protected abstract void OnInitialize(Flow flow);

		// Token: 0x060004D4 RID: 1236
		protected abstract void OnFinalize();

		// Token: 0x060004D5 RID: 1237 RVA: 0x000117E4 File Offset: 0x0000F9E4
		public override void OnPostGraphStarted()
		{
			if (!this.initialize.isConnected)
			{
				this.Initialize(default(Flow));
			}
		}

		// Token: 0x0400033C RID: 828
		private FlowInput initialize;

		// Token: 0x0400033D RID: 829
		private FlowOutput flowOut;

		// Token: 0x0400033E RID: 830
		private IDestroyable destructibleGraphAgent;

		// Token: 0x0400033F RID: 831
		private IDestroyable destructibleEventsSource;

		// Token: 0x04000340 RID: 832
		private bool isInitialized;

		// Token: 0x04000341 RID: 833
		private bool isFinalized;
	}
}
