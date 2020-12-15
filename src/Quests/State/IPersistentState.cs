using Newtonsoft.Json.Linq;

namespace QuestFramework.Quests.State
{
    public interface IPersistentState
    {
        JObject GetState();
        void Reset();
        void SetState(JObject state);
    }
}
