using QuestFramework.Messages;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Framework.Messages
{
    internal class TalkMessage : ITalkMessage
    {
        public Farmer Farmer { get; }
        public NPC Npc { get; }

        public TalkMessage(Farmer farmer, NPC npc)
        {
            this.Farmer = farmer;
            this.Npc = npc;
        }
    }
}
