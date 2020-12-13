﻿using Newtonsoft.Json.Linq;
using QuestFramework.Framework.Stats;
using QuestFramework.Framework.Store;
using QuestFramework.Hooks;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using QuestFramework.Quests.State;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace QuestFramework.Quests
{
    /// <summary>
    /// Custom quest definition
    /// </summary>
    public class CustomQuest
    {
        private int customTypeId;
        private string trigger;
        private string name;

        internal int id = -1;
        
        public event EventHandler<IQuestInfo> Completed;
        public event EventHandler<IQuestInfo> Accepted;
        public event EventHandler<IQuestInfo> Removed;

        [JsonIgnore]
        internal bool NeedsUpdate { get; set; }
        public string OwnedByModUid { get; internal set; }
        public QuestType BaseType { get; set; } = QuestType.Basic;
        public string Title { get; set; }
        public string Description { get; set; }
        public string Objective { get; set; }
        public List<string> NextQuests { get; set; }
        public int Reward { get; set; }
        public string RewardDescription { get; set; }
        public bool Cancelable { get; set; }
        public string ReactionText { get; set; }
        public int DaysLeft { get; set; } = 0;
        public List<Hook> Hooks { get; set; }

        public string Name
        {
            get => this.name;
            set
            {
                if (this.name != null)
                    throw new InvalidOperationException("Quest name can't be changed!");

                this.name = value;
            }
        }

        public string Trigger 
        {
            get => this.trigger;
            set
            {
                if (this is ITriggerLoader triggerLoader)
                {
                    triggerLoader.LoadTrigger(value);
                }

                this.trigger = value;
            }
        }

        public bool IsDailyQuest()
        {
            return this.DaysLeft > 0;
        }

        internal void ConfirmComplete(IQuestInfo questInfo)
        {
            StatsManager.AddCompletedQuest(this.GetFullName());
            this.Completed?.Invoke(this, questInfo);
        }

        internal void ConfirmAccept(IQuestInfo questInfo)
        {
            StatsManager.AddAcceptedQuest(this.GetFullName());
            this.Accepted?.Invoke(this, questInfo);
        }

        internal void ConfirmRemove(IQuestInfo questInfo)
        {
            StatsManager.AddRemovedQuest(this.GetFullName());
            this.Removed?.Invoke(this, questInfo);
        }

        public int CustomTypeId 
        { 
            get => this.BaseType == QuestType.Custom ? this.customTypeId : -1; 
            set => this.customTypeId = value >= 0 ? value : 0; 
        }

        internal protected static IModHelper Helper => QuestFrameworkMod.Instance.Helper;
        internal protected static IMonitor Monitor => QuestFrameworkMod.Instance.Monitor;
        private static StatsManager StatsManager => QuestFrameworkMod.Instance.StatsManager;

        public CustomQuest()
        {
            this.BaseType = QuestType.Custom;
            this.NextQuests = new List<string>();
            this.Hooks = new List<Hook>();
        }

        public CustomQuest(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Update quest when it needs update 
        /// (NeedsUpdate field is set to TRUE)
        /// </summary>
        internal virtual void Update()
        {
        }

        /// <summary>
        /// Reset this managed quest and their state.
        /// Primarily called before accept quest and after remove quest from quest log.
        /// If you override this method, be sure to call <code>base.Reset()</code>.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Get full quest name in format {questName}@{ownerModUniqueId}
        /// </summary>
        public string GetFullName()
        {
            return $"{this.Name}@{this.OwnedByModUid}";
        }

        /// <summary>
        /// Cast this custom quest as statefull custom quest if this quest contains state.
        /// </summary>
        /// <returns>A statefull custom quest or null if this quest not contains or hasn't defined type of state</returns>
        public IStatefull AsStatefull()
        {
            return this as IStatefull;
        }

        /// <summary>
        /// Cast this custom quest as statefull custom quest with specific state type.
        /// </summary>
        /// <typeparam name="TState">Type of state</typeparam>
        /// <returns>A statefull custom quest with specific type of state or null when quest has no state or doesn't match state type.</returns>
        public CustomQuest<TState> AsStatefull<TState>() where TState : class, new()
        {
            return this as CustomQuest<TState>;
        }

        internal string NormalizeName(string name)
        {
            if (name.Contains("@"))
                return name;

            return $"{name}@{this.OwnedByModUid}";
        }
    }
}
