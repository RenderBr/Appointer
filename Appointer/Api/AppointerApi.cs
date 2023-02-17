using Appointer.Models;
using Auxiliary;
using Auxiliary.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TShockAPI;

namespace Appointer
{
    public class AppointerApi
    {
        public AppointerSettings Settings { get; private set; }

        public AppointerApi()
        {
            Configuration<AppointerSettings>.Load("Appointer");
            Settings = Configuration<AppointerSettings>.Settings;
        }

        public void ReloadConfig()
        {
            Configuration<AppointerSettings>.Load("Appointer");
        }

        public static List<AFKPlayer> afkPlayers = new List<AFKPlayer>();

        public async Task<ITBCUser> RetrieveOrCreatePlaytime(string player)
        {
            if (LinkedModeEnabled() == true)
                return await IModel.GetAsync(GetRequest.Linked<LinkedTBCUser>(x => x.AccountName == player), x => x.AccountName = player);

           return await IModel.GetAsync(GetRequest.Bson<TBCUser>(x => x.AccountName == player), x => x.AccountName = player);

        }

    public async Task<ITBCUser> RetrieveOrCreatePlaytime(TSPlayer player)
            => await RetrieveOrCreatePlaytime(player.Account.Name);

        public async Task<ITBCUser> RetrievePlaytime(string player)
            => await IModel.GetAsync(GetRequest.Bson<TBCUser>(x => x.AccountName == player), x => x.AccountName = player);

        public async Task<ITBCUser> RetrievePlaytime(TSPlayer player)
            => await RetrievePlaytime(player.Account.Name);

        public bool AFKSystemEnabled()
            => Settings.UseAFKSystem;
        public bool LinkedModeEnabled()
            => Settings.EnableLinkedMode;

        internal void AddPlayerToAFK(TSPlayer player)
        {
            afkPlayers.Add(new AFKPlayer(player.Name, player.LastNetPosition));
        }

        internal AFKPlayer RetrieveAFKPlayer(TSPlayer player)
        {
            var any = afkPlayers.Any(x => x.PlayerName == player.Name);
            if (any == false)
                return null;

            return afkPlayers.First(x => x.PlayerName == player.Name);
        }


        public bool IsPlayerAFK(TSPlayer player)
        {
            var afkPlayer = RetrieveAFKPlayer(player);

            if (afkPlayer == null)
                return false;

            if (afkPlayer.isAFK == true)
                return true;

            return false;
        }
    }
}
