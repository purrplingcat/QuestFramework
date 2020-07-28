using Harmony;
using PurrplingCore.Patching;
using QuestFramework.Framework;
using QuestFramework.Framework.Events;
using QuestFramework.Framework.Quests;
using QuestFramework.Quests;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System;
using System.Collections.Generic;

namespace QuestFramework.Patches
{
    internal class QuestPatch : Patch<QuestPatch>
    {
        public override string Name => nameof(QuestPatch);

        QuestManager QuestManager { get; }
        EventManager EventManager { get; }
        HashSet<Quest> QuestCheckerLock { get; }

        public QuestPatch(QuestManager questManager, EventManager eventManager)
        {
            this.QuestManager = questManager;
            this.EventManager = eventManager;
            this.QuestCheckerLock = new HashSet<Quest>();
            Instance = this;
        }

        private static void After_getQuestFromId(int id, ref Quest __result)
        {
            try
            {
                var customQuest = Instance.QuestManager.GetById(id);

                if (__result != null && customQuest != null)
                {
                    if (customQuest.BaseType == QuestType.Monster && __result is SlayMonsterQuest)
                    {
                        // This is fix for use custom dialogue text for slay monster quest type
                        (__result as SlayMonsterQuest).dialogueparts.Clear();
                    }

                    if (!Game1.player.hasQuest(id))
                        customQuest.Reset();

                    __result.dailyQuest.Value = customQuest.IsDailyQuest();
                    __result.daysLeft.Value = customQuest.DaysLeft;
                    __result.canBeCancelled.Value = customQuest.Cancelable;
                    __result.questType.Value = customQuest.CustomTypeId != -1
                            ? customQuest.CustomTypeId
                            : (int)customQuest.BaseType;
                }
            } catch (Exception e)
            {
                Instance.LogFailure(e, nameof(After_getQuestFromId));
            }
        }

        private static void After_get_currentObjective(Quest __instance, ref string __result)
        {
            try
            {
                var managedQuest = Instance.QuestManager.GetById(__instance.id.Value);

                if (managedQuest is IQuestObserver observer)
                {
                    observer.UpdateObjective(
                        new QuestInfo(__instance, Game1.player), 
                        ref __instance._currentObjective);
                }

                if (__instance._currentObjective == null)
                    __instance._currentObjective = "";

                __result = __instance._currentObjective;
            }
            catch (Exception e)
            {
                Instance.LogFailure(e, nameof(After_get_currentObjective));
            }
        }

        private static bool Before_questComplete(Quest __instance, ref bool __state)
        {
            try
            {
                // save previous completed quest state (used in postfix)
                __state = __instance.completed.Value;
            }
            catch (Exception e)
            {
                Instance.LogFailure(e, nameof(Before_questComplete));
            }

            return true;
        }

        private static void After_questComplete(Quest __instance, bool __state)
        {
            try
            {
                if (!__instance.completed.Value || __instance.completed.Value == __state)
                    return; // Do nothing if completion status wasn't changed or completion status is false

                var managedQuest = Instance.QuestManager.GetById(__instance.id.Value);

                if (managedQuest == null)
                {
                    Instance.EventManager.QuestCompleted.Fire(new Events.QuestEventArgs(__instance), Instance);
                    return; // Only fire global uest completed event if quest is unmanaged
                }

                managedQuest.Complete(new QuestInfo(__instance, Game1.player));

                if (managedQuest.NextQuests?.Count > 0)
                {
                    foreach (var next in managedQuest.NextQuests)
                    {
                        Instance.QuestManager.AcceptQuest(managedQuest.NormalizeName(next));
                    }
                }

                Instance.EventManager.QuestCompleted.Fire(new Events.QuestEventArgs(__instance), Instance);
                Instance.Monitor.Log($"Quest `{managedQuest.GetFullName()}` #{__instance.id.Value} completed!");
                
                if (managedQuest.AddConversationTopicCompleted != null)
                {
                    string[] convTopicCompletedParts = managedQuest.AddConversationTopicCompleted.Split(' ');

                    if (convTopicCompletedParts.Length % 2 == 0 && convTopicCompletedParts.Length >= 2)
                    {
                        for (int i = 0; i < convTopicCompletedParts.Length; i += 2)
                        {
                            string convTopicCompleted = convTopicCompletedParts[i];
                            int daysActive = Convert.ToInt32(convTopicCompletedParts[i + 1]);

                            Game1.player.activeDialogueEvents.Add(convTopicCompleted, daysActive);
                            Instance.Monitor.Log($"Added conversation topic with the key: `{convTopicCompleted}`" +
                                $"the conversation topic will be active for `{convTopicCompletedParts[i+1]}");
                        }
                    }

                }

                if (managedQuest.RemoveConversationTopicCompleted != null)
                {
                    string[] convTopicCompleted = managedQuest.RemoveConversationTopicCompleted.Split(' ');

                    for (int i = 0; i < convTopicCompleted.Length; i += 1)
                    {
                        string convTopic = convTopicCompleted[i];
                        if (Game1.player.activeDialogueEvents.ContainsKey(convTopic) == true)
                        {
                            Game1.player.activeDialogueEvents.Remove(convTopic);
                            Instance.Monitor.Log($"Removed conversation topic: `{convTopic}`");
                        }
                        else Instance.Monitor.Log($"Fail to remove conversation topic, the key already inactive: `{convTopic}`");


                    }

                }

            }
            catch (Exception e)
            {
                Instance.LogFailure(e, nameof(After_questComplete));
            }
        }

