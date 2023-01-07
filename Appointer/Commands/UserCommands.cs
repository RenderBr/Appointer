using Auxiliary;
using CSF;
using CSF.TShock;
using Appointer.Models;
using System;
using Auxiliary.Configuration;
using static Appointer.Extensions;
using System.Threading.Tasks;
using TShockAPI;
using MongoDB.Driver.Linq;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Driver.Core.Operations;

namespace Appointer.Modules
{
    [RequirePermission("tbc.user")]
    internal class UserCommands : TSModuleBase<TSCommandContext>
    {

        public AppointerSettings settings = Configuration<AppointerSettings>.Settings; 

        [Command("check", "rank", "rankup", "playtime")]
        public async Task<IResult> CheckRank(string user = "")
        {
            if(user == "")
            {
                var entity = await IModel.GetAsync(GetRequest.Bson<TBCUser>(x => x.AccountName == Context.Player.Account.Name), x => x.AccountName = Context.Player.Account.Name);

                Success($"You currently have: {Extensions.ElapsedString(new TimeSpan(0,0,entity.Playtime))} of playtime.");

                string formatted = await Extensions.NextRankCostFormatted(Context.Player.Account);

                if (!(formatted == "You cannot obtain any further ranks!"))
                {
                    return Info($"You need: [c/90EE90:{formatted}] left to rank up!");

                }
                else
                {
                    return Info(""+ formatted);
                }

            }
            else
            {
                var entity = await IModel.GetAsync(GetRequest.Bson<TBCUser>(x => x.AccountName.ToLower() == user.ToLower()));
                var User = TShock.UserAccounts.GetUserAccountByName(user);

                if(entity == null)
                {
                    return Error("Invalid player name!");
                }

                Success($"{user} currently has: {Extensions.ElapsedString(new TimeSpan(0, 0, entity.Playtime))} of playtime.");
                return Info($"They need: [c/90EE90:{Extensions.NextRankCostFormatted(User)}] left to rank up!");
            }
        }

        [Command("afk")]
        public IResult AfkCommand()
        {
            Appointer.afkPlayers.First(x => x.PlayerName == Context.Player.Name).isAFK = true;
            return Announce($"{Context.Player.Name} is now AFK!", Color.LightYellow);
        }

        [Command("ranklist")]
        public IResult RankListCommand()
        {
            string message = "";

            if(Context.Player.Group.Name == settings.StartGroup)
                message += message += "[c/00FF00:" + char.ToUpper(settings.StartGroup[0]) + settings.StartGroup.Substring(1) + "] ";
            else
                message += char.ToUpper(settings.StartGroup[0]) + settings.StartGroup.Substring(1) + " ";

            foreach (Group group in settings.Groups)
            {
                if (group.Name == Context.Player.Group.Name)
                {
                    message += "[c/00FF00:> " + char.ToUpper(group.Name[0]) + group.Name.Substring(1) + "] ";
                    continue;
                }
                message += "> " + char.ToUpper(group.Name[0]) + group.Name.Substring(1) + " ";
            }
            Respond("Current user rank list: ");
            return Respond(message, Color.LightYellow);
            
                
        }
        
        
    }
}
