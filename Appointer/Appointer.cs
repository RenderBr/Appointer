using Appointer.Database;
using Appointer.Models;
using CSF.TShock;
using Microsoft.Xna.Framework;
using PetaPoco;
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
        private readonly TSCommandFramework _fx;

        public static AppointerDB _database = new();
        public static IDatabase DB = _database.DB;

        #region Plugin Metadata
        public override string Author => "Average";
        public override string Description => "Automatic rank progression plugin, along with an AFK system";
        public override string Name => "Appointer";
        public override Version Version => new(1, 3);
        #endregion

        private Timer _updateTimer;

        public Appointer(Main game) : base(game)
        {
            _fx = new(new()
            {
                DefaultLogLevel = CSF.LogLevel.Warning,
            });
        }

        public override async void Initialize()
        {
            api = new AppointerApi();
            _database.InitializeDB(api.Settings.UseMySQL);

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
            _updateTimer.Elapsed += Update;
            _updateTimer.Start();
            #endregion

            await _fx.BuildModulesAsync(typeof(Appointer).Assembly);
        }

        private void Update(object _, ElapsedEventArgs __)
        {
            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr is null || !(plr.Active && plr.IsLoggedIn) || plr.Account is null)
                    continue;

                if (api.AFKSystemEnabled)
                {
                    AFKPlayer afkPlayer = api.RetrieveAFKPlayer(plr);
                    if (afkPlayer == null)
                    {
                        api.AddPlayerToAFK(plr);
                        continue;
                    }

                    if (afkPlayer.isAFK && afkPlayer.LastPosition != plr.LastNetPosition)
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
                            if (afkPlayer.isAFK && afkPlayer.afkTicks < api.Settings.KickThreshold)
                                continue;

                            if (afkPlayer.isAFK && afkPlayer.afkTicks >= api.Settings.KickThreshold)
                            {
                                plr.Kick("Kicked for being AFK for too long! (over 15 minutes)", false, false);
                                continue;
                            }
                        }

                        if (afkPlayer.afkTicks >= 120 && !afkPlayer.isAFK)
                        {
                            afkPlayer.isAFK = true;
                            TSPlayer.All.SendInfoMessage($"{plr.Name} is now AFK!", Color.LightYellow);
                            continue;
                        }
                    }
                    else
                    {
                        afkPlayer.afkTicks = 0;
                        if (afkPlayer.isAFK)
                        {
                            afkPlayer.isAFK = false;
                            TSPlayer.All.SendInfoMessage($"{plr.Name} is no longer AFK!", Color.LightYellow);
                        }
                    }
                    afkPlayer.LastPosition = plr.LastNetPosition;
                }

                var entity = api.RetrievePlaytime(plr);
                entity.Playtime++;

                var nextRankCost = Extensions.NextRankCost(plr.Account);

                if (nextRankCost == Extensions.RankCodeFinal || nextRankCost == Extensions.RankCodeUnknown)
                    continue;

                if (nextRankCost < 0 && Extensions.NextGroup(plr.Account).Cost != -1)
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
