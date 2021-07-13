using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestFramework.Quests
{
    /// <summary>
    /// Quest observer which defines custom quest actions, acting and whatever.
    /// </summary>
    public interface IQuestObserver : IQuestInfoUpdater
    {
        bool CheckIfComplete(IQuestInfo questData, ICompletionArgs completion);
    }
}
