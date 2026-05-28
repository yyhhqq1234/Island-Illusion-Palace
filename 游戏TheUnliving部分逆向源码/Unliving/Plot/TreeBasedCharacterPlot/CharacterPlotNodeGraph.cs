using System;
using System.Collections.Generic;
using System.Linq;
using Common.CollectionsExtensions;
using GraphProcessor;
using UnityEngine;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x020002FF RID: 767
	[CreateAssetMenu(fileName = "CharacterPlotNodeGraph", menuName = "Game/Plot/Character Plot Node Graph")]
	public class CharacterPlotNodeGraph : BaseGraph, ICharacterPlot
	{
		// Token: 0x06001A0E RID: 6670 RVA: 0x000514FE File Offset: 0x0004F6FE
		protected override void OnEnable()
		{
			base.OnEnable();
			base.onGraphChanges += this.OnGraphChanges;
		}

		// Token: 0x06001A0F RID: 6671 RVA: 0x00051518 File Offset: 0x0004F718
		private void OnGraphChanges(GraphChanges changes)
		{
			CharacterPlotNodeBase characterPlotNodeBase = changes.addedNode as CharacterPlotNodeBase;
			if (characterPlotNodeBase != null)
			{
				characterPlotNodeBase.CreateNodeAsset();
			}
			CharacterPlotNodeBase characterPlotNodeBase2 = changes.removedNode as CharacterPlotNodeBase;
			if (characterPlotNodeBase2 != null)
			{
				characterPlotNodeBase2.RemoveNodeAsset();
			}
		}

		// Token: 0x06001A10 RID: 6672 RVA: 0x00051550 File Offset: 0x0004F750
		private bool IsActivePlotItem(CharacterPlotContext context, ICharacterPlotItem plotItem)
		{
			bool result;
			try
			{
				if (plotItem.DeactivationTrigger != null && plotItem.DeactivationTrigger.IsFired(context))
				{
					result = false;
				}
				else
				{
					result = (plotItem != null && (plotItem.Trigger == null || plotItem.Trigger.IsFired(context)));
				}
			}
			catch (ArgumentException ex)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"CharacterPlotNodeGraph \"",
					base.name,
					"-",
					plotItem.ID,
					"\" failed with exception: \"",
					ex.Message,
					"\""
				}));
				result = false;
			}
			return result;
		}

		// Token: 0x06001A11 RID: 6673 RVA: 0x000515F8 File Offset: 0x0004F7F8
		private bool IsAvailablePlotThreadNode(CharacterPlotContext context, CharacterPlotThreadNode plotThreadNode, CharacterPlotTreeProgress plotProgress)
		{
			CharacterPlotThread thread = plotThreadNode.nodeAsset.thread;
			if (thread != null && !plotProgress.IsCompletedPlotThread(thread, -1))
			{
				CharacterPlotThread parentPlotThread = plotThreadNode.ParentPlotThread;
				return (parentPlotThread == null || plotProgress.IsCompletedPlotThread(parentPlotThread, -1)) && this.IsActivePlotItem(context, thread);
			}
			return false;
		}

		// Token: 0x06001A12 RID: 6674 RVA: 0x00051640 File Offset: 0x0004F840
		private CharacterPlotThreadNode GetMainConversationsNode(CharacterPlotContext context, ConversationBranch conversationBranch, out int conversationIndex, out bool useMainConversationNodeForce)
		{
			useMainConversationNodeForce = false;
			CharacterPlotTreeProgress characterPlotTreeProgress = (CharacterPlotTreeProgress)context.characterPlotProgress;
			string b;
			int num;
			if (characterPlotTreeProgress.TryGetCurrentPlotThread(out b, out num))
			{
				CharacterPlotThreadNode characterPlotThreadNode = null;
				foreach (BaseNode baseNode in this.nodes)
				{
					CharacterPlotNodeBase characterPlotNodeBase = (CharacterPlotNodeBase)baseNode;
					if (characterPlotNodeBase.ID == b)
					{
						CharacterPlotThreadNode characterPlotThreadNode2 = characterPlotNodeBase as CharacterPlotThreadNode;
						if (characterPlotThreadNode2 != null)
						{
							characterPlotThreadNode = characterPlotThreadNode2;
							break;
						}
					}
				}
				if (characterPlotThreadNode.ConversationBranch == conversationBranch)
				{
					useMainConversationNodeForce = characterPlotTreeProgress.UseCurrentPlotThreadForce;
					if (useMainConversationNodeForce || this.IsAvailablePlotThreadNode(context, characterPlotThreadNode, characterPlotTreeProgress))
					{
						conversationIndex = num;
						return characterPlotThreadNode;
					}
				}
			}
			int num2 = int.MinValue;
			CharacterPlotThreadNode result = null;
			foreach (BaseNode baseNode2 in this.nodes)
			{
				CharacterPlotNodeBase characterPlotNodeBase2 = (CharacterPlotNodeBase)baseNode2;
				if (characterPlotNodeBase2.ConversationBranch == conversationBranch)
				{
					CharacterPlotThreadNode characterPlotThreadNode3 = characterPlotNodeBase2 as CharacterPlotThreadNode;
					if (characterPlotThreadNode3 != null && this.IsAvailablePlotThreadNode(context, characterPlotThreadNode3, characterPlotTreeProgress))
					{
						int priority = characterPlotThreadNode3.Priority;
						if (priority > num2)
						{
							result = characterPlotThreadNode3;
							num2 = priority;
						}
					}
				}
			}
			conversationIndex = 0;
			return result;
		}

		// Token: 0x06001A13 RID: 6675 RVA: 0x0005178C File Offset: 0x0004F98C
		private void GetAdditionalConversationsPools(CharacterPlotContext context, ConversationBranch conversationBranch, out CharacterPlotThread maxPriorityExpo, out CharacterPlotThread randomfiller)
		{
			maxPriorityExpo = null;
			randomfiller = null;
			List<CharacterPlotThread> list = new List<CharacterPlotThread>();
			foreach (BaseNode baseNode in this.nodes)
			{
				CharacterExtraConversationsNode characterExtraConversationsNode = ((CharacterPlotNodeBase)baseNode) as CharacterExtraConversationsNode;
				if (characterExtraConversationsNode != null && this.IsActivePlotItem(context, characterExtraConversationsNode.nodeAsset.thread))
				{
					CharacterPlotTreeProgress characterPlotTreeProgress = (CharacterPlotTreeProgress)context.characterPlotProgress;
					CharacterPlotThread thread = characterExtraConversationsNode.nodeAsset.thread;
					if (thread.ConversationBranch == conversationBranch && !characterPlotTreeProgress.IsCompletedPlotThread(thread, -1))
					{
						if (characterExtraConversationsNode.type == CharacterExtraConversationsNode.Type.ExpositionConversations)
						{
							if (maxPriorityExpo == null || characterExtraConversationsNode.Priority > maxPriorityExpo.Priority)
							{
								maxPriorityExpo = thread;
							}
						}
						else if (characterExtraConversationsNode.type == CharacterExtraConversationsNode.Type.FillingConversations)
						{
							list.Add(thread);
						}
					}
				}
			}
			if (list.Count != 0)
			{
				randomfiller = list.GetRandomItem(0, -1);
			}
		}

		// Token: 0x06001A14 RID: 6676 RVA: 0x00051880 File Offset: 0x0004FA80
		private CharactersConversation CreateConversation(CharacterPlotThread plotThread, CharactersConversation conversationPrototype, out CharacterPlotTreeItemRuntimeData runtimeData, int conversationIndex = -1)
		{
			if (conversationIndex < 0)
			{
				conversationIndex = plotThread.GetItemIndex(conversationPrototype);
			}
			CharactersConversation charactersConversation = conversationPrototype.Clone();
			charactersConversation.ID = string.Format("{0}_{1}", plotThread.ID, conversationIndex);
			runtimeData = new CharacterPlotTreeItemRuntimeData
			{
				plot = this,
				parentPlotItem = plotThread,
				plotItemIndex = conversationIndex,
				isSingleActivationAttemptItem = plotThread.isSingleActivationAttemptThread
			};
			charactersConversation.RuntimeData = runtimeData;
			return charactersConversation;
		}

		// Token: 0x06001A15 RID: 6677 RVA: 0x000518F0 File Offset: 0x0004FAF0
		public ICharactersConversation GetConversation(CharacterPlotContext context, ConversationBranch conversationBranch)
		{
			CharacterPlotNodeGraph.<>c__DisplayClass8_0 CS$<>8__locals1 = new CharacterPlotNodeGraph.<>c__DisplayClass8_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.context = context;
			CharacterPlotThread characterPlotThread;
			CharacterPlotThread characterPlotThread2;
			this.GetAdditionalConversationsPools(CS$<>8__locals1.context, conversationBranch, out characterPlotThread, out characterPlotThread2);
			int num;
			bool flag;
			CharacterPlotThreadNode mainConversationsNode = this.GetMainConversationsNode(CS$<>8__locals1.context, conversationBranch, out num, out flag);
			CharacterPlotThread characterPlotThread3 = (mainConversationsNode != null) ? mainConversationsNode.nodeAsset.thread : null;
			if (!flag && characterPlotThread != null && characterPlotThread.Items.Count != 0)
			{
				CharactersConversation charactersConversation = characterPlotThread.Items[0];
				if (this.IsActivePlotItem(CS$<>8__locals1.context, charactersConversation))
				{
					CharacterPlotTreeItemRuntimeData characterPlotTreeItemRuntimeData;
					charactersConversation = this.CreateConversation(characterPlotThread, charactersConversation, out characterPlotTreeItemRuntimeData, -1);
					characterPlotTreeItemRuntimeData.isFinalPlotItem = true;
					characterPlotTreeItemRuntimeData.isExpositionConversation = true;
					charactersConversation.conversationBranch = conversationBranch;
					return charactersConversation;
				}
			}
			if (characterPlotThread3 != null)
			{
				int count = characterPlotThread3.Items.Count;
				if (count != 0)
				{
					CharactersConversation charactersConversation2 = characterPlotThread3.Items[num];
					if (this.IsActivePlotItem(CS$<>8__locals1.context, charactersConversation2))
					{
						CharacterPlotTreeItemRuntimeData characterPlotTreeItemRuntimeData2;
						charactersConversation2 = this.CreateConversation(characterPlotThread3, charactersConversation2, out characterPlotTreeItemRuntimeData2, num);
						characterPlotTreeItemRuntimeData2.isFinalPlotItem = (num == count - 1);
						charactersConversation2.conversationBranch = conversationBranch;
						return charactersConversation2;
					}
				}
			}
			CharactersConversation charactersConversation3;
			if (characterPlotThread2 != null && characterPlotThread2.TryGetItem(new CharacterPlotItemsPool<CharactersConversation>.AvailabilityPredicate(CS$<>8__locals1.<GetConversation>g__IsAvailableFiller|0), out charactersConversation3, 0, true))
			{
				CharacterPlotTreeItemRuntimeData characterPlotTreeItemRuntimeData3;
				charactersConversation3 = this.CreateConversation(characterPlotThread2, charactersConversation3, out characterPlotTreeItemRuntimeData3, -1);
				characterPlotTreeItemRuntimeData3.isFillingConversation = true;
				charactersConversation3.conversationBranch = conversationBranch;
				return charactersConversation3;
			}
			return null;
		}

		// Token: 0x06001A16 RID: 6678 RVA: 0x00051A3C File Offset: 0x0004FC3C
		public void CompleteConversation(ICharactersConversation conversation, ICharacterPlotProgress characterPlotProgress)
		{
			if (characterPlotProgress != null)
			{
				CharacterPlotTreeProgress characterPlotTreeProgress = (CharacterPlotTreeProgress)characterPlotProgress;
				if (conversation.RuntimeData != null)
				{
					CharacterPlotTreeItemRuntimeData characterPlotTreeItemRuntimeData = (CharacterPlotTreeItemRuntimeData)conversation.RuntimeData;
					if (characterPlotTreeItemRuntimeData.isExpositionConversation)
					{
						characterPlotTreeProgress.AddCompletedExpositionThread(characterPlotTreeItemRuntimeData.parentPlotItem);
						return;
					}
					if (!characterPlotTreeItemRuntimeData.isFillingConversation)
					{
						characterPlotTreeProgress.Update(characterPlotTreeItemRuntimeData.parentPlotItem, characterPlotTreeItemRuntimeData.isFinalPlotItem ? -1 : (characterPlotTreeItemRuntimeData.plotItemIndex + 1));
					}
				}
			}
		}

		// Token: 0x06001A17 RID: 6679 RVA: 0x00051AA4 File Offset: 0x0004FCA4
		public void ConvertNode<T>(CharacterPlotNodeBase originalNode) where T : CharacterPlotNodeBase
		{
			if (originalNode is T)
			{
				return;
			}
			T t = (T)((object)Activator.CreateInstance(typeof(T)));
			t.OnNodeCreated();
			base.AddNode(t);
			t.CreateNodeAsset();
			t.nodeAsset.thread = originalNode.nodeAsset.thread;
			foreach (NodePort outputPort in (from e in originalNode.inputPorts[0].GetEdges()
			select e.outputPort).ToArray<NodePort>())
			{
				base.Connect(t.inputPorts[0], outputPort, false);
			}
			foreach (NodePort inputPort in (from e in originalNode.outputPorts[0].GetEdges()
			select e.inputPort).ToArray<NodePort>())
			{
				base.Connect(inputPort, t.outputPorts[0], false);
			}
			t.position = originalNode.position;
			base.RemoveNode(originalNode);
		}

		// Token: 0x06001A18 RID: 6680 RVA: 0x00051BF5 File Offset: 0x0004FDF5
		public void SetNodePriority<T>(CharacterPlotNodeBase originalNode, bool isPrimary) where T : CharacterPlotNodeBase
		{
			if (isPrimary)
			{
				originalNode.nodeAsset.thread.conversationBranch = ConversationBranch.Primary;
				return;
			}
			originalNode.nodeAsset.thread.conversationBranch = ConversationBranch.Secondary;
		}

		// Token: 0x04000E81 RID: 3713
		public string characterID;
	}
}
