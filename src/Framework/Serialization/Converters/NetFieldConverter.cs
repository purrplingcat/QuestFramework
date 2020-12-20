using Netcode;
using Newtonsoft.Json;
using System;

namespace QuestFramework.Framework.Serialization.Converters
{
    internal class NetFieldConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var check = IsSubclassOfRawGeneric(typeof(NetFieldBase<,>), objectType);
            return check;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var prop = existingValue?.GetType().GetProperty("Value");

            if (prop != null)
            {
                prop.SetValue(existingValue, serializer.Deserialize(reader, prop.PropertyType));
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var prop = value?.GetType().GetProperty("Value");

            if (prop == null)
            {
                writer.WriteNull();
                return;
            }

            serializer.Serialize(writer, prop.GetValue(value));
        }
        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}