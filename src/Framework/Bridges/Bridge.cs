using QuestFramework.Framework.Bridges.APIs;
using StardewModdingAPI;

namespace QuestFramework.Framework.Bridges
{
    internal class Bridge
    {
        public Bridge(IModRegistry modRegistry)
        {
            this.ModRegistry = modRegistry;
        }

        private IModRegistry ModRegistry { get; }
        public IJsonAssetsApi JsonAssets { get; private set; }
        public IConditionsChecker EPU { get; private set; }

        private TApi LoadApi<TApi>(string modUid) where TApi : class
        {
            return this.ModRegistry.IsLoaded(modUid) 
                ? this.ModRegistry.GetApi<TApi>(modUid) 
                : default;
        }

        public void Init()
        {
            this.JsonAssets = this.LoadApi<IJsonAssetsApi>(ApiIdentifiers.JSON_ASSETS);
            this.EPU = this.LoadApi<IConditionsChecker>(ApiIdentifiers.EPU);

            this.EPU?.Initialize(true, this.ModRegistry.ModID);
        }
    }
}
