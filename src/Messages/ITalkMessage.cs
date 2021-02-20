using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Messages
{
    public interface ITalkMessage
    {
        Farmer Farmer { get; }
        NPC Npc { get; }
    }
}
