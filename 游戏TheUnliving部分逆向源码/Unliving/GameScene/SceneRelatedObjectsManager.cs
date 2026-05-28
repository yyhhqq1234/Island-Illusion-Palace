using System;
using System.Runtime.CompilerServices;
using Common.Editor;
using Common.Shaders;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.LevelGeneration;

namespace Unliving.GameScene
{
	// Token: 0x02000261 RID: 609
	[CreateAssetMenu(fileName = "SceneRelatedObjectsManager", menuName = "Game/Global/Scene Related Objects Manager")]
	public class SceneRelatedObjectsManager : GlobalManagerBase
	{
		// Token: 0x06001436 RID: 5174 RVA: 0x0003F9D4 File Offset: 0x0003DBD4
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			base.CurrentGame.Services.TryGet<GameManager>(out this.gameManager);
		}

		// Token: 0x06001437 RID: 5175 RVA: 0x0003F9F4 File Offset: 0x0003DBF4
		protected override void OnSceneLoaded(Scene scene)
		{
			if (!this.isActive)
			{
				this.defaultData.ApplyShaderPropertyMultipliers();
				return;
			}
			base.OnSceneLoaded(scene);
			this.ApplyLocationTypeSettings();
		}

		// Token: 0x06001438 RID: 5176 RVA: 0x0003FA18 File Offset: 0x0003DC18
		private void ApplyLocationTypeSettings()
		{
			IGameLocationProvider gameLocationProvider;
			if (base.CurrentGame.Services.TryGet<IGameLocationProvider>(out gameLocationProvider))
			{
				GameSceneManager gameSceneManager = gameLocationProvider as GameSceneManager;
				if (gameSceneManager != null)
				{
					gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.<ApplyLocationTypeSettings>g__ApplySettingsInternal|9_0));
					return;
				}
				this.<ApplyLocationTypeSettings>g__ApplySettingsInternal|9_0(gameLocationProvider);
			}
		}

		// Token: 0x06001439 RID: 5177 RVA: 0x0003FA60 File Offset: 0x0003DC60
		internal void ApplyShaderPropertyMultipliers(GameLocation.TypeID locationType)
		{
			for (int i = 0; i < this.locationsData.Length; i++)
			{
				if (this.locationsData[i].locationType == locationType)
				{
					this.locationsData[i].ApplyShaderPropertyMultipliers();
					return;
				}
			}
			this.defaultData.ApplyShaderPropertyMultipliers();
		}

		// Token: 0x0600143A RID: 5178 RVA: 0x0003FAB4 File Offset: 0x0003DCB4
		private void SpawnSceneObjects(GameLocation.TypeID locationType)
		{
			foreach (SceneRelatedObjectsManager.SceneObjectReference sceneObjectReference in this.defaultData.objectReferences)
			{
				bool flag = false;
				int j = 0;
				while (j < this.locationsData.Length)
				{
					SceneRelatedObjectsManager.SceneObjectsData sceneObjectsData = this.locationsData[j];
					SceneRelatedObjectsManager.SceneObjectReference sceneObjectReference2;
					if (sceneObjectsData.locationType == locationType && sceneObjectsData.TryGetObjectOfType(sceneObjectReference.objectType, out sceneObjectReference2))
					{
						flag = true;
						if (!sceneObjectReference2.prefab.IsNull())
						{
							UnityEngine.Object.Instantiate<GameObject>(sceneObjectReference2.prefab);
							break;
						}
						break;
					}
					else
					{
						j++;
					}
				}
				if (!flag && !sceneObjectReference.prefab.IsNull())
				{
					UnityEngine.Object.Instantiate<GameObject>(sceneObjectReference.prefab);
				}
			}
		}

		// Token: 0x0600143C RID: 5180 RVA: 0x0003FB6F File Offset: 0x0003DD6F
		[CompilerGenerated]
		private void <ApplyLocationTypeSettings>g__ApplySettingsInternal|9_0(IGameLocationProvider gameLocationProvider)
		{
			this.SpawnSceneObjects(gameLocationProvider.LocationType);
			this.ApplyShaderPropertyMultipliers(gameLocationProvider.LocationType);
		}

		// Token: 0x04000BC8 RID: 3016
		public bool isActive;

		// Token: 0x04000BC9 RID: 3017
		public SceneRelatedObjectsManager.SceneObjectsData defaultData;

		// Token: 0x04000BCA RID: 3018
		public SceneRelatedObjectsManager.SceneObjectsData[] locationsData;

		// Token: 0x04000BCB RID: 3019
		private GameManager gameManager;

		// Token: 0x020004DD RID: 1245
		public enum SceneObjectType
		{
			// Token: 0x04001A0A RID: 6666
			GameCamera,
			// Token: 0x04001A0B RID: 6667
			GameSessionManager,
			// Token: 0x04001A0C RID: 6668
			InputManager,
			// Token: 0x04001A0D RID: 6669
			AudioManager,
			// Token: 0x04001A0E RID: 6670
			ShadowManager,
			// Token: 0x04001A0F RID: 6671
			IngameUIManager,
			// Token: 0x04001A10 RID: 6672
			GlobalLight,
			// Token: 0x04001A11 RID: 6673
			FogController,
			// Token: 0x04001A12 RID: 6674
			ParticleSystemsManager,
			// Token: 0x04001A13 RID: 6675
			ParallaxRenderer
		}

		// Token: 0x020004DE RID: 1246
		[Serializable]
		public struct SceneObjectReference
		{
			// Token: 0x04001A14 RID: 6676
			public SceneRelatedObjectsManager.SceneObjectType objectType;

			// Token: 0x04001A15 RID: 6677
			public GameObject prefab;
		}

		// Token: 0x020004DF RID: 1247
		[Serializable]
		public struct SceneObjectsData
		{
			// Token: 0x0600257D RID: 9597 RVA: 0x00074754 File Offset: 0x00072954
			public void ApplyShaderPropertyMultipliers()
			{
				for (int i = 0; i < this.shaderPropertyMultipliers.Length; i++)
				{
					this.shaderPropertyMultipliers[i].Apply();
				}
			}

			// Token: 0x0600257E RID: 9598 RVA: 0x00074784 File Offset: 0x00072984
			public bool TryGetObjectOfType(SceneRelatedObjectsManager.SceneObjectType objectType, out SceneRelatedObjectsManager.SceneObjectReference objectReference)
			{
				objectReference = default(SceneRelatedObjectsManager.SceneObjectReference);
				for (int i = 0; i < this.objectReferences.Length; i++)
				{
					SceneRelatedObjectsManager.SceneObjectReference sceneObjectReference = this.objectReferences[i];
					if (sceneObjectReference.objectType == objectType)
					{
						objectReference = sceneObjectReference;
						return true;
					}
				}
				return false;
			}

			// Token: 0x04001A16 RID: 6678
			public GameLocation.TypeID locationType;

			// Token: 0x04001A17 RID: 6679
			public SceneRelatedObjectsManager.SceneObjectReference[] objectReferences;

			// Token: 0x04001A18 RID: 6680
			[SerializeReference]
			[ManagedObjectField(typeof(IShaderPropertyMultiplier))]
			public IShaderPropertyMultiplier[] shaderPropertyMultipliers;
		}
	}
}
