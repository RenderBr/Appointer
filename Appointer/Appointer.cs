using Appointer.Models;
using CSF.TShock;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Appointer
{
    [ApiVersion(2, 1)]
    public class Appointer : TerrariaPlugin
    {
        public static AppointerApi api;
        private Timer _updateTimer;
        private readonly TSCommandFramework _fx;
        #region Plugin Metadata
        public override string Author
            => "Average";

        public override string Description
            => "Automatic rank progression plugin intended to be used by TBC";

        public override string Name
            => "Appointer";

        public override Version Version
            => new Version(1, 2);
        #endregion
        public Appointer(Main game)
            : base(game)
        {
            _fx = new(new()
            {
                DefaultLogLevel = CSF.LogLevel.Warning,
            });
        }

        public override async void Initialize()
        {
            api = new AppointerApi();

            //reloading
            GeneralHooks.ReloadEvent += (x) =>
            {
                api.ReloadConfig();
                x.Player.SendSuccessMessage("Successfully reloaded Appointer!");
            };

            #region Timer initialization
            _updateTimer = new(1000)
            {
                AutoReset = true
            };
            _updateTimer.Elapsed += async (_, x)
                => await Update(x);
            _updateTimer.Start();
            #endregion

            await _fx.BuildModulesAsync(typeof(Appointer).Assembly);
        }

        private static async Task Update(ElapsedEventArgs _)
        {
            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr is null || !(plr.Active && plr.IsLoggedIn))
                    continue;
                if (plr.Account is null)
                    continue;

                if (api.AFKSystemEnabled() == true)
                {

                    AFKPlayer afkPlayer = api.RetrieveAFKPlayer(plr);
                    if (afkPlayer == null)
                    {
                        api.AddPlayerToAFK(plr);
                        continue;
                    }

                    if (afkPlayer.isAFK == true && afkPlayer.LastPosition != plr.LastNetPosition)
                    {
                        afkPlayer.isAFK = false;
                        afkPlayer.afkTicks = 0;
                        TSPlayer.All.SendInfoMessage($"{plr.Name} is no longer AFK!");
                    }

                    if (afkPlayer.LastPosition == plr.LastNetPosition)
                    {
                        afkPlayer.afkTicks++;
                        if (api.Settings.KickForAFK)
                        {
                            if (afkPlayer.isAFK == true && afkPlayer.afkTicks < api.Settings.KickThreshold)
                                continue;

                            if (afkPlayer.isAFK == true && afkPlayer.afkTicks >= api.Settings.KickThreshold)
                            {
                                plr.Kick("Kicked for being AFK for too long! (over 15 minutes)", false, false);
                                continue;
                            }
                        }

                        if (afkPlayer.afkTicks >= 120 && afkPlayer.isAFK == false)
                        {
                            afkPlayer.isAFK = true;
                            TSPlayer.All.SendInfoMessage($"{plr.Name} is now AFK!", Color.LightYellow);
                            continue;
                        }
                    }
                    else
                    {
                        afkPlayer.afkTicks = 0;
                        if (afkPlayer.isAFK == true)
                        {
                            afkPlayer.isAFK = false;
                            TSPlayer.All.SendInfoMessage($"{plr.Name} is no longer AFK!", Color.LightYellow);
                        }
                    }
                    afkPlayer.LastPosition = plr.LastNetPosition;
                }

                var entity = await api.RetrieveOrCreatePlaytime(plr);
                entity.Playtime++;

                if (await Extensions.NextRankCost(plr.Account) == Extensions.RankCodeFinal || await Extensions.NextRankCost(plr.Account) == Extensions.RankCodeUnknown)
                    continue;

                if (await Extensions.NextRankCost(plr.Account) < 0 && Extensions.NextGroup(plr.Account).Cost != -1)
                {
                    string newGroup = Extensions.NextGroup(plr.Account).Name;
                    plr.Group = TShock.Groups.GetGroupByName(newGroup);
                    TShock.UserAccounts.SetUserGroup(plr.Account, newGroup);
                    TSPlayer.All.SendMessage($"{plr.Name} has ranked up to {Extensions.TrimPrefixStyling(plr.Group.Prefix)}! Congratulations :D", Color.LightGreen);
                }


            }
        }

    }
}
