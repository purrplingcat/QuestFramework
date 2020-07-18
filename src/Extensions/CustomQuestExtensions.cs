using QuestFramework.Quests;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.src.Extensions
{
    public static class CustomQuestExtensions
    {
        /// <summary>
        /// Complete this managed quest if this quest is accepted in player's quest log
        /// </summary>
        /// <param name="customQuest"></param>
        public static void Complete(this CustomQuest customQuest)
        {
            if (Context.IsWorldReady && Game1.player.hasQuest(customQuest.id))
                Game1.player.completeQuest(customQuest.id);
        }

        /// <summary>
        /// Accept this managed quest and add them to player's quest log if this quest not already accepted in the log.
        /// </summary>
        /// <param name="customQuest"></param>
        public static void Accept(this CustomQuest customQuest)
        {
            if (Context.IsWorldReady)
                Game1.player.addQuest(customQuest.id);
        }

        /// <summary>
        /// Remove this managed quest from player's quest log without completion.
        /// </summary>
        /// <param name="customQuest"></param>
        public static void RemoveFromQuestLog(this CustomQuest customQuest)
        {
            if (Context.IsWorldReady)
                Game1.player.removeQuest(customQuest.id);
        }

        /// <summary>
        /// Get connected vanilla quest with this managed quest in quest log
        /// </summary>
        /// <param name="customQuest"></param>
        /// <returns>null if this quest is not in quest log, otherwise the connected vanilla quest</returns>
        public static Quest GetInQuestLog(this CustomQuest customQuest)
        {
            if (Context.IsWorldReady)
                return null;

            return Game1.player.questLog
                .Where(q => q.id.Value == customQuest.id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Check if this quest is included in player's quest log.
        /// </summary>
        /// <param name="customQuest"></param>
        /// <returns></returns>
        public static bool IsInQuestLog(this CustomQuest customQuest)
        {
            return Context.IsWorldReady && Game1.player.hasQuest(customQuest.id);
        }

        /// <summary>
        /// Check if this quest is included and accepted in player's quest log.
        /// </summary>
        /// <param name="customQuest"></param>
        /// <returns></returns>
        public static bool IsAccepted(this CustomQuest customQuest)
        {
            return IsInQuestLog(customQuest) && GetInQuestLog(customQuest).accepted.Value;
        }
    }
}
