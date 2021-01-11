using Microsoft.Xna.Framework;
using StardewValley.Menus;
using System;

namespace QuestFramework.Framework.Structures
{
    public class CustomBoardTrigger
    {
        public string LocationName { get; set; }
        public Point Tile { get; set; }
        public string BoardName { get; set; }
        public bool ShowIndicator { get; set; } = true;

        public Func<bool> unlockConditionFunc;

        public bool IsUnlocked()
        {
            return this.unlockConditionFunc == null || this.unlockConditionFunc();
        }
    }
}