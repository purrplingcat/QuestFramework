using Newtonsoft.Json;

namespace QuestFramework.Quests
{
    public class CustomQuestObjective
    {
        public CustomQuestObjective(string tag, string text)
        {
            this.Tag = tag;
            this.Text = text;
        }

        public string Tag { get; set; }
        public string Text { get; set; }
        public bool IsCompleted { get; set; }
    }
}