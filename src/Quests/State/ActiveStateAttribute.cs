using System;

namespace QuestFramework.Quests.State
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ActiveStateAttribute : Attribute
    {
        public string Name { get; }

        public ActiveStateAttribute()
        {
        }

        public ActiveStateAttribute(string name)
        {
            this.Name = name;
        }
    }
}