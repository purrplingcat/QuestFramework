using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuestFramework.Framework.Menus;
using StardewModdingAPI;
using StardewValley.Menus;
using System;

namespace QuestFramework.Framework.Structures
{
    public sealed class CustomBoardTrigger
    {
        public string LocationName { get; set; }
        public Point Tile { get; set; }
        public string BoardName { get; set; }
        public bool ShowIndicator { get; set; } = true;
        public BoardType BoardType { get; set; } = BoardType.Quests;
        public Texture2D Texture { get; set; }

        public Func<bool> unlockConditionFunc;

        public bool IsUnlocked()
        {
            return this.unlockConditionFunc == null || this.unlockConditionFunc();
        }
    }

    public enum BoardType
    {
        Quests,
        SpecialOrders,
    }
}