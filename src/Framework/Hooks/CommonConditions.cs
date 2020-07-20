using PurrplingCore;
using QuestFramework.Framework.Stats;
using QuestFramework.Quests;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuestFramework.Framework.Hooks
{
    internal class CommonConditions
    {
        internal protected static IMonitor Monitor => QuestFrameworkMod.Instance.Monitor;
        public static Dictionary<string, Func<string, CustomQuest, bool>> GetConditions()
        {
            return new Dictionary<string, Func<string, CustomQuest, bool>>()
            {
                ["Weather"] = (valueToCheck, _) => GetCurrentWeatherName() == valueToCheck,
                ["Date"] = (valueToCheck, _) => SDate.Now() == Utils.ParseDate(valueToCheck),
                ["Days"] = (valueToCheck, _) => Utility.parseStringToIntArray(valueToCheck).Any(d => d == SDate.Now().Day),
                ["Seasons"] = (valueToCheck, _) => valueToCheck.Split(' ').Any(s => s == SDate.Now().Season),
                ["DaysOfWeek"] = (valueToCheck, _) => valueToCheck.Split(' ').Any(
                        d => d.ToLower() == SDate.Now().DayOfWeek.ToString().ToLower()),
                ["Friendship"] = (valueToCheck, _) => CheckFriendshipCondition(valueToCheck),
                ["MailReceived"] = (valueToCheck, _) => CheckReceivedMailCondition(valueToCheck),
                ["EventSeen"] = (valueToCheck, _) => CheckEventSeenCondition(valueToCheck),
                ["MinDaysPlayed"] = (valueToCheck, _) => Game1.Date.TotalDays >= Convert.ToInt32(valueToCheck),
                ["MaxDaysPlayed"] = (valueToCheck, _) => Game1.Date.TotalDays <= Convert.ToInt32(valueToCheck),
                ["DaysPlayed"] = (valueToCheck, _) => Game1.Date.TotalDays == Convert.ToInt32(valueToCheck),
                ["IsPlayerMarried"] = (valueToCheck, _) => ParseBool(valueToCheck) == Game1.player.isMarried(),
                ["QuestAcceptedInPeriod"] = (valueToCheck, managedQuest) => IsQuestAcceptedInPeriod(valueToCheck, managedQuest),
                ["QuestAcceptedDate"] = (valueToCheck, managedQuest) => IsQuestAcceptedDate(valueToCheck, managedQuest),
                ["KnownCraftingRecipe"] = (valueToCheck, _) => Game1.player.craftingRecipes.ContainsKey(valueToCheck),
                ["KnownCookingRecipe"] = (valueToCheck, _) => Game1.player.cookingRecipes.ContainsKey(valueToCheck),
                ["Random"] = (valueToCheck, _) => Game1.random.NextDouble() < Convert.ToDouble(valueToCheck) / 100, // Chance is in %
            };
        }

        private static bool IsQuestAcceptedDate(string valueToCheck, CustomQuest managedQuest)
        {
            return GetQuestStats(managedQuest).LastAccepted == Utils.ParseDate(valueToCheck);
        }

        private static bool IsQuestAcceptedInPeriod(string valueToCheck, CustomQuest managedQuest)
        {
            if (!Context.IsWorldReady)
                return false;

            var parts = valueToCheck.Split(' ');
            var acceptDate = GetQuestStats(managedQuest).LastAccepted;
            SDate now = SDate.Now();
            
            Monitor.VerboseLog(
                $"Checking quest accept date `{acceptDate}` matches current `{now}` by `{valueToCheck}`");

            if (parts.Length < 1 || acceptDate == null || (parts.Contains("today") && acceptDate != now))
                return false;

            bool flag = true;
            foreach (string part in parts)
            {
                string[] period = part.Split('=');
                string type = period.ElementAtOrDefault(0);
                string value = period.ElementAtOrDefault(1);

                switch (type)
                {
                    case "y":
                    case "year":
                        flag &= acceptDate.Year == (
                            int.TryParse(value, out var year) 
                                ? year 
                                : now.Year
                            );
                        break;
                    case "s":
                    case "season":
                        flag &= acceptDate.Season == (value ?? now.Season);
                        break;
                    case "wd":
                    case "weekday":
                        flag &= acceptDate.DayOfWeek == (
                            Enum.TryParse<DayOfWeek>(value, out var dayOfWeek) 
                                ? dayOfWeek 
                                : now.DayOfWeek
                            );
                        break;
                    case "d":
                    case "day":
                        flag &= acceptDate.Day == (
                            int.TryParse(value, out var day) 
                                ? day
                                : now.Day
                            );
                        break;
                    default:
                        flag &= false;
                        break;
                }
            }

            return flag;
        }

        private static QuestStatSummary GetQuestStats(CustomQuest managedQuest)
        {
            return QuestFrameworkMod.Instance
                .StatsManager
                .GetStats(Game1.player.UniqueMultiplayerID)
                .GetQuestStatSummary(managedQuest.GetFullName());
        }

        public static bool CheckEventSeenCondition(string valueToCheck)
        {
            int[] events = Utility.parseStringToIntArray(valueToCheck);
            bool flag = true;

            if (events.Length < 1)
                return false;

            foreach (var ev in events)
            {
                flag &= Game1.player.eventsSeen.Contains(ev);
                Monitor.VerboseLog($"Checked if event `{ev}` was seen. Current flag: {flag}");
            }

            return flag;
        }

        public static bool CheckReceivedMailCondition(string valueToCheck)
        {
            string[] mails = valueToCheck.Split(' ');
            bool flag = true;

            if (mails.Length < 1)
                return false;

            foreach (string mail in mails)
            {
                flag &= Game1.player.mailReceived.Contains(mail);
                Monitor.VerboseLog($"Checked if mail letter `{mail}` was received. Current flag: {flag}");
            }

            return flag;
        }

        public static bool CheckFriendshipCondition(string friendshipDefinition)
        {
            string[] fships = friendshipDefinition.Split(' ');
            bool flag = true;

            if (fships.Length < 2)
                return false;

            for (int i = 0; i < fships.Length; i += 2)
            {
                string who = fships[i];
                int currentLevel = Game1.player.getFriendshipHeartLevelForNPC(who);
                int expectedLevel = Convert.ToInt32(fships[i + 1]);

                flag &= currentLevel >= expectedLevel;
                
                if (Monitor.IsVerbose)
                    Monitor.Log(
                        $"Checked friendship level for `{who}`, " +
                        $"current level: {currentLevel}, " +
                        $"expected: {expectedLevel}, " +
                        $"current flag: {flag}");
            }

            return flag;
        }

        private static bool ParseBool(string str)
        {
            var truthyVals = new string[] { "true", "yes", "1", "on", "enabled" };
            var falsyVals = new string[] { "false", "no", "0", "off", "disabled" };
            str = str.ToLower();

            if (truthyVals.Contains(str))
                return true;

            if (falsyVals.Contains(str))
                return false;

            throw new InvalidCastException($"Unable to convert `{str}` to boolean.");
        }

        private static string GetCurrentWeatherName()
        {
            if (Game1.isRaining)
                return "rainy";
            if (Game1.isSnowing)
                return "snowy";
            if (Game1.isLightning)
                return "stormy";
            if (Game1.isDebrisWeather)
                return "cloudy";

            return "sunny";
        }
    }
}
