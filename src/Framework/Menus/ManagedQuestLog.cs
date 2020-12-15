using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PurrplingCore;
using QuestFramework.Extensions;
using QuestFramework.Quests;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuestFramework.Framework.Menus
{
    internal class ManagedQuestLog : QuestLog
    {
        protected internal static IReflectionHelper Reflection => QuestFrameworkMod.Instance.Helper.Reflection;

        private int lastQuestPage = -1;
        private Item rewardItem;

        private List<List<Quest>> Pages => Reflection.GetField<List<List<Quest>>>(this, "pages").GetValue();
        private int QuestPage => Reflection.GetField<int>(this, "questPage").GetValue();
        private int CurrentPage => Reflection.GetField<int>(this, "currentPage").GetValue();

        public Quest CurrentQuest
        {
            get
            {
                if (this.Pages.ElementAtOrDefault(this.CurrentPage) == null || this.Pages[this.CurrentPage].ElementAtOrDefault(this.QuestPage) == null)
                    return null;

                return this.Pages[this.CurrentPage][this.QuestPage];
            }
        }

        public override void update(GameTime time)
        {
            if (this.lastQuestPage != this.QuestPage)
            {
                this.lastQuestPage = this.QuestPage;
                this.UpdateRewardBoxContent();
            }

            base.update(time);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.IsClickedOnRewardBox(x, y))
            {
                switch (this.CurrentQuest.AsManagedQuest().RewardType)
                {
                    case RewardType.Money:
                        Game1.player.Money += this.CurrentQuest.moneyReward.Value;
                        Game1.playSound("purchaseRepeat");
                        break;
                    case RewardType.Item:
                        Game1.playSound("coin");
                        Game1.player.addItemByMenuIfNecessary(this.rewardItem, (ItemGrabMenu.behaviorOnItemSelect)null);
                        this.rewardItem = null;
                        break;
                }

                this.CurrentQuest.moneyReward.Value = 0;
                this.CurrentQuest.destroy.Value = true;

                return;
            }

            base.receiveLeftClick(x, y, playSound);
        }

        private bool IsClickedOnRewardBox(int x, int y)
        {
            return this.QuestPage != -1 
                && this.CurrentQuest.IsManaged()
                && this.CurrentQuest.completed.Value 
                && this.CurrentQuest.moneyReward.Value > 0 
                && this.rewardBox.containsPoint(x, y);
        }

        private void UpdateRewardBoxContent()
        {
            var managedQuest = this.CurrentQuest?.AsManagedQuest();

            if (managedQuest != null && managedQuest.RewardType == RewardType.Item)
            {
                this.rewardItem = new StardewValley.Object(
                    Vector2.Zero, managedQuest.Reward, 
                    managedQuest.RewardAmount > 0 ? managedQuest.RewardAmount : 1);
            } else
            {
                this.rewardItem = null;
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (this.CurrentQuest != null && this.CurrentQuest.IsManaged())
            {
                this.DrawWindow(b);
                this.DrawManagedQuestDetails(b);
                this.DrawCursors(b);

                return;
            }

            base.draw(b);
        }

        private void DrawCursors(SpriteBatch b)
        {
            if (this.upperRightCloseButton != null || this.shouldDrawCloseButton())
            {
                this.upperRightCloseButton.draw(b);
            }

            if (this.QuestPage != -1)
            {
                this.backButton.draw(b);
            }

            Game1.mouseCursorTransparency = 1f;
            this.drawMouse(b);

            var hoverText = Reflection.GetField<string>(this, "hoverText").GetValue();
            if (hoverText.Length > 0)
            {
                drawHoverText(b, hoverText, Game1.dialogueFont);
            }
        }

        private void DrawWindow(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(
                b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11373"),
                this.xPositionOnScreen + (this.width / 2), this.yPositionOnScreen - 64, "", 1f, -1, 0, 0.88f, false);
            drawTextureBox(
                b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), this.xPositionOnScreen, this.yPositionOnScreen,
                this.width, this.height, Color.White, 4f, true);
        }

        private void DrawManagedQuestDetails(SpriteBatch b)
        {
            SpriteText.drawStringHorizontallyCenteredAt(
                b, this.CurrentQuest.questTitle,
                this.xPositionOnScreen
                + (this.width / 2)
                + (!this.CurrentQuest.dailyQuest.Value || this.CurrentQuest.daysLeft.Value <= 0 ? 0 : Math.Max(
                    32, SpriteText.getWidthOfString(this.CurrentQuest.questTitle, 999999) / 3) - 32),
                this.yPositionOnScreen + 32, 999999, -1, 999999, 1f, 0.88f, false, -1, 99999);

            if (this.CurrentQuest.dailyQuest.Value && this.CurrentQuest.daysLeft.Value > 0)
            {
                Utility.drawWithShadow(b, Game1.mouseCursors,
                    new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 48 - 8),
                    new Rectangle(410, 501, 9, 9), Color.White, 0.0f, Vector2.Zero, 4f, false, 0.99f, -1, -1, 0.35f);
                Utility.drawTextWithShadow(
                    b,
                    Game1.parseText(this.CurrentQuest.daysLeft.Value > 1 
                        ? Game1.content.LoadString(@"Strings\StringsFromCSFiles:QuestLog.cs.11374", this.CurrentQuest.daysLeft.Value)
                        : Game1.content.LoadString(@"Strings\StringsFromCSFiles:QuestLog.cs.11375", this.CurrentQuest.daysLeft.Value), 
                    Game1.dialogueFont, this.width - 128),
                    Game1.dialogueFont, new Vector2(this.xPositionOnScreen + 80, this.yPositionOnScreen + 48 - 8),
                    Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
            }
            Utility.drawTextWithShadow(b, Game1.parseText(this.CurrentQuest.questDescription, Game1.dialogueFont, this.width - 128), Game1.dialogueFont, new Vector2(this.xPositionOnScreen + 64, this.yPositionOnScreen + 96), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
            float y = (float)(this.yPositionOnScreen + 96 + (double)Game1.dialogueFont.MeasureString(Game1.parseText(this.CurrentQuest.questDescription, Game1.dialogueFont, this.width - 128)).Y + 32.0);
            if (this.CurrentQuest.completed.Value)
            {
                SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11376"), this.xPositionOnScreen + 32 + 4, this.rewardBox.bounds.Y + 21 + 4, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                this.rewardBox.draw(b);

                if (this.CurrentQuest.moneyReward.Value <= 0)
                {
                    return;
                }

                if (this.CurrentQuest.AsManagedQuest().RewardType == RewardType.Item && this.rewardItem != null)
                {
                    this.rewardItem.drawInMenu(
                        b, new Vector2(this.rewardBox.bounds.X + 18, this.rewardBox.bounds.Y + 18 - Game1.dialogueButtonScale / 6f), 0.9f + this.rewardBox.scale * 0.03f);
                    SpriteText.drawString(b,
                        Utils.CutText(this.rewardItem.DisplayName, 15),
                        this.xPositionOnScreen + 448, this.rewardBox.bounds.Y + 21 + 4, 999999, -1, 999999, 1f, 0.88f,
                        false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                }
                else if (this.CurrentQuest.AsManagedQuest().RewardType == RewardType.Money)
                {
                    b.Draw(Game1.mouseCursors,
                           new Vector2(this.rewardBox.bounds.X + 16, this.rewardBox.bounds.Y + 16 - Game1.dialogueButtonScale / 2f),
                           new Rectangle?(new Rectangle(280, 410, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f,
                           SpriteEffects.None, 1f);
                    SpriteText.drawString(b,
                        Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.CurrentQuest.moneyReward.Value),
                        this.xPositionOnScreen + 448, this.rewardBox.bounds.Y + 21 + 4, 999999, -1, 999999, 1f, 0.88f,
                        false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                }
            }
            else
            {
                Utility.drawWithShadow(
                    b, Game1.mouseCursors,
                    new Vector2(this.xPositionOnScreen + 96 + (float)(8.0 * Game1.dialogueButtonScale / 10.0), y),
                    new Rectangle(412, 495, 5, 4), Color.White, 1.570796f, Vector2.Zero, -1f, false, -1f, -1, -1, 0.35f);
                Utility.drawTextWithShadow(b,
                    Game1.parseText(this.CurrentQuest.currentObjective, Game1.dialogueFont, this.width - 256),
                    Game1.dialogueFont, new Vector2(this.xPositionOnScreen + 128, y - 8f), Color.DarkBlue, 1f, -1f, -1,
                    -1, 1f, 3);

                if (this.CurrentQuest.canBeCancelled.Value)
                {
                    this.cancelQuestButton.draw(b);
                }
            }
        }
    }
}
