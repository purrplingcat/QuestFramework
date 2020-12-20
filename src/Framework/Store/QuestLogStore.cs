using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;

namespace QuestFramework.Framework.Store
{
    internal class QuestLogStore
    {
        private readonly IDataHelper dataHelper;
        private readonly IMonitor monitor;
        private List<QuestLogEntry> _questlog;

        public QuestLogStore(IDataHelper dataHelper, IMonitor monitor)
        {
            this.dataHelper = dataHelper;
            this.monitor = monitor;
            this._questlog = new List<QuestLogEntry>();
        }

        public void SetData(IEnumerable<QuestLogEntry> questLog)
        {
            if (questLog == null)
            {
                throw new System.ArgumentNullException(nameof(questLog));
            }

            this._questlog = questLog.ToList();
        }

        public IEnumerable<QuestLogEntry> GetData()
        {
            return this._questlog;
        }

        public void Clear()
        {
            this._questlog.Clear();
        }

        public void Commit(QuestLogEntry entry)
        {
            var existing = this._questlog.FirstOrDefault(e => e.FarmerId == entry.FarmerId && e.FullQuestName == entry.FullQuestName);
            
            if (existing != null)
            {
                existing.Quest = entry.Quest;
                return;
            }

            this._questlog.Add(entry);
        }

        public void Persist()
        {
            if (Context.IsMainPlayer)
            {
                this.dataHelper.WriteSaveData("questLogStore", this._questlog);
                this.monitor.Log("Store data was written to savefile.");
            }
        }

        public void Restore()
        {
            if (Context.IsMainPlayer)
            {
                var data = this.dataHelper.ReadSaveData<List<QuestLogEntry>>("questLogStore");

                if (data == null)
                {
                    this._questlog = new List<QuestLogEntry>();
                    this.monitor.Log("No quests state data to restore from savefile.");
                    return;
                }

                this._questlog = data;
                this.monitor.Log("Quests store data was restored from savefile.");
            }
        }
    }
}