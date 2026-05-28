using System;
using System.Collections.Generic;
using Common.Factories;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003E8 RID: 1000
	public abstract class AbilitySpecialBehaviourFactoryBase<TPrototype, TBehaviour> : PrototypeBasedFactory<TPrototype, TBehaviour> where TPrototype : class, IUnityObjectDescription where TBehaviour : ScriptableObject
	{
		// Token: 0x060021D1 RID: 8657
		protected abstract bool TryGetSpecialBehaviourItemInstanceID(IAbilityExtension specialBehaviourItem, out int instanceID);

		// Token: 0x060021D2 RID: 8658 RVA: 0x00069830 File Offset: 0x00067A30
		protected int GetSpecialBehaviourItemID(BaseAbility ability)
		{
			IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
			for (int i = 0; i < extensions.Count; i++)
			{
				int key;
				TPrototype tprototype;
				if (this.TryGetSpecialBehaviourItemInstanceID(extensions[i], out key) && this.prototypesDataByInstanceID.TryGetValue(key, out tprototype))
				{
					return tprototype.ObjectID;
				}
			}
			return 0;
		}

		// Token: 0x060021D3 RID: 8659 RVA: 0x00069883 File Offset: 0x00067A83
		protected override void OnPrototypeDataAdded(TPrototype prototypeData)
		{
			if (prototypeData.UnityObjectPrototype == null)
			{
				return;
			}
			this.prototypesDataByInstanceID.Add(prototypeData.UnityObjectPrototype.GetInstanceID(), prototypeData);
		}

		// Token: 0x0400152D RID: 5421
		private readonly Dictionary<int, TPrototype> prototypesDataByInstanceID = new Dictionary<int, TPrototype>(32);
	}
}
