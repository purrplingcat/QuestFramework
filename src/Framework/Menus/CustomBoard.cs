using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuestFramework.Extensions;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Framework.Menus
{
    class CustomBoard : IClickableMenu
    {
        public static readonly Dictionary<string, Quest> todayQuests;

        public ClickableComponent AcceptQuestButton;
        private readonly Texture2D _billboardTexture;
        private readonly Quest _offeredQuest;
        private readonly string _boardName;

        static CustomBoard()
        {
            todayQuests = new Dictionary<string, Quest>();
        }

        public static void LoadTodayQuestsIfNecessary(string boardName)
        {
            if (string.IsNullOrEmpty(boardName) || todayQuests.ContainsKey(boardName))
                return;

            QuestFrameworkMod.Instance.Monitor.Log($"Refreshing quests for board `{boardName}` ...");
            Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + Game1.dayOfMonth);
            var quests = QuestFrameworkMod.Instance.QuestOfferManager.GetMatchedOffers($"Board:{boardName}").ToList();

            if (quests.Count > 0)
            {
                int which = random.Next(quests.Count);
                string randomQuestName = quests.ElementAt(which < quests.Count ? which : 0).QuestName;
                int questId = QuestFrameworkMod.Instance.QuestManager.ResolveGameQuestId(randomQuestName);

                if (questId != -1)
                {
                    todayQuests.Add(boardName, Quest.getQuestFromId(questId));
                    QuestFrameworkMod.Instance.Monitor.VerboseLog($"  Chosen quest at index {which} of count {quests.Count} ({randomQuestName})");
                }
                else
                {
                    QuestFrameworkMod.Instance.Monitor.VerboseLog($"  Unknown quest `{randomQuestName}`");
                }
            }
            else
            {
                QuestFrameworkMod.Instance.Monitor.VerboseLog($"  No quests available today for board `{boardName}`.");
            }
        }

        public CustomBoard(string boardName, Texture2D boardTexture = null) : base(0, 0, 0, 0, showUpperRightCloseButton: true)
        {
            this._boardName = boardName;
            base.width = 338 * 4;
            base.height = 792;

            Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)center.X;
            base.yPositionOnScreen = (int)center.Y;

            Vector2 acceptTextBounds = Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest"));
            Rectangle acceptButtonBounds = new Rectangle(
                x: base.xPositionOnScreen + base.width / 2 - 128,
                y: base.yPositionOnScreen + base.height - 128,
                width: (int)acceptTextBounds.X + 24,
                height: (int)acceptTextBounds.Y + 24);
            Rectangle closeButtonBounds = new Rectangle(
                x: base.xPositionOnScreen + base.width - 20,
                y: base.yPositionOnScreen,
                width: 48,
                height: 48);

            this.AcceptQuestButton = new ClickableComponent(acceptButtonBounds, "")
            {
                myID = 0,
                visible = false,
            };
            
            base.upperRightCloseButton = new ClickableTextureComponent(
                bounds: closeButtonBounds,
                texture: Game1.mouseCursors,
                sourceRect: new Rectangle(337, 494, 12, 12),
                scale: 4f);

            LoadTodayQuestsIfNecessary(boardName);
            Game1.playSound("bigSelect");

            if (!string.IsNullOrEmpty(boardName) && todayQuests.TryGetValue(boardName, out Quest quest))
            {
                this._offeredQuest = quest;
                this.AcceptQuestButton.visible = !quest.accepted.Value;
            }
            
            this._billboardTexture = boardTexture ?? Game1.content.Load<Texture2D>("LooseSprites\\Billboard");

            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            Game1.activeClickableMenu = new CustomBoard(this._boardName, this._billboardTexture);

        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.AcceptQuestButton.visible && this.AcceptQuestButton.containsPoint(x, y))
            {

                if (this._offeredQuest == null || !this._offeredQuest.IsManaged())
                    return;

                if (Game1.player.hasQuest(this._offeredQuest.id.Value))
                {
                    this.AcceptQuestButton.visible = false;
                    return;
                }

                var managedQuestOfTheDay = this._offeredQuest.AsManagedQuest();

                Game1.playSound("newArtifact");
                this._offeredQuest.accepted.Value = true;
                this._offeredQuest.canBeCancelled.Value = managedQuestOfTheDay.Cancelable;
                this._offeredQuest.dailyQuest.Value = managedQuestOfTheDay.IsDailyQuest();
                this._offeredQuest.daysLeft.Value = managedQuestOfTheDay.DaysLeft;
                this._offeredQuest.dayQuestAccepted.Value = Game1.Date.TotalDays;
                Game1.player.questLog.Add(this._offeredQuest);
                this.AcceptQuestButton.visible = false;
            }

            base.receiveLeftClick(x, y, playSound);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            if (this._offeredQuest != null && !this._offeredQuest.accepted.Value)
            {
                float oldScale = this.AcceptQuestButton.scale;
                this.AcceptQuestButton.scale = (this.AcceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);

                if (this.AcceptQuestButton.scale > oldScale)
                {
                    Game1.playSound("Cowboy_gunshot");
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            bool hide_mouse = false;
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            b.Draw(this._billboardTexture, new Vector2(this.xPositionOnScreen, this.yPositionOnScreen), new Rectangle(0, 0, 338, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            if (Game1.options.SnappyMenus)
            {
                hide_mouse = true;
            }
            if (this._offeredQuest == null || this._offeredQuest.currentObjective == null || this._offeredQuest.currentObjective.Length == 0)
            {
                b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_NothingPosted"), new Vector2(this.xPositionOnScreen + 384, this.yPositionOnScreen + 320), Game1.textColor);
            }
            else
            {
                SpriteFont font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont;
                string description = Game1.parseText(this._offeredQuest.questDescription, font, 640);
                Utility.drawTextWithShadow(b, description, font, new Vector2(this.xPositionOnScreen + 320 + 32, this.yPositionOnScreen + 256), Game1.textColor, 1f, -1f, -1, -1, 0.5f);
                if (this.AcceptQuestButton.visible)
                {
                    hide_mouse = false;
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.AcceptQuestButton.bounds.X, this.AcceptQuestButton.bounds.Y, this.AcceptQuestButton.bounds.Width, this.AcceptQuestButton.bounds.Height, (this.AcceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * this.AcceptQuestButton.scale);
                    Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(this.AcceptQuestButton.bounds.X + 12, this.AcceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
                }
            }

            base.draw(b);

            if (!hide_mouse)
            {
                Game1.mouseCursorTransparency = 1f;
                this.drawMouse(b);
            }
        }
    }
}
