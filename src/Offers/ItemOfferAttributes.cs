using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Offers
{
    public class ItemOfferAttributes
    {
        /// <summary>
        /// Context tags for item which offer this quest when it's picked by farmer
        /// <example>item_amethyst</example>
        /// </summary>
        public string ItemContextTags { get; set; }

        /// <summary>
        /// A object dialogue message show up after pickup the quest offering item
        /// </summary>
        public string FoundMessage { get; set; }
    }
}
