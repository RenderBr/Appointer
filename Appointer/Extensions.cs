﻿using Auxiliary;
using Auxiliary.Configuration;
using MongoDB.Driver.Linq;
using Appointer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
using Banker.Models;

namespace Appointer
{
    public static class Extensions
    {
        public static int UserGroupIndex(UserAccount plr)
        {
            int index = Configuration<AppointerSettings>.Settings.Groups.FindIndex(x => x.Name.ToLower() == TShock.UserAccounts.GetUserAccount(plr).Group);
            if(plr.Group == Configuration<AppointerSettings>.Settings.StartGroup)
            {
                // CODE FOR START GROUP
                return -111;
            }

            if(index == -1)
            {
                // CODE IF GROUP COULD NOT BE FOUND
                return -404;
            }

            return index;
        }
        public static Group UserCurrentGroup(UserAccount plr)
        {
            if(UserGroupIndex(plr) == -404)
                return null;
            
            if (UserGroupIndex(plr) == -111)
                return new Group("default", Configuration<AppointerSettings>.Settings.Groups[0].Name, 0);

            Group group = Configuration<AppointerSettings>.Settings.Groups[UserGroupIndex(plr)];

            return group;
        }

        public static Group NextGroup(UserAccount plr)
        {
            if (UserGroupIndex(plr) == -404)
            {
                return new Group("unranked", "", -1);
            }
            if (UserGroupIndex(plr) == -111)
            {
                return Configuration<AppointerSettings>.Settings.Groups[0];
            }

            if(UserGroupIndex(plr) + 1 >= Configuration<AppointerSettings>.Settings.Groups.Count || UserCurrentGroup(plr).NextRank == "final")
            {
                return new Group("final", "", -1);
            }

            Group group = Configuration<AppointerSettings>.Settings.Groups[UserGroupIndex(plr)+1];

            return group;
        }

        public async static Task<int> NextRankCost(UserAccount plr)
        {
            var player = await IModel.GetAsync(GetRequest.Bson<TBCUser>(x => x.AccountName == plr.Name), x => x.AccountName = plr.Name);
            BankAccount bankAccount;
            int playtime = player.Playtime;

            if(Configuration<AppointerSettings>.Settings.DoesCurrencyAffectRankTime == true)
            {
                bankAccount = await IModel.GetAsync(GetRequest.Bson<BankAccount>(x => x.AccountName == plr.Name), x => x.AccountName = plr.Name);
                playtime += (int)((Configuration<AppointerSettings>.Settings.CurrencyMultiplier / 100) * bankAccount.Currency);
            }
            
            //final rank code
            if (NextGroup(plr).Name == "final")
            {
                return -666;
            }

            if (UserGroupIndex(plr) == -404)
            {
                return -404;
            }
            if (UserGroupIndex(plr) == -111)
            {
                return Configuration<AppointerSettings>.Settings.Groups[0].Cost-playtime;
            }
            Group group = Configuration<AppointerSettings>.Settings.Groups[UserGroupIndex(plr)];


            int timeLeft = NextGroup(plr).Cost - playtime;
            return timeLeft;

        }

        public async static Task<string> NextRankCostFormatted(UserAccount plr)
        {
            int nextRankCost = await NextRankCost(plr);

            string formatted;

            if(nextRankCost == -404 || nextRankCost == -666)
            {
                formatted = "You cannot obtain any further ranks!";
            }
            else
            {
               formatted = ElapsedString(new TimeSpan(0, 0, NextRankCost(plr).Result));
            }

            return formatted;
            
        }

        public static string ElapsedString(this TimeSpan ts)
        {
            var sb = new StringBuilder();
            if (ts.Days > 0)
                sb.Append(string.Format("{0} day{1}{2}", ts.Days, ts.Days.Suffix(), ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));
            
            if (ts.Hours > 0)
                sb.Append(string.Format("{0} hour{1}{2}", ts.Hours, ts.Hours.Suffix(), ts.Minutes > 0 || ts.Seconds > 0 ? ", " : ""));

            if (ts.Minutes > 0)
                sb.Append(string.Format("{0} minute{1}{2}", ts.Minutes, ts.Minutes.Suffix(), ts.Seconds > 0 ? ", " : ""));

            if (ts.Seconds > 0)
                sb.Append(string.Format("{0} second{1}", ts.Seconds, ts.Seconds.Suffix()));

            if (sb.Length == 0)
                return "an unknown period of time";

            return sb.ToString();
        }

        public static string RemovePrefixOperators(string prefix)
        {
            char[] toTrim = { '(', ')'};

            prefix = prefix.Trim(toTrim);

            return prefix;
        }

        private static string Suffix(this int number)
        {
            return number == 0 || number > 1 ? "s" : "";
        }
    }
}
