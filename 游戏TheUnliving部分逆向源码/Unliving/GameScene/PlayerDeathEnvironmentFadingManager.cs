using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.GameScene
{
	// Token: 0x02000260 RID: 608
	[CreateAssetMenu(fileName = "PlayerDeathEnvironmentFadingManager", menuName = "Game/Player Death Environment Fading Manager")]
	public sealed class PlayerDeathEnvironmentFadingManager : GlobalManagerBase
	{
		// Token: 0x0600142C RID: 5164 RVA: 0x0003F6D8 File Offset: 0x0003D8D8
		private static List<BaseGameMob> CollectAffectableMobs(ILocationChunk playerChunk)
		{
			List<BaseGameMob> list = new List<BaseGameMob>(64);
			if (playerChunk != null)
			{
				IList<ILocationChunkGateway> gateways = playerChunk.Gateways;
				PlayerDeathEnvironmentFadingManager.<CollectAffectableMobs>g__CollectAffectableMobs|2_0(playerChunk.Visitors, list);
				PlayerDeathEnvironmentFadingManager.<CollectAffectableMobs>g__CollectAffectableMobs|2_0(playerChunk.EnvironmentObjects, list);
				for (int i = 0; i < gateways.Count; i++)
				{
					ILocationChunk nextChunk = gateways[i].GetNextChunk();
					if (nextChunk != null && nextChunk.IsVisible)
					{
						PlayerDeathEnvironmentFadingManager.<CollectAffectableMobs>g__CollectAffectableMobs|2_0(nextChunk.Visitors, list);
						PlayerDeathEnvironmentFadingManager.<CollectAffectableMobs>g__CollectAffectableMobs|2_0(nextChunk.EnvironmentObjects, list);
					}
				}
			}
			return list;
		}

		// Token: 0x140000C7 RID: 199
		// (add) Token: 0x0600142D RID: 5165 RVA: 0x0003F754 File Offset: 0x0003D954
		// (remove) Token: 0x0600142E RID: 5166 RVA: 0x0003F78C File Offset: 0x0003D98C
		public event Action PlayerDeathEffectStarted;

		// Token: 0x0600142F RID: 5167 RVA: 0x0003F7C4 File Offset: 0x0003D9C4
		private async void FadeSceneLight(Light2D targetLight)
		{
			float fadeSpeed = targetLight.intensity / this.mainSceneLightFadeTime;
			while (!GameApplication.IsGameStateChanging && targetLight.intensity > 0f)
			{
				await Task.Yield();
				targetLight.intensity -= fadeSpeed * Time.unscaledDeltaTime;
			}
			if (targetLight != null)
			{
				targetLight.intensity = 0f;
			}
		}

		// Token: 0x06001430 RID: 5168 RVA: 0x0003F808 File Offset: 0x0003DA08
		private void FadeMob(Renderer mobRenderer, PlayerDeathEnvironmentFadingManager.MobFadeData fadeData)
		{
			PlayerDeathEnvironmentFadingManager.<FadeMob>d__12 <FadeMob>d__;
			<FadeMob>d__.mobRenderer = mobRenderer;
			<FadeMob>d__.fadeData = fadeData;
			<FadeMob>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<FadeMob>d__.<>1__state = -1;
			AsyncVoidMethodBuilder <>t__builder = <FadeMob>d__.<>t__builder;
			<>t__builder.Start<PlayerDeathEnvironmentFadingManager.<FadeMob>d__12>(ref <FadeMob>d__);
		}

		// Token: 0x06001431 RID: 5169 RVA: 0x0003F84C File Offset: 0x0003DA4C
		protected override async void OnSceneLoaded(Scene loadedScene)
		{
			if (this.isActive)
			{
				Shader.SetGlobalFloat(PlayerDeathEnvironmentFadingManager.MinLightIntensityOverridePropertyId, float.MaxValue);
				IGameSessionManager playerProvider;
				if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out playerProvider))
				{
					PlayerBehaviour playerBehaviour = await playerProvider.GetPlayerAsync();
					if (playerBehaviour != null)
					{
						playerBehaviour.Killed += this.OnPlayerKilled;
					}
				}
			}
		}

		// Token: 0x06001432 RID: 5170 RVA: 0x0003F888 File Offset: 0x0003DA88
		private void OnPlayerKilled(IGameMob player)
		{
			player.Killed -= this.OnPlayerKilled;
			List<BaseGameMob> list = PlayerDeathEnvironmentFadingManager.CollectAffectableMobs(((ILocationObject)player).CurrentLocationChunk);
			Shader.SetGlobalFloat(PlayerDeathEnvironmentFadingManager.MinLightIntensityOverridePropertyId, this.minLightIntensityOverride);
			if (MainSceneLight.Instance != null)
			{
				this.FadeSceneLight(MainSceneLight.Instance);
			}
			for (int i = 0; i < list.Count; i++)
			{
				BaseGameMob baseGameMob = list[i];
				for (int j = 0; j < this.mobsFadeData.Length; j++)
				{
					ref PlayerDeathEnvironmentFadingManager.MobFadeData ptr = ref this.mobsFadeData[j];
					if (ptr.CanBeFaded(baseGameMob))
					{
						this.FadeMob(baseGameMob.Renderer, ptr);
						break;
					}
				}
			}
			Action playerDeathEffectStarted = this.PlayerDeathEffectStarted;
			if (playerDeathEffectStarted == null)
			{
				return;
			}
			playerDeathEffectStarted();
		}

		// Token: 0x06001435 RID: 5173 RVA: 0x0003F97C File Offset: 0x0003DB7C
		[CompilerGenerated]
		internal static void <CollectAffectableMobs>g__CollectAffectableMobs|2_0(IEnumerable<ILocationObject> chunkObjects, List<BaseGameMob> mobsList)
		{
			IReadOnlyList<ILocationObject> readOnlyList = (IReadOnlyList<ILocationObject>)chunkObjects;
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				BaseGameMob baseGameMob = readOnlyList[i].CastOrGetComponent<BaseGameMob>();
				if (!(baseGameMob == null) && !(baseGameMob.Renderer == null) && baseGameMob.IsRendererVisible)
				{
					mobsList.Add(baseGameMob);
				}
			}
		}

		// Token: 0x04000BC1 RID: 3009
		private static readonly int MinLightIntensityOverridePropertyId = Shader.PropertyToID("_MinLightIntensityOverride");

		// Token: 0x04000BC3 RID: 3011
		public bool isActive = true;

		// Token: 0x04000BC4 RID: 3012
		public float mainSceneLightFadeTime = 1f;

		// Token: 0x04000BC5 RID: 3013
		[Tooltip("Minimum light intensity value for lit shader after player death")]
		public float minLightIntensityOverride;

		// Token: 0x04000BC6 RID: 3014
		public bool hideWorldCanvas = true;

		// Token: 0x04000BC7 RID: 3015
		public PlayerDeathEnvironmentFadingManager.MobFadeData[] mobsFadeData;

		// Token: 0x020004D9 RID: 1241
		[Serializable]
		public struct MobFadeData
		{
			// Token: 0x06002575 RID: 9589 RVA: 0x00074264 File Offset: 0x00072464
			public int GetShaderPropertyID()
			{
				int num = this.shaderPropertyID.GetValueOrDefault();
				if (this.shaderPropertyID == null)
				{
					num = Shader.PropertyToID(this.shaderPropertyName);
					this.shaderPropertyID = new int?(num);
					return num;
				}
				return num;
			}

			// Token: 0x06002576 RID: 9590 RVA: 0x000742A5 File Offset: 0x000724A5
			public bool CanBeFaded(IGameMob mob)
			{
				return this.fadeTime > 0f && !string.IsNullOrEmpty(this.shaderPropertyName) && mob.IsLayerInMask(this.affectableMobLayers);
			}

			// Token: 0x040019F0 RID: 6640
			public LayerMask affectableMobLayers;

			// Token: 0x040019F1 RID: 6641
			public string shaderPropertyName;

			// Token: 0x040019F2 RID: 6642
			public Color shaderPropertyValue;

			// Token: 0x040019F3 RID: 6643
			public float fadeTime;

			// Token: 0x040019F4 RID: 6644
			private int? shaderPropertyID;
		}
	}
}
