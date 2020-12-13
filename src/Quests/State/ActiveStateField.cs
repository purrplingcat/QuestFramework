using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Quests.State
{
    public abstract class ActiveStateField
    {
        private string _name;

        public string Name { 
            get => this._name;
            set
            {
                if (this._name != null)
                    throw new InvalidOperationException("This active state field is already named.");

                this._name = value;
            }
        }

        protected abstract void Reset(bool silent);
        public Action<ActiveStateField> OnChange { get; internal set; }
        public abstract JToken ToJToken();
        public abstract void FromJToken(JToken token);
        public void Reset() => this.Reset(silent: false);
        internal void SilentReset() => this.Reset(silent: true);
    }

    public class ActiveStateField<T> : ActiveStateField
    {
        private readonly T _defaultValue;
        private T _value;

        public ActiveStateField()
        {
            this._defaultValue = default;
            this._value = default;
        }

        public ActiveStateField(T initialValue)
        {
            this._defaultValue = initialValue;
            this._value = initialValue;
        }

        public ActiveStateField(T initialValue, string name) : this(initialValue)
        {
            this.Name = name;
        }

        public T Value
        {
            get => this._value;
            set
            {
                this._value = value;
                this.OnChange?.Invoke(this);
            }
        }

        public override JToken ToJToken()
        {
            return JToken.FromObject(this._value);
        }

        protected override void Reset(bool silent)
        {
            this._value = this._defaultValue;

            if (!silent)
            {
                this.OnChange?.Invoke(this);
            }
        }

        public override void FromJToken(JToken token)
        {
            this._value = token.ToObject<T>();
        }
    }
}
