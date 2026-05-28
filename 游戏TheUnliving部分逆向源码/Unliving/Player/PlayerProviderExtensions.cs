using System;
using System.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Unliving.Player
{
	// Token: 0x02000161 RID: 353
	public static class PlayerProviderExtensions
	{
		// Token: 0x060009E1 RID: 2529 RVA: 0x00021C54 File Offset: 0x0001FE54
		public static bool TryGetPlayer(this IGame game, out PlayerBehaviour player)
		{
			IPlayerProvider playerProvider;
			if (game.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				player = playerProvider.CurrentPlayer;
				return player != null;
			}
			player = null;
			return false;
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x00021C88 File Offset: 0x0001FE88
		public static async Task<PlayerBehaviour> GetPlayerAsync(this IPlayerProvider playerProvider)
		{
			await new WaitForEndOfFrame();
			return (playerProvider != null) ? playerProvider.CurrentPlayer : null;
		}

		// Token: 0x060009E3 RID: 2531 RVA: 0x00021CD0 File Offset: 0x0001FED0
		public static async Task<PlayerBehaviour> GetPlayerAsync(this IGame game)
		{
			await new WaitForEndOfFrame();
			PlayerBehaviour result;
			if (game == null)
			{
				result = null;
			}
			else
			{
				IPlayerProvider playerProvider = game.Services.Get<IPlayerProvider>();
				result = ((playerProvider != null) ? playerProvider.CurrentPlayer : null);
			}
			return result;
		}
	}
}
