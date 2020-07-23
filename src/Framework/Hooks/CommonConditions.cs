using PurrplingCore;
using QuestFramework.Framework.Stats;
using QuestFramework.Quests;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;
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
                ["Weather"] = (valueToCheck, _) => valueToCheck.ToLower().GetCurrentWeatherName() == valueToCheck,
                ["Date"] = (valueToCheck, _) => SDate.Now() == Utils.ParseDate(valueToCheck),
                ["Days"] = (valueToCheck, _) => Utility.parseStringToIntArray(valueToCheck).Any(d => d == SDate.Now().Day),
                ["Seasons"] = (valueToCheck, _) => valueToCheck.ToLower().Split(' ').Any(s => s == SDate.Now().Season),
                ["DaysOfWeek"] = (valueToCheck, _) => valueToCheck.Split(' ').Any(
                        d => d.ToLower() == SDate.Now().DayOfWeek.ToString().ToLower()),
                ["FriendshipLevel"] = (valueToCheck, _) => CheckFriendshipLevel(valueToCheck), //Emily 7
                ["FriendshipStatus"] = (valueToCheck, _) => CheckFriendshipStatus(valueToCheck), //[Shane Dating]_Valid status: Friendly, Dating, Engaged, Married, Divorced
                ["MailReceived"] = (valueToCheck, _) => CheckReceivedMailCondition(valueToCheck),
                ["EventSeen"] = (valueToCheck, _) => CheckEventSeenCondition(valueToCheck),
                ["MinDaysPlayed"] = (valueToCheck, _) => Game1.Date.TotalDays >= Convert.ToInt32(valueToCheck),
                ["MaxDaysPlayed"] = (valueToCheck, _) => Game1.Date.TotalDays <= Convert.ToInt32(valueToCheck),
                ["DaysPlayed"] = (valueToCheck, _) => Game1.Date.TotalDays == Convert.ToInt32(valueToCheck),
                ["IsPlayerMarried"] = (valueToCheck, _) => ParseBool(valueToCheck) == Game1.player.isMarried(),
                ["QuestAcceptedInPeriod"] = (valueToCheck, managedQuest) => IsQuestAcceptedInPeriod(valueToCheck, managedQuest),
                ["QuestAcceptedDate"] = (valueToCheck, managedQuest) => IsQuestAcceptedDate(valueToCheck, managedQuest),
                ["SkillLevel"] = (valueToCheck, _) => CheckSkillLevel(valueToCheck), //[Farming 1 Foraging 2]_Valid skill: Farming, Fishing, Foraging, Mining, Combat, Luck
                ["KnownCraftingRecipe"] = (valueToCheck, _) => Game1.player.craftingRecipes.ContainsKey(valueToCheck),
                ["KnownCookingRecipe"] = (valueToCheck, _) => Game1.player.cookingRecipes.ContainsKey(valueToCheck),
                ["CompletedCommunityCenter"] = (valueToCheck, _) => ParseBool(valueToCheck) == Game1.player.hasCompletedCommunityCenter(),
                ["ConstructedBuilding"] = (valueToCheck, _) => CheckBuilding(valueToCheck), //[Barn, Deluxe Barn]_I guess most building will work?
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

        public static bool CheckFriendshipLevel(string friendshipLevel)
        {
            string[] flevel = friendshipLevel.Split(' ');
            bool flag = true;

            if (flevel.Length < 2)
                return false;

            for (int i = 0; i < flevel.Length; i += 2)
            {
                string whoLevel = flevel[i];
                int currentFriendshipLevel = Game1.player.getFriendshipHeartLevelForNPC(whoLevel);
                int expectedFriendshipLevel = Convert.ToInt32(flevel[i + 1]);

                flag &= currentFriendshipLevel >= expectedFriendshipLevel;

                if (Monitor.IsVerbose)
                    Monitor.Log(
                        $"Checked friendship level for `{whoLevel}`, " +
                        $"current level: {currentFriendshipLevel}, " +
                        $"expected: {expectedFriendshipLevel}, " +
                        $"current flag: {flag}");
            }

            return flag;
        }

        public static bool CheckFriendshipStatus(string friendshipStatus) 
        {
            string[] fstatus = friendshipStatus.Split(' ');
            bool flag = true;

            if (fstatus.Length < 2)
                return false;

            for (int i = 0; i < fstatus.Length; i += 2)
            {
                string whoStatus = fstatus[i];
                string currentStatus = Game1.player.friendshipData[whoStatus].Status.ToString();
                string expectedStatus = fstatus[i + 1];

                flag &= currentStatus == expectedStatus;

                if (Monitor.IsVerbose)
                    Monitor.Log(
                        $"Checked friendship status for `{whoStatus}`, " +
                        $"current status: {currentStatus}, " +
                        $"expected status: {expectedStatus}, " +
                        $"current flag: {flag}");
            }
            
            return flag;
        }

        public static bool CheckSkillLevel(string skillLevel) 
        {
            string[] slevel = skillLevel.Split(' ');
            bool flag = true;

            if (slevel.Length < 2)
                return false;

            for (int i = 0; i < slevel.Length; i += 2)
            {
                string[] skillList = { "Farming", "Fishing", "Foraging", "Mining", "Combat", "Luck"};
                string skillName = slevel[i];
                int skillId = Array.IndexOf(skillList, slevel[i]);
                int currentSkillLevel = Game1.player.getEffectiveSkillLevel(skillId);
                int expectedSkillLevel = Convert.ToInt32(slevel[i + 1]);

                flag &= currentSkillLevel >= expectedSkillLevel;

                if (Monitor.IsVerbose)
                    Monitor.Log(
                        $"Checked skill level for `{skillName}`, " +
                        $"with skill id: {skillId}, " +
                        $"current level: {currentSkillLevel}, " +
                        $"expected level: {expectedSkillLevel}, " +
                        $"current flag: {flag}");
            }

            return flag;
        }

        public static bool CheckBuilding(string checkedBuilding)
        {
            checkedBuilding = checkedBuilding.Replace(", ", "|");
            string[] building = checkedBuilding.Split('|');

            bool flag = true;
            for (int i = 0; i < building.Length; i += 1)
            {
                string buildingName = building[i];
                bool isBuildingConstructed = Game1.getFarm().isBuildingConstructed(building[i]);
                flag &= isBuildingConstructed;

                if (Monitor.IsVerbose)
                    Monitor.Log(
                        $"Checked `{buildingName}` is built in farm" +
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
