﻿using StardewValley;

namespace QuestFramework.Quests
{
    public interface ICompletionArgs : ICompletionMessage
    {
        /// <summary>
        /// Interaction with NPC
        /// </summary>
        NPC Npc { get; }

        /// <summary>
        /// First number to test
        /// </summary>
        int Number1 { get; }

        /// <summary>
        /// Second number to test
        /// </summary>
        int Number2 { get; }

        /// <summary>
        /// Which item was used for check quest completion
        /// </summary>
        Item Item { get; }

        /// <summary>
        /// Text to test
        /// </summary>
        string String { get; }

        /// <summary>
        /// Type of completion source (Quest type id)
        /// </summary>
        int CompletionType { get; }
    }
}
