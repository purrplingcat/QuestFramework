using System;
using System.Runtime.Serialization;

namespace QuestFramework.Framework.Helpers
{
    [Serializable]
    internal class ActiveStateFieldException : Exception
    {
        public ActiveStateFieldException()
        {
        }

        public ActiveStateFieldException(string message) : base(message)
        {
        }

        public ActiveStateFieldException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ActiveStateFieldException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}