﻿using QuestFramework.Framework.Serialization;
using QuestFramework.Quests;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Framework.Store
{
    internal class QuestStateStoreData : Dictionary<long, Dictionary<string, StatePayload>> { }
    internal class QuestStateStore
    {
        private QuestStateStoreData _store;
        public IDataHelper Helper { get; }
        public IMonitor Monitor { get; }

        public QuestStateStore(IDataHelper helper, IMonitor monitor)
        {
            this._store = new QuestStateStoreData();
            this.Helper = helper;
            this.Monitor = monitor;
        }

        public void Persist()
        {
            if (Context.IsMainPlayer)
            {
                this.Helper.WriteSaveData("questStateStore", this._store);
                this.Monitor.Log("Store data was written to savefile.");
            }
        }

        public void RestoreData()
        {
            if (Context.IsMainPlayer)
            {
                var data = this.Helper.ReadSaveData<QuestStateStoreData>("questStateStore");

                if (data == null)
                {
                    this.Monitor.Log("No quests state data to restore from savefile.");
                    return;
                }

                this._store = data;
                this.Monitor.Log("Quests store data was restored from savefile.");
            }
        }

        public void Verify(long farmerUid, IEnumerable<CustomQuest> customQuests)
        {
            var payloadList = this.GetPayloadList(farmerUid);
            foreach (var customQuest in customQuests)
            {
                StatePayload payload = payloadList[customQuest.Name];
                if (customQuest is IStateRestorable restorable && !restorable.VerifyState(payload))
                    this.Monitor.Log($"State for quest `{customQuest.Name}` for farmer UID {farmerUid} mismatch! " +
                        $"Did you call Sync() in your CustomQuest type?", LogLevel.Warn);
            }
        }

        public void RestoreData(QuestStateStoreData data)
        {
            this._store = data;
            this.Monitor.Log("Quests store data was restored from given payload.");
        }

        public Dictionary<string, StatePayload> GetPayloadList(long farmerId)
        {
            if (this._store.TryGetValue(farmerId, out var payloadList))
                return payloadList;

            return null;
        }

        internal void Commit(StatePayload payload)
        {
            if (!this._store.ContainsKey(payload.FarmerId))
                this._store.Add(payload.FarmerId, new Dictionary<string, StatePayload>());

            this._store[payload.FarmerId][payload.QuestName] = payload;
            this.Monitor.Log($"Payload `{payload.QuestName}/{payload.FarmerId}` type `{payload.StateData.Type}` commited to store");
        }

        internal void Clean()
        {
            this._store = new QuestStateStoreData();
        }
    }
}
