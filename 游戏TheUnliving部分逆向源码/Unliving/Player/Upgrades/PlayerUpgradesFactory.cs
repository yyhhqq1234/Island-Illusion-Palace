using System;
using System.Collections.Generic;
using Common.Factories;
using Common.ServiceRegistry;
using Unliving.Scripting;

namespace Unliving.Player.Upgrades
{
	// Token: 0x0200016E RID: 366
	[Service(typeof(PlayerUpgradesFactory), new Type[]
	{
		typeof(IPlayerUpgradesFactory)
	})]
	public sealed class PlayerUpgradesFactory : PrototypeBasedFactory<PlayerUpgradeData, IPlayerUpgrade>, IPlayerUpgradesFactory, IObjectFactory<IPlayerUpgrade>, IFactory<IBaseObjectDescription, IPlayerUpgrade>, IFactory
	{
		// Token: 0x06000A2A RID: 2602 RVA: 0x00022040 File Offset: 0x00020240
		private IPlayerUpgrade Create(IPlayerUpgrade upgradePrototype, ScriptVariablesOverrides propertiesOverrides, int level)
		{
			IPlayerUpgrade playerUpgrade = upgradePrototype.Clone();
			playerUpgrade.ItemLevel = level;
			if (propertiesOverrides != null)
			{
				IScript script = playerUpgrade as IScript;
				if (script != null)
				{
					propertiesOverrides.ApplyOverrides(script);
				}
			}
			return playerUpgrade;
		}

		// Token: 0x06000A2B RID: 2603 RVA: 0x00022070 File Offset: 0x00020270
		protected override IPlayerUpgrade Create(PlayerUpgradeData data, IBaseObjectDescription args)
		{
			if (((data != null) ? data.upgradePrototype : null) == null)
			{
				return null;
			}
			PlayerUpgradesFactoryArgs playerUpgradesFactoryArgs = (PlayerUpgradesFactoryArgs)args;
			int upgradeLevel = playerUpgradesFactoryArgs.UpgradeLevel;
			ScriptVariablesOverrides propertiesOverrides = playerUpgradesFactoryArgs.upgradePropertiesOverrides ?? data.GetPropertiesOverrides(upgradeLevel);
			return this.Create((IPlayerUpgrade)data.upgradePrototype, propertiesOverrides, upgradeLevel);
		}

		// Token: 0x06000A2C RID: 2604 RVA: 0x000220C4 File Offset: 0x000202C4
		protected override void OnPrototypeDataAdded(PlayerUpgradeData data)
		{
			if (data.upgradePrototype != null)
			{
				IPlayerUpgrade playerUpgrade = data.upgradePrototype as IPlayerUpgrade;
				if (playerUpgrade != null)
				{
					this.upgradesID.Add(playerUpgrade, data.upgradeID);
				}
			}
		}

		// Token: 0x06000A2D RID: 2605 RVA: 0x00022100 File Offset: 0x00020300
		public PlayerUpgradesFactory(PlayerUpgradesFactoryParams factoryParams)
		{
			this.factoryParams = factoryParams;
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x0002211C File Offset: 0x0002031C
		public PlayerUpgradeID GetPlayerUpgradeID(IPlayerUpgrade playerUpgrade)
		{
			PlayerUpgradeID result;
			if (!this.upgradesID.TryGetValue(playerUpgrade.UpgradePrototype ?? playerUpgrade, out result))
			{
				result = PlayerUpgradeID.None;
			}
			return result;
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x00022146 File Offset: 0x00020346
		public PlayerUpgradeData GetPlayerUpgradeData(PlayerUpgradeID upgradeID)
		{
			return base.GetObjectPrototype((int)upgradeID);
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x0002214F File Offset: 0x0002034F
		public IPlayerUpgrade Create(PlayerUpgradesFactoryArgs args)
		{
			if (args.upgradePrototype != null)
			{
				return this.Create(args.upgradePrototype, args.upgradePropertiesOverrides, args.UpgradeLevel);
			}
			return base.Create(args);
		}

		// Token: 0x06000A31 RID: 2609 RVA: 0x00022179 File Offset: 0x00020379
		public IPlayerUpgrade CreatePlayerFeaturesBlocker()
		{
			IPlayerUpgrade playerUpgrade = (IPlayerUpgrade)this.factoryParams.playerFeaturesBlockerAsset;
			if (playerUpgrade == null)
			{
				return null;
			}
			return playerUpgrade.Clone();
		}

		// Token: 0x040005FE RID: 1534
		private readonly PlayerUpgradesFactoryParams factoryParams;

		// Token: 0x040005FF RID: 1535
		private readonly Dictionary<IPlayerUpgrade, PlayerUpgradeID> upgradesID = new Dictionary<IPlayerUpgrade, PlayerUpgradeID>(32);
	}
}
