using Netcode;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace QuestFramework.Framework.Serialization
{
    internal class SContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);

            if (typeof(AbstractNetSerializable).IsAssignableFrom(prop.PropertyType))
            {
                prop.ShouldDeserialize = o => true;
                prop.Writable = true;
            }

            if (ShouldBeIgnored(prop))
            {
                prop.ShouldDeserialize = o => false;
                prop.ShouldSerialize = o => false;
                prop.Ignored = true;
            }

            ResolveName(prop);

            return prop;
        }

        private static void ResolveName(JsonProperty prop)
        {
            var elementAttr = prop.AttributeProvider
                            .GetAttributes(typeof(XmlElementAttribute), true)
                            .OfType<XmlElementAttribute>()
                            .FirstOrDefault(a => !string.IsNullOrEmpty(a.ElementName));

            if (elementAttr != null)
            {
                prop.PropertyName = elementAttr.ElementName;
            }
        }

        private static bool ShouldBeIgnored(JsonProperty prop)
        {
            return prop.AttributeProvider.GetAttributes(typeof(XmlIgnoreAttribute), true).Any()
                || typeof(NetFields).IsAssignableFrom(prop.PropertyType)
                || typeof(NetEvent0).IsAssignableFrom(prop.PropertyType);
        }
    }
}