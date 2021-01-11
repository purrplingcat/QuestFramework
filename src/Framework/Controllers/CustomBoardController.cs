using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuestFramework.Framework.Menus;
using QuestFramework.Framework.Structures;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Framework.Controllers
{
    class CustomBoardController
    {
        private readonly List<CustomBoardTrigger> _customBoardTriggers;
        private CustomBoardTrigger[] _currentLocationBoardTriggers;

        public CustomBoardController(IModEvents events)
        {
            events.Player.Warped += this.OnPlayerWarped;
            events.Display.RenderedWorld += this.OnWorldRendered;
            this._customBoardTriggers = new List<CustomBoardTrigger>();
        }

        private void OnWorldRendered(object sender, RenderedWorldEventArgs e)
        {
            if (Game1.eventUp || this._currentLocationBoardTriggers == null)
                return;

            foreach (var boardTrigger in this._currentLocationBoardTriggers)
            {
                if (!ShouldShowIndicator(boardTrigger))
                    continue;

                float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                e.SpriteBatch.Draw(Game1.mouseCursors,
                    Game1.GlobalToLocal(
                        Game1.viewport, new Vector2(boardTrigger.Tile.X * 64 + 8, (boardTrigger.Tile.Y * 64) + yOffset - 32)),
                    new Rectangle(395, 497, 3, 8),
                    Color.White, 0f,
                    new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 16f),
                    SpriteEffects.None, 1f);
            }
        }

        private static bool ShouldShowIndicator(CustomBoardTrigger boardTrigger)
        {
            return boardTrigger.ShowIndicator
                && boardTrigger.IsUnlocked()
                && CustomBoard.todayQuests.ContainsKey(boardTrigger.BoardName)
                && CustomBoard.todayQuests[boardTrigger.BoardName] != null
                && CustomBoard.todayQuests[boardTrigger.BoardName].accepted.Value == false;
        }

        public void RefreshBoards()
        {
            foreach (var boardTrigger in this._customBoardTriggers)
            {
                CustomBoard.LoadTodayQuestsIfNecessary(boardTrigger.BoardName);
            }
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            this._currentLocationBoardTriggers = this._customBoardTriggers
                .Where(t => t.LocationName == e.NewLocation.Name)
                .ToArray();
        }

        public bool CheckBoardHere(Point tile)
        {
            var boardTrigger = this._currentLocationBoardTriggers?.FirstOrDefault(t => t.Tile.Equals(tile));

            if (boardTrigger != null && !Game1.eventUp)
            {
                Game1.activeClickableMenu = new CustomBoard(boardTrigger.BoardName);

                return true;
            }

            return false;
        }

        public void RegisterBoardTrigger(CustomBoardTrigger trigger)
        {
            if (QuestFrameworkMod.Instance.Status != State.LAUNCHING)
            {
                throw new InvalidOperationException($"Cannot register new board trigger when in state `{QuestFrameworkMod.Instance.Status}`.");
            }

            this._customBoardTriggers.Add(trigger);
        }

        public void Reset()
        {
            this._customBoardTriggers.Clear();
        }
    }
}
