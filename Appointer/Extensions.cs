using Auxiliary.Configuration;
using Banker.Api;
using Banker.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;

namespace Appointer
{
    public static class Extensions
    {
        internal class RankCode
        {
            public const int Start = -111;
            public const int Final = -666;
            public const int Unknown = -404;

        }

        public static int UserGroupIndex(UserAccount plr)
        {
            int index = Configuration<AppointerSettings>.Settings.Groups.FindIndex(x => x.Name.ToLower() == TShock.UserAccounts.GetUserAccount(plr).Group);
            if (plr.Group == Configuration<AppointerSettings>.Settings.StartGroup)
            {
                // CODE FOR START GROUP
                return RankCode.Start;
            }

            if (index == -1)
            {
                // CODE IF GROUP COULD NOT BE FOUND
                return RankCode.Unknown;
            }

            return index;
        }
        public static Group UserCurrentGroup(UserAccount plr)
        {
            if (UserGroupIndex(plr) == RankCode.Unknown)
                return null;

            if (UserGroupIndex(plr) == RankCode.Start)
                return new Group("default", Configuration<AppointerSettings>.Settings.Groups[0].Name, 0);

            Group group = Configuration<AppointerSettings>.Settings.Groups[UserGroupIndex(plr)];

            return group;
        }

        public static Group NextGroup(UserAccount plr)
        {
            if (UserGroupIndex(plr) == RankCode.Unknown || plr == null)
            {
                return new Group("unranked", "", -1);
            }
            if (UserGroupIndex(plr) == RankCode.Start)
            {
                return Configuration<AppointerSettings>.Settings.Groups[0];
            }

            if (UserGroupIndex(plr) + 1 >= Configuration<AppointerSettings>.Settings.Groups.Count || UserCurrentGroup(plr).NextRank == "final")
            {
                return new Group("final", "", -1);
            }

            Group group = Configuration<AppointerSettings>.Settings.Groups[UserGroupIndex(plr) + 1];

            return group;
        }

        public async static Task<int> NextRankCost(UserAccount plr)
        {
            if (plr == null)
            {
                return RankCode.Unknown;
            }

            var player = await Appointer.api.RetrieveOrCreatePlaytime(plr.Name);
            BankerApi bankApi = new();
            IBankAccount bankAccount;
            int playtime = player.Playtime;

            if (Configuration<AppointerSettings>.Settings.DoesCurrencyAffectRankTime == true)
            {
                bankAccount = await bankApi.RetrieveOrCreateBankAccount(plr.Name);
                playtime += (int)(Configuration<AppointerSettings>.Settings.CurrencyMultiplier / 100 * bankAccount.Currency);
            }

            //final rank code
            if (NextGroup(plr).Name == "final")
            {
                return RankCode.Final;
            }

            if (UserGroupIndex(plr) == RankCode.Unknown)
            {
                return RankCode.Unknown;
            }
            if (UserGroupIndex(plr) == RankCode.Start)
            {
                return Configuration<AppointerSettings>.Settings.Groups[0].Cost - playtime;
            }
            Group group = Configuration<AppointerSettings>.Settings.Groups[UserGroupIndex(plr)];

            int timeLeft = NextGroup(plr).Cost - playtime;
            return timeLeft;

        }

        public async static Task<string> NextRankCostFormatted(UserAccount plr)
        {
            int nextRankCost = await NextRankCost(plr);

            string formatted;

            switch (nextRankCost)
            {
                case RankCode.Unknown:
                    {
                        formatted = "You are not in the rank chain!";
                        break;
                    }
                case RankCode.Final:
                    {
                        formatted = "You cannot obtain any further ranks!";
                        break;
                    }
                default:
                    {
                        formatted = ElapsedString(new TimeSpan(0, 0, NextRankCost(plr).Result));
                        break;
                    }


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
            char[] toTrim = { '(', ')' };

            prefix = prefix.Trim(toTrim);

            return prefix;
        }

        private static string Suffix(this int number)
        {
            return number == 0 || number > 1 ? "s" : "";
        }
    }
}
