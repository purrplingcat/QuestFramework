using QuestFramework.Quests;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.src.Extensions
{
    public static class CustomQuestExtensions
    {
        public static void Complete(this CustomQuest customQuest)
        {
            if (Context.IsWorldReady && Game1.player.hasQuest(customQuest.id))
                Game1.player.completeQuest(customQuest.id);
        }

        public static void Accept(this CustomQuest customQuest)
        {
            if (Context.IsWorldReady)
                Game1.player.addQuest(customQuest.id);
        }

        public static void RemoveFromQuestLog(this CustomQuest customQuest)
        {
            if (Context.IsWorldReady)
                Game1.player.removeQuest(customQuest.id);
        }
    }
}
