using Appointer.Models;
using Auxiliary;
using Auxiliary.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TShockAPI;

namespace Appointer
{
	/// <summary>
	/// Provides an API for interacting with the Appointer plugin.
	/// </summary>
	public class AppointerApi
	{
		public AppointerSettings Settings { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AppointerApi"/> class.
		/// </summary>
		public AppointerApi()
		{
			Configuration<AppointerSettings>.Load("Appointer");
			Settings = Configuration<AppointerSettings>.Settings;
		}

		/// <summary>
		/// Reloads the Appointer configuration.
		/// </summary>
		public void ReloadConfig() => Configuration<AppointerSettings>.Load("Appointer");

		public static List<AFKPlayer> afkPlayers = new();

		/// <summary>
		/// Retrieves or creates the playtime for the specified player.
		/// </summary>
		/// <param name="player">The name of the player.</param>
		/// <returns>An instance of <see cref="ITBCUser"/> representing the playtime information.</returns>
		public async Task<ITBCUser> RetrieveOrCreatePlaytime(string player)
		{
			if (LinkedModeEnabled() == true)
				return await IModel.GetAsync(GetRequest.Linked<LinkedTBCUser>(x => x.AccountName == player), x => x.AccountName = player);

			return await IModel.GetAsync(GetRequest.Bson<TBCUser>(x => x.AccountName == player), x => x.AccountName = player);
		}

		/// <summary>
		/// Retrieves or creates the playtime for the specified player.
		/// </summary>
		/// <param name="player">The <see cref="TSPlayer"/> object representing the player.</param>
		/// <returns>An instance of <see cref="ITBCUser"/> representing the playtime information.</returns>
		public async Task<ITBCUser> RetrieveOrCreatePlaytime(TSPlayer player)
			=> await RetrieveOrCreatePlaytime(player.Account.Name);

		/// <summary>
		/// Retrieves the playtime for the specified player.
		/// </summary>
		/// <param name="player">The name of the player.</param>
		/// <returns>An instance of <see cref="ITBCUser"/> representing the playtime information.</returns>
		public async Task<ITBCUser> RetrievePlaytime(string player)
		{
			if (LinkedModeEnabled() == true)
				return await IModel.GetAsync(GetRequest.Linked<LinkedTBCUser>(x => x.AccountName == player));

			return await IModel.GetAsync(GetRequest.Bson<TBCUser>(x => x.AccountName == player));
		}

		/// <summary>
		/// Retrieves the playtime for the specified player.
		/// </summary>
		/// <param name="player">The <see cref="TSPlayer"/> object representing the player.</param>
		/// <returns>An instance of <see cref="ITBCUser"/> representing the playtime information.</returns>
		public async Task<ITBCUser> RetrievePlaytime(TSPlayer player)
			=> await RetrievePlaytime(player.Account.Name);

		/// <summary>
		/// Determines whether the AFK system is enabled.
		/// </summary>
		/// <returns><c>true</c> if the AFK system is enabled; otherwise, <c>false</c>.</returns>
		public bool AFKSystemEnabled() => Settings.UseAFKSystem;

		/// <summary>
		/// Determines whether linked mode is enabled.
		/// </summary>
		/// <returns><c>true</c> if linked mode is enabled; otherwise, <c>false</c>.</returns>
		public bool LinkedModeEnabled() => Settings.EnableLinkedMode;

		internal void AddPlayerToAFK(TSPlayer player) => afkPlayers.Add(new AFKPlayer(player.Name, player.LastNetPosition));

		internal AFKPlayer RetrieveAFKPlayer(TSPlayer player)
		{
			var any = afkPlayers.Any(x => x.PlayerName == player.Name);
			if (any == false)
				return null;

			return afkPlayers.First(x => x.PlayerName == player.Name);
		}

		/// <summary>
		/// Determines whether the player is AFK.
		/// </summary>
		/// <param name="player">The <see cref="TSPlayer"/> object representing the player.</param>
		/// <returns><c>true</c> if the player is AFK; otherwise, <c>false</c>.</returns>
		public bool IsPlayerAFK(TSPlayer player)
		{
			var afkPlayer = RetrieveAFKPlayer(player);
			return afkPlayer?.isAFK ?? false;
		}
	}
}
