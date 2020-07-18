# Changelog

## Upcoming version

### Common changes

- Added `not:` prefix for negate result of checked condition (can be used in hooks and offers and in both programatic and content pack usage)
- Added new conditions for check if quest was current season: `QuestAcceptedThisYear`, `QuestAcceptedThisSeason`, `QuestAcceptedThisDay` and `QuestAcceptedThisWeekDay`
- Added stats support
- Improved some minor refactors

### For SMAPI modders

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
