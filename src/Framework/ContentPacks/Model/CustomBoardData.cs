using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Framework.ContentPacks.Model
{
    class CustomBoardData
    {
        public string BoardName { get; set; }
        public string Location { get; set; }
        public Point Tile { get; set; }
        public Dictionary<string, string> UnlockWhen { get; set; }
        public bool ShowIndicator { get; set; } = true;
    }
}
