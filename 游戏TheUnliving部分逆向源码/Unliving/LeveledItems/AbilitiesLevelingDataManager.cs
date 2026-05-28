using System;
using System.Collections.Generic;
using Common;
using Common.Editor.Reorderable;
using Game.Core;
using UnityEngine;

namespace Unliving.LeveledItems
{
	// Token: 0x0200024B RID: 587
	[CreateAssetMenu(fileName = "AbilitiesLevelingDataManager", menuName = "Abilities/Abilities Leveling Data Manager")]
	public sealed class AbilitiesLevelingDataManager : GlobalManagerBase, IAbilityPropertiesOverridesSource, IInitializable<IGame>, ISerializationCallbackReceiver
	{
		// Token: 0x060013BA RID: 5050 RVA: 0x0003E0D4 File Offset: 0x0003C2D4
		public static void ClearLoadedLevelingControllers()
		{
			Dictionary<string, AbilityLevelBasedPropertiesModifier> loadedLevelingControllers = AbilitiesLevelingDataManager.LoadedLevelingControllers;
			if (loadedLevelingControllers == null)
			{
				return;
			}
			loadedLevelingControllers.Clear();
		}

		// Token: 0x060013BB RID: 5051 RVA: 0x0003E0E8 File Offset: 0x0003C2E8
		private bool TryGetLevelingControllerData(IAbilityLevelingController levelingController, out AbilitiesLevelingDataManager.LevelingControllerData data)
		{
			AbilityLevelBasedPropertiesModifier abilityLevelBasedPropertiesModifier = levelingController as AbilityLevelBasedPropertiesModifier;
			if (abilityLevelBasedPropertiesModifier != null && this.levelingControllersCache.TryGetValue(abilityLevelBasedPropertiesModifier.GetInstanceID(), out data))
			{
				return true;
			}
			data = default(AbilitiesLevelingDataManager.LevelingControllerData);
			return false;
		}

		// Token: 0x060013BC RID: 5052 RVA: 0x0003E120 File Offset: 0x0003C320
		public AbilityPropertyValuesOverrides[] GetAbilityPropertiesOverrides(IAbilityLevelingController levelingController)
		{
			AbilitiesLevelingDataManager.LevelingControllerData levelingControllerData;
			if (!this.TryGetLevelingControllerData(levelingController, out levelingControllerData))
			{
				return null;
			}
			return levelingControllerData.propertiesOverrides;
		}

		// Token: 0x060013BD RID: 5053 RVA: 0x0003E140 File Offset: 0x0003C340
		public IReadOnlyList<AbilityPropertyValuesOverrides> GetAbilityPropertiesOverrides(string abilityName)
		{
			List<AbilityPropertyValuesOverrides> result;
			this.abilityPropertiesCache.TryGetValue(abilityName, out result);
			return result;
		}

		// Token: 0x060013BE RID: 5054 RVA: 0x0003E160 File Offset: 0x0003C360
		void IInitializable<IGame>.Initialize(IGame game)
		{
			if (this.cacheIsDirty || this.levelingControllersCache.Count == 0)
			{
				this.UpdateCache();
				this.UpdateData();
			}
			foreach (object obj in game.Services.GetAll())
			{
				IAbilityPropertiesOverridesHandler abilityPropertiesOverridesHandler = obj as IAbilityPropertiesOverridesHandler;
				if (abilityPropertiesOverridesHandler != null)
				{
					abilityPropertiesOverridesHandler.AbilityPropertiesOverridesSource = this;
				}
			}
		}

		// Token: 0x060013BF RID: 5055 RVA: 0x0003E1DC File Offset: 0x0003C3DC
		private void UpdateData()
		{
		}

		// Token: 0x060013C0 RID: 5056 RVA: 0x0003E1DE File Offset: 0x0003C3DE
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		// Token: 0x060013C1 RID: 5057 RVA: 0x0003E1E0 File Offset: 0x0003C3E0
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (GameApplication.IsMainThread)
			{
				this.UpdateCache();
				return;
			}
			this.cacheIsDirty = true;
		}

