using System;
using System.Runtime.Serialization;

namespace QuestFramework.Quests.State
{
    [Serializable]
    public sealed class ActiveStateException : Exception
    {
        internal ActiveStateException()
        {
        }

        internal ActiveStateException(string message) : base(message)
        {
        }

        internal ActiveStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}