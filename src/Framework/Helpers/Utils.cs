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
            foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
            {
                if (specialOrder.questKey.Value == key)
                    return true;
            }

            return false;
        }
    }
}