        private static void After_get_questDescription(Quest __instance, ref string __result)
        {
            try
            {
                var managedQuest = Instance.QuestManager.GetById(__instance.id.Value);

                if (managedQuest is IQuestObserver observer)
                {
                    observer.UpdateDescription(
                        new QuestInfo(__instance, Game1.player),
                        ref __instance._questDescription);
                }

                if (__instance._questDescription == null)
                    __instance._questDescription = "";

                __result = __instance._questDescription;
            }
            catch (Exception e)
            {
                Instance.LogFailure(e, nameof(After_get_questDescription));
            }
        }

        private static bool Before_checkIfComplete(Quest __instance, ref bool __result, NPC n, int number1, int number2, Item item, string str)
        {
            try
            {
                if (Instance.QuestCheckerLock.Contains(__instance))
                    return true; // Always call only the original method for locked quests

                var managedQuest = Instance.QuestManager.GetById(__instance.id.Value);

                if (managedQuest is IQuestObserver observer)
                {
                    Instance.QuestCheckerLock.Add(__instance);
                    var goingComplete = observer.CheckIfComplete(
                        new QuestInfo(__instance, Game1.player),
                        new CompletionArgs(n, number1, number2, item, str));

                    if (goingComplete && !__instance.completed.Value)
                        __instance.questComplete();

                    __result = goingComplete;
                    Instance.QuestCheckerLock.Remove(__instance);

                    // For observed managed quest we don't call the original method Quest.checkIfComplete
                    return false;
                }

                return true;

            } catch (Exception e)
            {
                Instance.LogFailure(e, nameof(Before_checkIfComplete));
                Instance.QuestCheckerLock.Remove(__instance);

                return true;
            }
        }

        protected override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Quest), nameof(Quest.getQuestFromId)),
                postfix: new HarmonyMethod(typeof(QuestPatch), nameof(QuestPatch.After_getQuestFromId))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Quest), nameof(Quest.questComplete)),
                prefix: new HarmonyMethod(typeof(QuestPatch), nameof(QuestPatch.Before_questComplete)),
                postfix: new HarmonyMethod(typeof(QuestPatch), nameof(QuestPatch.After_questComplete))
            );
            harmony.Patch(
                original: AccessTools.Property(typeof(Quest), nameof(Quest.currentObjective)).GetGetMethod(),
                postfix: new HarmonyMethod(typeof(QuestPatch), nameof(QuestPatch.After_get_currentObjective))
            );
            harmony.Patch(
                original: AccessTools.Property(typeof(Quest), nameof(Quest.questDescription)).GetGetMethod(),
                postfix: new HarmonyMethod(typeof(QuestPatch), nameof(QuestPatch.After_get_questDescription))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Quest), nameof(Quest.checkIfComplete)),
                prefix: new HarmonyMethod(typeof(QuestPatch), nameof(QuestPatch.Before_checkIfComplete))
            );
        }
    }
}
