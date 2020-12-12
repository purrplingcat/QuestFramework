using Newtonsoft.Json.Linq;
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

    /// <summary>
    /// Custom quest definition with custom local state
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class CustomQuest<TState> : CustomQuest, IStatefull<TState>, IStateRestorable where TState : class, new()
    {
        /// <summary>
        /// A state data
        /// </summary>
        public TState State { get; private set; }

        /// <summary>
        /// Create custom quest with state (Statefull quest)
        /// </summary>
        public CustomQuest() : base()
        {
            this.State = this.PrepareState();
        }

        /// <summary>
        /// Create custom quest with state (Statefull quest)
        /// </summary>
        /// <param name="name">Name of the quest</param>
        public CustomQuest(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Sync quest state with the store (singleplayer or mainplayer)
        /// or with host (server) via network in multiplayer game.
        /// 
        /// CALL THIS METHOD EVER WHEN YOU CHANGED QUEST STATE!
        /// </summary>
        public void Sync()
        {
            var payload = new StatePayload(
                questName: this.GetFullName(),
                farmerId: Game1.player.UniqueMultiplayerID,
                stateData: this.State is ActiveState activeState 
                    ? activeState.GetState() 
                    : JObject.FromObject(this.State)
             );

            if (!Context.IsMainPlayer)
            {
                Helper.Multiplayer.SendMessage(
                    payload, "SyncState", new[] { QuestFrameworkMod.Instance.ModManifest.UniqueID });
                Monitor.Log($"Payload `{payload.QuestName}/{payload.FarmerId}` type `{payload.StateData.Type}` sent to sync to host.");
            }

            QuestFrameworkMod.Instance.QuestStateStore.Commit(payload);
            (this.State as ActiveState)?.OnSync();
        }

        void IStateRestorable.RestoreState(StatePayload payload)
        {
            if (payload.StateData == null)
            {
                this.ClearState();
                this.State = this.PrepareState();
                return;
            }

            if (this is CustomQuest<ActiveState> activeStateQuest)
            {
                if (activeStateQuest.State == null)
                    activeStateQuest.State = activeStateQuest.PrepareState();

                activeStateQuest.State.SetState(payload.StateData);

                return;
            }

            this.State = payload.StateData.ToObject<TState>();
        }

        private void ClearState()
        {
            if (this.State is IDisposable disposableState)
                disposableState.Dispose();

            this.State = null;
        }

        bool IStateRestorable.VerifyState(StatePayload payload)
        {
            if (this.State is ActiveState activeState)
                return activeState.WasChanged;

            return JToken.DeepEquals(payload.StateData, JObject.FromObject(this.State));
        }

        internal override void Update()
        {
            base.Update();

            if (this.State is ActiveState activeState && activeState.WasChanged)
            {
                this.Sync();
            }
        }

        /// <summary>
        /// Creates and prepares new quest state.
        /// </summary>
        /// <returns></returns>
        protected virtual TState PrepareState()
        {
            var state = new TState();

            if (state is ActiveState activeState)
            {
                activeState.WatchFields(this.GatherActiveStateFields().ToArray());
                activeState.OnChange += this.StateChanged;
            }

            return state;
        }

        private IEnumerable<ActiveStateField> GatherActiveStateFields()
        {
            List<ActiveStateField> activeStateFields = new List<ActiveStateField>();

            foreach (var statePropInfo in this.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<ActiveStateAttribute>() != null))
            {
                var stateProp = (ActiveStateField)statePropInfo.GetValue(this);

                if (stateProp != null && stateProp.Name == null)
                    stateProp.Name = statePropInfo.GetCustomAttribute<ActiveStateAttribute>().Name ?? statePropInfo.Name;

                activeStateFields.Add(stateProp);
            }

            foreach (var stateFieldInfo in this.GetType().GetFields().Where(field => field.GetCustomAttribute<ActiveStateAttribute>() != null))
            {
                var stateField = (ActiveStateField)stateFieldInfo.GetValue(this);

                if (stateField != null && stateField.Name == null)
                    stateField.Name = stateFieldInfo.GetCustomAttribute<ActiveStateAttribute>().Name ?? stateFieldInfo.Name;

                activeStateFields.Add(stateField);
            }

            return activeStateFields;
        }

        private void StateChanged(JObject state)
        {
            this.NeedsUpdate = true;
        }

        /// <inheritdoc cref="CustomQuest.Reset"/>
        public override void Reset()
        {
            base.Reset();

            if (this.State is IResetableState resetableState)
            {
                resetableState.Reset();
                this.Sync();
                return;
            }

            if (this.State is IDisposable disposableState)
                disposableState.Dispose();

            this.State = this.PrepareState();
            this.Sync();
        }

        /// <summary>
        /// Reset quest state to default state data
        /// </summary>
        [Obsolete("Deprecated. Use method CustomQuest.Reset instead")]
        public void ResetState()
        {
            this.Reset();
        }
    }
}
