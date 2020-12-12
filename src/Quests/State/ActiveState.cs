﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuestFramework.Quests.State
{
    public sealed class ActiveState : IDisposable, IResetableState
    {
        private JObject _state;
        private Dictionary<string, ActiveStateField> _activeFields;

        public event Action<JObject> OnChange;
        public event Action<Dictionary<string, ActiveStateField>> OnUpdateFields;
        internal bool WasChanged { get; private set; }

        public ActiveState()
        {
            this._state = new JObject();
            this._activeFields = new Dictionary<string, ActiveStateField>();
        }

        public ActiveState WatchFields(params ActiveStateField[] fields)
        {
            foreach (var field in fields)
            {
                if (field == null)
                    throw new ActiveStateException($"Watched Field can't be null!");

                if (field.Name == null)
                    throw new ActiveStateException("Watched field has no given name. Do you forget add JsonProperty attribute for your active state field declaration?");

                this._activeFields.Add(field.Name, field);

                field.OnChange = (af) => this.UpdateState(field.Name, af.ToJToken());
            }

            return this;
        }

        private void UpdateState(string fieldName, JToken value)
        {
            this._state[fieldName] = value;
            this.WasChanged = true;
            this.OnChange?.Invoke(this._state);
        }

        private void UpdateFields()
        {
            foreach (var field in this._activeFields.Values)
            {
                if (this._state.ContainsKey(field.Name))
                    field.FromJToken(this._state[field.Name]);
                else
                    field.Reset();
            }

            this.OnUpdateFields?.Invoke(this._activeFields);
        }

        public JObject GetState()
        {
            return (JObject)this._state.DeepClone();
        }

        public void SetState(JObject state)
        {
            this._state = state;
            this.UpdateFields();
        }

        public void Dispose()
        {
            foreach (var field in this._activeFields.Values)
            {
                if (field is IDisposable disposableField)
                    disposableField.Dispose();
            }

            this.OnChange = null;
            this._activeFields = null;
            this._state = null;
        }

        public void Reset()
        {
            foreach (var field in this._activeFields.Values)
                field.Reset();
        }

        internal void OnSync()
        {
            this.WasChanged = false;
        }
    }
}
