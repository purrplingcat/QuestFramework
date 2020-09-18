namespace QuestFramework.Framework.ContentPacks.Model
{
    internal class ConversationTopicData
    {
        public string AddWhenQuestAccepted { get; set; }
        public string AddWhenQuestRemoved { get; set; }
        public string AddWhenQuestCompleted { get; set; }
        public string RemoveWhenQuestCompleted { get; set; }
        public string RemoveWhenQuestRemoved { get; set; }
        public string RemoveWhenQuestAccepted { get; set; }
    }
}