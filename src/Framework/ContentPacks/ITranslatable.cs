using StardewModdingAPI;

namespace QuestFramework.Framework.ContentPacks
{
    internal interface ITranslatable<T>
    {
        T Translate(ITranslationHelper translation);
    }
}
