using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Core;
using Unliving.Abilities;
using Unliving.Currencies;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x020001DB RID: 475
	public class AbilityActivatedContainer : IAbilityActivatedContainer
	{
		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06000F5E RID: 3934 RVA: 0x000309FF File Offset: 0x0002EBFF
		public AbilityFactoryPrototype AbilityPrototype
		{
			get
			{
				return this.lastAbilityPrototype;
			}
		}

		// Token: 0x140000A3 RID: 163
		// (add) Token: 0x06000F5F RID: 3935 RVA: 0x00030A08 File Offset: 0x0002EC08
		// (remove) Token: 0x06000F60 RID: 3936 RVA: 0x00030A40 File Offset: 0x0002EC40
		public event Action<BaseAbility> AbilityAdded;

		// Token: 0x06000F61 RID: 3937 RVA: 0x00030A75 File Offset: 0x0002EC75
		public AbilityActivatedContainer(IGame game)
		{
			this.currentGame = game;
			this.currentGame.Services.TryGet<IAbilityActivatedContainersFactory>(out this.factory);
		}

		// Token: 0x06000F62 RID: 3938 RVA: 0x00030AB1 File Offset: 0x0002ECB1
		public AbilityActivatedContainer(IGame game, IAbilityActivatedContainerData data) : this(game)
		{
			this.SetAbilitiesData(data);
		}

		// Token: 0x06000F63 RID: 3939 RVA: 0x00030AC1 File Offset: 0x0002ECC1
		public IAbilityActivatedContainerData GetAbilitiesContainerData()
		{
			return this.containerData;
		}

		// Token: 0x06000F64 RID: 3940 RVA: 0x00030AC9 File Offset: 0x0002ECC9
		public void SetDestructionRewardArgs(ICurrencyOperationArgs args)
		{
			this.containerData.destructionRewardArgs = (CurrencyOperationArgs)args;
		}

		// Token: 0x06000F65 RID: 3941 RVA: 0x00030ADC File Offset: 0x0002ECDC
		public void AddAbility(AbilityInfo abilityInfo)
		{
			AbilityFactoryArgs query = new AbilityFactoryArgs
			{
				abilityID = abilityInfo.abilityID,
				abilityLevel = abilityInfo.abilityLevel
			};
			this.lastAbilityPrototype = this.factory.GetAbilityPrototypeData((int)abilityInfo.abilityID);
			BaseAbility baseAbility = (BaseAbility)this.factory.Create(query);
			if (baseAbility != null)
			{
				this.currentAbilities.Add(baseAbility);
				this.containerData.Add(abilityInfo);
				Action<BaseAbility> abilityAdded = this.AbilityAdded;
				if (abilityAdded == null)
				{
					return;
				}
				abilityAdded(baseAbility);
			}
		}

		// Token: 0x06000F66 RID: 3942 RVA: 0x00030B64 File Offset: 0x0002ED64
		public void SetAbilitiesData(IAbilityActivatedContainerData abilityActivatedContainerData)
		{
			AbilityActivatedContainer.Data data = abilityActivatedContainerData as AbilityActivatedContainer.Data;
			if (data != null)
			{
				this.containerData.destructionRewardArgs = data.destructionRewardArgs;
				for (int i = 0; i < data.abilities.Count; i++)
				{
					AbilityInfo abilityInfo = data.abilities[i];
					if (abilityInfo.abilityID != AbilityID.None)
					{
						this.AddAbility(abilityInfo);
					}
				}
			}
		}

		// Token: 0x06000F67 RID: 3943 RVA: 0x00030BC0 File Offset: 0x0002EDC0
		public void DestroyContainer(object damageSender)
		{
			IPlayerProvider playerProvider;
			if (this.currentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				PlayerBehaviour currentPlayer = playerProvider.CurrentPlayer;
				if (currentPlayer != null)
				{
					CurrencyOperationArgs destructionRewardArgs = this.containerData.destructionRewardArgs;
					destructionRewardArgs.sender = this;
					if (!object.Equals(currentPlayer, damageSender))
					{
						currentPlayer.PerformCurrencyOperation(destructionRewardArgs);
					}
					PlayerAbilitiesController playerAbilitiesController = currentPlayer.AbilitiesController as PlayerAbilitiesController;
					if (playerAbilitiesController != null)
					{
						BaseAbility.UsingArgs usingArgs = new BaseAbility.UsingArgs
						{
							targetPosition = currentPlayer.Position
						};
						for (int i = 0; i < this.currentAbilities.Count; i++)
						{
							playerAbilitiesController.UseExternalAbility(this.currentAbilities[i], usingArgs);
						}
					}
				}
			}
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x00030C73 File Offset: 0x0002EE73
		public IReadOnlyList<BaseAbility> GetAbilities()
		{
			return this.currentAbilities;
		}

		// Token: 0x04000907 RID: 2311
		private AbilityFactoryPrototype lastAbilityPrototype;

		// Token: 0x04000908 RID: 2312
		private readonly IGame currentGame;

		// Token: 0x04000909 RID: 2313
		private readonly List<BaseAbility> currentAbilities = new List<BaseAbility>();

		// Token: 0x0400090A RID: 2314
		private readonly IAbilityActivatedContainersFactory factory;

		// Token: 0x0400090B RID: 2315
		private readonly AbilityActivatedContainer.Data containerData = new AbilityActivatedContainer.Data();

		// Token: 0x0200049B RID: 1179
		[Serializable]
		public class Data : IAbilityActivatedContainerData
		{
			// Token: 0x0600245E RID: 9310 RVA: 0x00070B0A File Offset: 0x0006ED0A
			public void Add(AbilityInfo abilityInfo)
			{
				this.abilities.Add(abilityInfo);
			}

			// Token: 0x040018EC RID: 6380
			public CurrencyOperationArgs destructionRewardArgs;

			// Token: 0x040018ED RID: 6381
			public List<AbilityInfo> abilities = new List<AbilityInfo>();
		}
	}
}
