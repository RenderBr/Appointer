using Appointer.Models;
using Auxiliary.Configuration;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

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
        /// <param name="accName">The name of the user's account.</param>
        /// <returns>An instance of <see cref="UserPlaytime"/> representing the playtime information.</returns>
        public UserPlaytime RetrievePlaytime(string accName)
        {
            string sql = $"SELECT * FROM UserPlaytime WHERE AccountName = @AccountName";
            var playtime = Appointer.DB.FirstOrDefault<UserPlaytime>(sql, new { AccountName = accName });

            if (playtime == null)
            {
                UserPlaytime _playtime = new()
                {
                    AccountName = accName,
                    Playtime = 0
                };
                var createdID = Appointer.DB.Insert(_playtime);
                return Appointer.DB.Single<UserPlaytime>(createdID);
            }
            return playtime;

        }

        /// <summary>
        /// Retrieves or creates the playtime for the specified player, they must have an account.
        /// </summary>
        /// <param name="account">The <see cref="UserAccount"/> object representing the user's account.</param>
        /// <returns>An instance of <see cref="UserPlaytime"/> representing the playtime information.</returns>
        public UserPlaytime RetrievePlaytime(UserAccount account)
            => RetrievePlaytime(account.Name);

        /// <summary>
        /// If the AFK system is enabled, this returns true.
        /// </summary>
        public bool AFKSystemEnabled => Settings.UseAFKSystem;

        /// <summary>
        /// If the user has decided to use MySQL, this returns true.
        /// </summary>
        public bool IsUsingMySQL => Settings.UseMySQL;

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
