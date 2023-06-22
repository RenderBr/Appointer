using Auxiliary.Configuration;
using CSF;
using CSF.TShock;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using TShockAPI;

namespace Appointer.Modules
{
    [RequirePermission("tbc.user")]
    internal class UserCommands : TSModuleBase<TSCommandContext>
    {

        public AppointerSettings settings = Configuration<AppointerSettings>.Settings;

		[Command("check", "rank", "rankup", "playtime")]
		public async Task<IResult> CheckRank(string user = "")
		{
			if (string.IsNullOrWhiteSpace(user))
				return await CheckOwnRank();

			if (user.ToLower() == "list")
				return RankListCommand();

			return await CheckUserRank(user);
		}

		private async Task<IResult> CheckOwnRank()
		{
			var player = await Appointer.api.RetrieveOrCreatePlaytime(Context.Player);

			Success($"You currently have: {Extensions.ElapsedString(new TimeSpan(0, 0, player.Playtime))} of playtime.");

			string formattedRankCost = await Extensions.NextRankCostFormatted(Context.Player.Account);

			if (formattedRankCost != "You cannot obtain any further ranks!")
				return Info($"You need: [c/90EE90:{formattedRankCost}] left to rank up!");

			return Info(formattedRankCost);
		}

		private async Task<IResult> CheckUserRank(string username)
		{
			var player = await Appointer.api.RetrieveOrCreatePlaytime(username);
			var userAccount = TShock.UserAccounts.GetUserAccountByName(username);

			if (player is null)
				return Error("Invalid player name!");

			Success($"{username} currently has: {Extensions.ElapsedString(new TimeSpan(0, 0, player.Playtime))} of playtime.");
			return Info($"They need: [c/90EE90:{await Extensions.NextRankCostFormatted(userAccount)}] left to rank up!");
		}

		[Command("afk")]
        public IResult AfkCommand()
        {
            Appointer.api.AddPlayerToAFK(Context.Player);
            return Announce($"{Context.Player.Account.Name} is now AFK!", Color.LightYellow);
        }

        [Command("ranklist")]
		public IResult RankListCommand()
		{
			string startGroup = settings.StartGroup;
			string currentPlayerGroup = Context.Player.Group.Name;

			var formattedGroups = settings.Groups.Select(group =>
			{
				string groupName = group.Name;
				string formattedGroupName = char.ToUpper(groupName[0]) + groupName.Substring(1);
				bool isCurrentGroup = (groupName == currentPlayerGroup);
				bool isStartGroup = (groupName == startGroup);
				string prefix = isCurrentGroup ? "[c/00FF00:> " : (isStartGroup && currentPlayerGroup == startGroup) ? "[c/00FF00:" + char.ToUpper(startGroup[0]) + startGroup.Substring(1) + "] " : "> ";

				return prefix + formattedGroupName;
			});

			string message = "Current user rank list: " + string.Join(" ", formattedGroups);

			return Respond(message, Color.LightYellow);
		}




	}
}
