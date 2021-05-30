using Newtonsoft.Json.Linq;
using QuestFramework.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Extensions
{
    public static class CompletionMessageExtensions
    {
        public static T Cast<T>(this ICompletionMessage completionMessage)
        {
            if (completionMessage is T _completionMessage)
            {
                return _completionMessage;
            }

            return JObject.FromObject(completionMessage).ToObject<T>();
        }

        public static object[] ToArray(this ICompletionMessage completionMessgage)
        {
            return JObject.FromObject(completionMessgage).AsJEnumerable().ToArray();
        }
    }
}
