using Auxiliary.Configuration;
using CSF;
using CSF.TShock;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using TShockAPI;

namespace Appointer.Modules
{
    [RequirePermission("tbc.user")]
    internal class UserCommands : TSModuleBase<TSCommandContext>
    {

        public AppointerSettings Settings => Configuration<AppointerSettings>.Settings;

        [Command("check", "rank", "rankup", "playtime")]
        public IResult CheckRank(string user = "")
        {
            if (string.IsNullOrWhiteSpace(user))
                return CheckOwnRank(Context.Player.Account.Name);

            if (user.ToLower() == "list")
                return RankListCommand();

            return CheckUserRank(user);
        }

        private IResult CheckOwnRank(string accName)
        {
            var player = Appointer.api.RetrievePlaytime(accName);
            if (player == null) return Error("You must be logged in to use this command!");

            Success($"You currently have: {Extensions.ElapsedString(new TimeSpan(0, 0, player.Playtime))} of playtime.");

            string formattedRankCost = Extensions.NextRankCostFormatted(Context.Player.Account);

            if (formattedRankCost != "You cannot obtain any further ranks!")
                return Info($"You need: [c/90EE90:{formattedRankCost}] left to rank up!");

            return Info(formattedRankCost);
        }

        private IResult CheckUserRank(string username)
        {
            var player = Appointer.api.RetrievePlaytime(username);
            var userAccount = TShock.UserAccounts.GetUserAccountByName(username);

            if (player is null)
                return Error("Invalid player name!");

            Success($"{username} currently has: {Extensions.ElapsedString(new TimeSpan(0, 0, player.Playtime))} of playtime.");
            return Info($"They need: [c/90EE90:{Extensions.NextRankCostFormatted(userAccount)}] left to rank up!");
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
            string startGroup = Settings.StartGroup;
            string currentPlayerGroup = Context.Player.Group.Name;

            var formattedGroups = Settings.Groups.Select(group =>
            {
                string groupName = group.Name;
                string formattedGroupName = char.ToUpper(groupName[0]) + groupName.Substring(1);
                bool isCurrentGroup = groupName == currentPlayerGroup;
                bool isStartGroup = groupName == startGroup;
                string prefix = isCurrentGroup ? "[c/00FF00:> " : (isStartGroup && currentPlayerGroup == startGroup) ? "[c/00FF00:" + char.ToUpper(startGroup[0]) + startGroup.Substring(1) + "] " : "> ";

                return prefix + formattedGroupName;
            });

            string message = "Current user rank list: " + string.Join(" ", formattedGroups);

            return Respond(message, Color.LightYellow);
        }




    }
}
