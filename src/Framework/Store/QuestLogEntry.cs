using StardewValley.Quests;

namespace QuestFramework.Framework.Store
{
    internal class QuestLogEntry
    {
        public QuestLogEntry()
        {

        }

        public QuestLogEntry(long farmerId, string fullQuestName, Quest quest) : this()
        {
            this.FarmerId = farmerId;
            this.FullQuestName = fullQuestName;
            this.Quest = quest;
        }

        public string FullQuestName { get; set; }
        public long FarmerId { get; set; }
        public Quest Quest { get; set; }
    }
}