		// Token: 0x060013C2 RID: 5058 RVA: 0x0003E1F8 File Offset: 0x0003C3F8
		private void UpdateCache()
		{
			if (this.levelingControllersCache == null)
			{
				this.levelingControllersCache = new Dictionary<int, AbilitiesLevelingDataManager.LevelingControllerData>(this.data.list.Count);
			}
			if (this.abilityPropertiesCache == null)
			{
				this.abilityPropertiesCache = new Dictionary<string, List<AbilityPropertyValuesOverrides>>(this.data.list.Count, StringComparer.OrdinalIgnoreCase);
			}
			this.levelingControllersCache.Clear();
			this.abilityPropertiesCache.Clear();
			foreach (AbilitiesLevelingDataManager.LevelingControllerData levelingControllerData in this.data.list)
			{
				if (!(levelingControllerData.targetLevelingController == null))
				{
					this.levelingControllersCache.Add(levelingControllerData.targetLevelingController.GetInstanceID(), levelingControllerData);
					List<AbilityPropertyValuesOverrides> list;
					if (!this.abilityPropertiesCache.TryGetValue(levelingControllerData.mainAbilityAssetName, out list))
					{
						list = new List<AbilityPropertyValuesOverrides>(16);
						this.abilityPropertiesCache.Add(levelingControllerData.mainAbilityAssetName, list);
					}
					list.AddRange(levelingControllerData.propertiesOverrides);
				}
			}
			this.cacheIsDirty = false;
		}

		// Token: 0x04000B79 RID: 2937
		private const string LevelingControllerTag = "_levelingModifier";

		// Token: 0x04000B7A RID: 2938
		private static readonly string[] LevelingControllerSearchPaths = new string[]
		{
			"Assets/!Abilities",
			"Assets/!Characters"
		};

		// Token: 0x04000B7B RID: 2939
		private static readonly Dictionary<string, AbilityLevelBasedPropertiesModifier> LoadedLevelingControllers = new Dictionary<string, AbilityLevelBasedPropertiesModifier>(128, StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000B7C RID: 2940
		[SerializeField]
		private TextAsset externalTable;

		// Token: 0x04000B7D RID: 2941
		[SerializeField]
		[CustomReorderableList(null, "targetLevelingController", true, true)]
		private ReorderableListAdapter<List<AbilitiesLevelingDataManager.LevelingControllerData>> data;

		// Token: 0x04000B7E RID: 2942
		[SerializeField]
		[HideInInspector]
		private string lastExternalTableHash;

		// Token: 0x04000B7F RID: 2943
		private Dictionary<int, AbilitiesLevelingDataManager.LevelingControllerData> levelingControllersCache;

		// Token: 0x04000B80 RID: 2944
		private Dictionary<string, List<AbilityPropertyValuesOverrides>> abilityPropertiesCache;

		// Token: 0x04000B81 RID: 2945
		private bool cacheIsDirty;

		// Token: 0x020004D0 RID: 1232
		[Serializable]
		private struct LevelingControllerData
		{
			// Token: 0x06002551 RID: 9553 RVA: 0x00073AB4 File Offset: 0x00071CB4
			public override int GetHashCode()
			{
				int num = -1194179324;
				num = num * -1521134295 + this.mainAbilityAssetName.GetHashCode();
				if (this.targetLevelingController != null)
				{
					num = num * -1521134295 + this.targetLevelingController.GetInstanceID();
				}
				return num;
			}

			// Token: 0x040019D5 RID: 6613
			public string mainAbilityAssetName;

			// Token: 0x040019D6 RID: 6614
			public AbilityLevelBasedPropertiesModifier targetLevelingController;

			// Token: 0x040019D7 RID: 6615
			public AbilityPropertyValuesOverrides[] propertiesOverrides;
		}
	}
}
