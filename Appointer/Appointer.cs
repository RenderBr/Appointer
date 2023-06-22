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
		public static AppointerApi Api { get; private set; }
		private Timer _updateTimer;
		private readonly TSCommandFramework _fx;

		#region Plugin Metadata
		public override string Author => "Average";
		public override string Description => "Automatic rank progression plugin intended to be used by TBC";
		public override string Name => "Appointer";
		public override Version Version => new Version(1, 1, 1);
		#endregion

		public Appointer(Main game) : base(game)
		{
			_fx = new TSCommandFramework(new TSCommandFrameworkConfig
			{
				DefaultLogLevel = CSF.LogLevel.Warning
			});
		}

		public override async void Initialize()
		{
			Api = new AppointerApi();

			// Reloading
			GeneralHooks.ReloadEvent += x =>
			{
				Api.ReloadConfig();
				x.Player.SendSuccessMessage("Successfully reloaded Appointer!");
			};

			// Timer initialization
			_updateTimer = new Timer(1000)
			{
				AutoReset = true
			};
			_updateTimer.Elapsed += async (_, args) => await Update(args);
			_updateTimer.Start();

			await _fx.BuildModulesAsync(typeof(Appointer).Assembly);
		}

		private static async Task Update(ElapsedEventArgs args)
		{
			foreach (TSPlayer player in TShock.Players)
			{
				if (player is null || !(player.Active && player.IsLoggedIn) || player.Account is null)
					continue;

				if (Api.AFKSystemEnabled())
				{
					AFKPlayer afkPlayer = Api.RetrieveAFKPlayer(player);
					if (afkPlayer == null)
					{
						Api.AddPlayerToAFK(player);
						continue;
					}

					if (afkPlayer.IsAFK && afkPlayer.LastPosition != player.LastNetPosition)
					{
						afkPlayer.IsAFK = false;
						afkPlayer.AFKTicks = 0;
						TSPlayer.All.SendInfoMessage($"{player.Name} is no longer AFK!");
					}

					if (afkPlayer.LastPosition == player.LastNetPosition)
					{
						afkPlayer.AFKTicks++;
						if (Api.Settings.KickForAFK)
						{
							if (afkPlayer.IsAFK && afkPlayer.AFKTicks < Api.Settings.KickThreshold)
								continue;

							if (afkPlayer.IsAFK && afkPlayer.AFKTicks >= Api.Settings.KickThreshold)
							{
								player.Kick("Kicked for being AFK for too long! (over 15 minutes)", false, false);
								continue;
							}
						}

						if (afkPlayer.AFKTicks >= 120 && !afkPlayer.IsAFK)
						{
							afkPlayer.IsAFK = true;
							TSPlayer.All.SendInfoMessage($"{player.Name} is now AFK!", Color.LightYellow);
							continue;
						}
					}
					else
					{
						afkPlayer.AFKTicks = 0;
						if (afkPlayer.IsAFK)
						{
							afkPlayer.IsAFK = false;
							TSPlayer.All.SendInfoMessage($"{player.Name} is no longer AFK!", Color.LightYellow);
						}
					}
					afkPlayer.LastPosition = player.LastNetPosition;
				}

				var account = player.Account;
				var nextRankCost = await account.NextRankCost();
				if (nextRankCost == Extensions.RankCodeFinal || nextRankCost == Extensions.RankCodeUnknown)
					continue;

				if (nextRankCost < 0 && account.Group.Cost != -1)
				{
					var nextGroup = account.NextGroup();
					string newGroupName = nextGroup.Name;
					TShock.Group newGroup = TShock.Groups.GetGroupByName(newGroupName);
					player.Group = newGroup;
					TShock.UserAccounts.SetUserGroup(account, newGroupName);
					TSPlayer.All.SendMessage($"{player.Name} has ranked up to {Extensions.TrimPrefixStyling(newGroup.Prefix)}! Congratulations :D", Color.LightGreen);
				}
			}
		}
	}
}
