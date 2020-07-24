# Changelog

## Upcoming changes

### Common changes

- Added new debug commands: `quests_accept`, `quests_complete` and `quests_remove`.
- Fixed wrong quest type binding in asset editor for `Data/Quests`
- Fixed removing quests marked as destroy from questlog
- Added new conditions: `QuestCompletedDate`, `QuestAcceptedToday`, `QuestCompletedToday`, `QuestNeverAccepted` and `QuestNeverCompleted`

### For Content Pack modders

- Added support for `OR` logical function between values in conditions (with `|` separator)

### For SMAPI modders

- Added new extensions for class `CustomQuest`: `IsNeverAccepted` and `IsNeverCompleted`

## 1.0.0-beta.3

### Common changes

- Fixed problematic issues with Android
- Added Portuguese translation (thx Randy)

## 1.0.0-beta.2

### Common changes

- Improved Bulletinboard quest offer
- Added `not:` prefix for negate result of checked condition (can be used in hooks and offers and in both programatic and content pack usage)
- Added new conditions for check known recipes: `KnownCraftingRecipe` and `KnownCookingRecipe`
- Added new condition `Random` with random chance in % to offer or execute hook
- Added new condition `QuestAcceptedDate` for check if quest was accepted in specified date
- Added new condition `QuestAcceptedInPeriod` for check if quest was accepted in current season, day, year and other time period(s).
- Added stats support
- Improved some minor refactors

### For SMAPI modders

- Added API method `ForceRefresh()`
- Improved accept quest API which allows add quest quietly (without popup alert message and without "new" flag in questlog)
- Added new global quest events: `QuestCompleted`, `QuestAccepted`, `QuestRemoved`, `QuestLogMenuOpen`, `QuestLogMenuClosed` and `Refreshed`.
- Added new managed API: `ExposeGlobalCondition` for expose global condition(s) for using in quest offers or hooks.
- Added extensions for `CustomQuest` class (use namespace `QuestFramework.Extensions` for apply)

## 1.0.0-beta.1

### Common changes

- Fixed saving game
- Added new global conditions: *MinDaysPlayed*, *MaxDaysPlayed*, *DaysPlayed* and *IsFarmerMarried*
- Added support for daily quests (day limited quests)

### For Content Pack modders

- You can define days left for quests (daily quests)
- Added token for use objects from JsonAssets in quest triggers. 
(example for ItemDelivery quest type: `Willy {{ja:Fish Oil}}`)

### For SMAPI modders

- You can define days left for quests (daily quests) via SMAPI mod-provided API
- Added trigger parser-loader in CustomQuest class

## 1.0.0-beta

- First release
