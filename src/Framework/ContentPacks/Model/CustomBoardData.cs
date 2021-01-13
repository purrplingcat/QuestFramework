using Microsoft.Xna.Framework;
using QuestFramework.Framework.Structures;
using System.Collections.Generic;

namespace QuestFramework.Framework.ContentPacks.Model
{
    class CustomBoardData
    {
        public string BoardName { get; set; }
        public BoardType BoardType { get; set; } = BoardType.Quests;
        public string Location { get; set; }
        public Point Tile { get; set; }
        public Dictionary<string, string> UnlockWhen { get; set; }
        public bool ShowIndicator { get; set; } = true;
        public string Texture { get; set; }
    }
}
