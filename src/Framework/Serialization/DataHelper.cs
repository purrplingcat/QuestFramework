using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuestFramework.Framework.Serialization.Converters;
using StardewModdingAPI;
using StardewModdingAPI.Toolkit.Serialization;
using System.Collections.Generic;

namespace QuestFramework.Framework.Serialization
{
    class DataHelper : IDataHelper
    {
        public DataHelper(IDataHelper innerHelper)
        {
            var jsonSettings = new JsonHelper().JsonSettings;
            var converters = new List<JsonConverter>()
            {
                new ColorConverter(),
                new PointConverter(),
                new Vector2Converter(),
                new RectangleConverter(),
                new NetFieldConverter(),
            };

            jsonSettings.ContractResolver = new SContractResolver();
            jsonSettings.TypeNameHandling = TypeNameHandling.Auto;
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            converters.ForEach(c => jsonSettings.Converters.Add(c));
            
            this.Serializer = JsonSerializer.CreateDefault(jsonSettings);
            this.InnerHelper = innerHelper;
        }

        private JsonSerializer Serializer { get; }
        private IDataHelper InnerHelper { get; }

        public TModel ReadGlobalData<TModel>(string key) where TModel : class
        {
            return this.FromJToken<TModel>(this.InnerHelper.ReadGlobalData<JToken>(key));
        }

        public TModel ReadJsonFile<TModel>(string path) where TModel : class
        {
            return this.FromJToken<TModel>(this.InnerHelper.ReadJsonFile<JToken>(path));
        }

        public TModel ReadSaveData<TModel>(string key) where TModel : class
        {
            return this.FromJToken<TModel>(this.InnerHelper.ReadSaveData<JToken>(key));
        }

        public void WriteGlobalData<TModel>(string key, TModel data) where TModel : class
        {
            this.InnerHelper.WriteGlobalData(key, this.ToJToken(data));
        }

        public void WriteJsonFile<TModel>(string path, TModel data) where TModel : class
        {
            this.InnerHelper.WriteJsonFile(path, this.ToJToken(data));
        }

        public void WriteSaveData<TModel>(string key, TModel data) where TModel : class
        {
            this.InnerHelper.WriteSaveData(key, this.ToJToken(data));
        }

        private JToken ToJToken<TModel>(TModel data) where TModel : class
        {
            return JToken.FromObject(data, this.Serializer);
        }

        private TModel FromJToken<TModel>(JToken jToken) where TModel : class
        {
            return jToken?.ToObject<TModel>(this.Serializer);
        }
    }
}
