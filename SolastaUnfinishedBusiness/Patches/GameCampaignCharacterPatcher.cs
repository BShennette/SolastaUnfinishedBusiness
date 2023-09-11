﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class GameCampaignCharacterPatcher
{
    [HarmonyPatch(typeof(GameCampaignCharacter), nameof(GameCampaignCharacter.EngageRest))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class EngageRest_Patch
    {
        [UsedImplicitly]
        public static bool Prefix([NotNull] GameCampaignCharacter __instance, RestType restType)
        {
            //PATCH: terminates effects correctly on world travel
            // call `RefreshEffectsForRest` instead of `ApplyRestForConditions` for heroes
            // this makes powers and spells that last until rest properly terminate on rest during world travel
            if (__instance.RulesetCharacter is not RulesetCharacterHero hero)
            {
                return true;
            }

            hero.RefreshEffectsForRest(restType);
            return false;
        }
    }
}
