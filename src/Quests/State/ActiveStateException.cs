using System;
using System.Runtime.Serialization;

namespace QuestFramework.Quests.State
{
    [Serializable]
    public class ActiveStateException : Exception
    {
        public ActiveStateException()
        {
        }

        public ActiveStateException(string message) : base(message)
        {
        }

        public ActiveStateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ActiveStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}