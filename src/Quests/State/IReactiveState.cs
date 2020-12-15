using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace QuestFramework.Quests.State
{
    internal interface IReactiveState
    {
        [JsonIgnore]
        bool WasChanged { get; }

        event Action<JObject> OnChange;
        void Initialize(CustomQuest customQuest);
        void StateSynced();
    }
}
