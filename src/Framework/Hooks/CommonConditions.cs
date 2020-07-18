using PurrplingCore;
using QuestFramework.Framework.Stats;
using QuestFramework.Quests;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Framework.Hooks
{
    internal class CommonConditions
    {
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
                ["QuestAcceptedThisYear"] = (valueToCheck, managedQuest) => IsQuestAcceptedThisYear(valueToCheck, managedQuest),
                ["QuestAcceptedThisSeason"] = (valueToCheck, managedQuest) => IsQuestAcceptedThisSeason(valueToCheck, managedQuest),
                ["QuestAcceptedThisDay"] = (valueToCheck, managedQuest) => IsQuestAcceptedThisDay(valueToCheck, managedQuest),
                ["QuestAcceptedThisWeekDay"] = (valueToCheck, managedQuest) => IsQuestAcceptedThisWeekDay(valueToCheck, managedQuest),
            };
        }

        private static bool IsQuestAcceptedThisYear(string valueToCheck, CustomQuest managedQuest)
        {
            if (!Context.IsWorldReady)
                return false;

            return GetQuestStats(valueToCheck, managedQuest).LastAccepted?.Year == SDate.Now().Year;
        }

        private static bool IsQuestAcceptedThisSeason(string valueToCheck, CustomQuest managedQuest)
        {
            if (!Context.IsWorldReady)
                return false;

            return GetQuestStats(valueToCheck, managedQuest).LastAccepted?.Season == SDate.Now().Season;
        }

        private static bool IsQuestAcceptedThisDay(string valueToCheck, CustomQuest managedQuest)
        {
            if (!Context.IsWorldReady)
                return false;

            return GetQuestStats(valueToCheck, managedQuest).LastAccepted?.Day == SDate.Now().Day;
        }

        private static bool IsQuestAcceptedThisWeekDay(string valueToCheck, CustomQuest managedQuest)
        {
            if (!Context.IsWorldReady)
                return false;

            return GetQuestStats(valueToCheck, managedQuest).LastAccepted?.DayOfWeek == SDate.Now().DayOfWeek;
        }

        private static QuestStatSummary GetQuestStats(string valueToCheck, CustomQuest managedQuest)
        {
            return QuestFrameworkMod.Instance
                .StatsManager
                .GetStats(Game1.player.UniqueMultiplayerID)
                .GetQuestStatSummary(ResolveQuestName(valueToCheck, managedQuest));
        }

        private static string ResolveQuestName(string possibleName, CustomQuest context)
        {
            if (possibleName == "@self")
                return context.GetFullName();

            if (possibleName.Contains('@'))
                return possibleName;

            return $"{possibleName}@{context.OwnedByModUid}";
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
                flag &= Game1.player.getFriendshipHeartLevelForNPC(fships[i])
                    == Convert.ToInt32(fships[i + 1]);
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
