﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using PurrplingCore.Patching;
using QuestFramework.Framework;
using QuestFramework.Framework.Controllers;
using QuestFramework.Framework.Hooks;
using StardewValley;
using System;
using xTile.Dimensions;

namespace QuestFramework.Patches
{
    class LocationPatch : Patch<LocationPatch>
    {
        public override string Name => nameof(LocationPatch);

        public ConditionManager ConditionManager { get; }
        public CustomBoardController CustomBoardController { get; }

        public LocationPatch(ConditionManager conditionManager, CustomBoardController customBoardController)
        {
            this.ConditionManager = conditionManager;
            this.CustomBoardController = customBoardController;
            Instance = this;
        }

        protected override void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction)),
                postfix: new HarmonyMethod(typeof(LocationPatch), nameof(LocationPatch.After_performTouchAction))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                prefix: new HarmonyMethod(typeof(LocationPatch), nameof(LocationPatch.Before_checkAction))
            );
        }

        private static bool Before_checkAction(Location tileLocation)
        {
            return !Instance.CustomBoardController.CheckBoardHere(new Point(tileLocation.X, tileLocation.Y));
        }

        private static void After_performTouchAction(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            if (Instance.ConditionManager.Observers["Tile"] is TileHook hookObserver)
            {
                hookObserver.CurrentLocation = __instance.Name;
                hookObserver.Position = playerStandingPosition;
                hookObserver.TouchAction = fullActionString;
                hookObserver.Observe();
            }
        }
    }
}
