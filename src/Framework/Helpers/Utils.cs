using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Framework.Helpers
{
    internal static class Utils
    {
        public static T Clone<T>(T toBeCloned)
        {
            return JObject.FromObject(toBeCloned)
                .ToObject<T>();
        }

        public static bool IsSpecialOrderAccepted(string key)
        {
            return Game1.player.team.SpecialOrderActive(key);
        }
    }
}
