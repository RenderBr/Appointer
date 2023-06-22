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
		public const int RankCodeStart = -111;
		public const int RankCodeFinal = -666;
		public const int RankCodeUnknown = -404;
		public static int UserGroupIndex(UserAccount plr)
		{
			var userGroup = TShock.UserAccounts.GetUserAccount(plr).Group;
			var groupIndex = Configuration<AppointerSettings>.Settings.Groups.FindIndex(x =>
				x.Name.Equals(userGroup, StringComparison.OrdinalIgnoreCase));

			if (userGroup.Equals(Configuration<AppointerSettings>.Settings.StartGroup, StringComparison.OrdinalIgnoreCase))
				return RankCodeStart;

			if (groupIndex == -1)
				return RankCodeUnknown;

			return groupIndex;
		}

		public static Group UserCurrentGroup(UserAccount plr)
		{
			var userGroupIndex = UserGroupIndex(plr);

			if (userGroupIndex == RankCodeUnknown)
				return null;

			if (userGroupIndex == RankCodeStart)
				return new Group("default", Configuration<AppointerSettings>.Settings.Groups[0].Name, 0);

			return Configuration<AppointerSettings>.Settings.Groups[userGroupIndex];
		}

		public static Group NextGroup(UserAccount plr)
		{
			var userGroupIndex = UserGroupIndex(plr);

			if (userGroupIndex == RankCodeUnknown || plr is null)
				return new Group("unranked", "", -1);

			if (userGroupIndex == RankCodeStart)
				return Configuration<AppointerSettings>.Settings.Groups[0];

			if (userGroupIndex + 1 >= Configuration<AppointerSettings>.Settings.Groups.Count ||
				UserCurrentGroup(plr).NextRank.Equals("final", StringComparison.OrdinalIgnoreCase))
				return new Group("final", "", -1);

			return Configuration<AppointerSettings>.Settings.Groups[userGroupIndex + 1];
		}

		public static async Task<int> NextRankCost(UserAccount plr)
		{
			if (plr is null)
				return RankCodeUnknown;

			var player = await Appointer.api.RetrieveOrCreatePlaytime(plr.Name);
			var bankApi = new BankerApi();
			IBankAccount bankAccount;
			var playtime = player.Playtime;

			if (Configuration<AppointerSettings>.Settings.DoesCurrencyAffectRankTime)
			{
				bankAccount = await bankApi.RetrieveOrCreateBankAccount(plr.Name);
				playtime += (int)(Configuration<AppointerSettings>.Settings.CurrencyMultiplier / 100 * bankAccount.Currency);
			}

			if (NextGroup(plr).Name.Equals("final", StringComparison.OrdinalIgnoreCase))
				return RankCodeFinal;

			if (UserGroupIndex(plr) == RankCodeUnknown)
				return RankCodeUnknown;

			if (UserGroupIndex(plr) == RankCodeStart)
				return Configuration<AppointerSettings>.Settings.Groups[0].Cost - playtime;

			var group = Configuration<AppointerSettings>.Settings.Groups[UserGroupIndex(plr)];
			var timeLeft = NextGroup(plr).Cost - playtime;
			return timeLeft;
		}

		public static async Task<string> NextRankCostFormatted(UserAccount plr)
		{
			var nextRankCost = await NextRankCost(plr);
			string formatted;

			switch (nextRankCost)
			{
				case RankCodeUnknown:
					formatted = "You are not in the rank chain!";
					break;
				case RankCodeFinal:
					formatted = "You cannot obtain any further ranks!";
					break;
				default:
					formatted = ElapsedString(new TimeSpan(0, 0, nextRankCost));
					break;
			}

			return formatted;
		}

		public static string ElapsedString(this TimeSpan ts)
		{
			var sb = new StringBuilder();

			if (ts.Days > 0)
				sb.Append($"{ts.Days} day{(ts.Days > 1 ? "s" : "")}{(ts.Hours > 0 || ts.Minutes > 0 || ts.Seconds > 0 ? ", " : "")}");

			if (ts.Hours > 0)
				sb.Append($"{ts.Hours} hour{(ts.Hours > 1 ? "s" : "")}{(ts.Minutes > 0 || ts.Seconds > 0 ? ", " : "")}");

			if (ts.Minutes > 0)
				sb.Append($"{ts.Minutes} minute{(ts.Minutes > 1 ? "s" : "")}{(ts.Seconds > 0 ? ", " : "")}");

			if (ts.Seconds > 0)
				sb.Append($"{ts.Seconds} second{(ts.Seconds > 1 ? "s" : "")}");

			if (sb.Length == 0)
				return "an unknown period of time";

			return sb.ToString();
		}

		public static string TrimPrefixStyling(string prefix) => prefix.Trim('(', ')');
	}
}
