using System;
using StardewModdingAPI;
using StardewValley;


namespace QuestFramework
{
    class ConversationTopic
    {
        internal protected static IMonitor Monitor => QuestFrameworkMod.Instance.Monitor;
        static public void AddConversationTopic(string addConversationTopicInput)
        {
            string[] convTopicToAddParts = addConversationTopicInput.Split(' ');

            if (convTopicToAddParts.Length % 2 == 0 && convTopicToAddParts.Length >= 2)
            {
                for (int i = 0; i < convTopicToAddParts.Length; i += 2)
                {
                    string convTopicToAdd = convTopicToAddParts[i];
                    int convTopicCompletedDaysActive = Convert.ToInt32(convTopicToAddParts[i + 1]);

                    Game1.player.activeDialogueEvents.Add(convTopicToAdd, convTopicCompletedDaysActive);
                    Monitor.Log($"Added conversation topic with the key: `{convTopicToAdd}`" +
                        $"the conversation topic will be active for `{convTopicCompletedDaysActive}` days");
                }
            }

        }
        static public void RemoveConversationTopic(string removeConversationTopicInput)
        {
            string[] convTopicToRemoveParts = removeConversationTopicInput.Split(' ');

            for (int i = 0; i < convTopicToRemoveParts.Length; i += 1)
            {
                string convTopicToRemove = convTopicToRemoveParts[i];
                if (Game1.player.activeDialogueEvents.ContainsKey(convTopicToRemove))
                {
                    Game1.player.activeDialogueEvents.Remove(convTopicToRemove);
                    Monitor.Log($"Removed conversation topic with the key: `{convTopicToRemove}`");
                }
                else Monitor.Log($"Failed to remove conversation topic, the key`{convTopicToRemove}` already inactive");


            }
        }

    }
}
