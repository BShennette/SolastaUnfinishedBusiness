﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class CharacterActionBreakFreePatcher
{
    //PATCH: this is almost vanilla code except for the checks on Web and Bound by Ice conditions
    [HarmonyPatch(typeof(CharacterActionBreakFree), nameof(CharacterActionBreakFree.ExecuteImpl))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ExecuteImpl_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(IEnumerator values, CharacterActionBreakFree __instance)
        {
            RulesetCondition restrainingCondition = null;

            __instance.ActingCharacter.RulesetCharacter.EnumerateFeaturesToBrowse<FeatureDefinitionActionAffinity>(
                __instance.ActingCharacter.RulesetCharacter.FeaturesToBrowse);

            foreach (var definitionActionAffinity in __instance.ActingCharacter.RulesetCharacter.FeaturesToBrowse
                         .Cast<FeatureDefinitionActionAffinity>()
                         .Where(definitionActionAffinity => definitionActionAffinity.AuthorizedActions
                             .Contains(__instance.ActionId)))
            {
                restrainingCondition = __instance.ActingCharacter.RulesetCharacter
                    .FindFirstConditionHoldingFeature(definitionActionAffinity);
            }

            if (restrainingCondition == null)
            {
                yield break;
            }


            var actionModifier = new ActionModifier();

            var abilityScoreName =
                __instance.ActionParams.BreakFreeMode == ActionDefinitions.BreakFreeMode.Athletics
                    ? AttributeDefinitions.Strength
                    : AttributeDefinitions.Dexterity;

            var proficiencyName = __instance.ActionParams.BreakFreeMode == ActionDefinitions.BreakFreeMode.Athletics
                ? SkillDefinitions.Athletics
                : SkillDefinitions.Acrobatics;

            var checkDC = 10;
            var sourceGuid = restrainingCondition.SourceGuid;

            switch (restrainingCondition.ConditionDefinition.Name)
            {
                // BEGIN CHANGE
                case "ConditionGrappledRestrainedIceBound":
                    __instance.ActingCharacter.RulesetCharacter.RemoveCondition(restrainingCondition);
                    yield break;
                case "ConditionGrappledRestrainedSpellWeb":
                {
                    if (RulesetEntity.TryGetEntity(sourceGuid, out RulesetCharacterHero rulesetCharacterHero))
                    {
                        checkDC = rulesetCharacterHero.SpellRepertoires
                            .Select(x => x.SaveDC)
                            .Max();
                    }

                    proficiencyName = string.Empty;

                    break;
                }
                // END CHANGE
                default:
                {
                    if (restrainingCondition.HasSaveOverride)
                    {
                        checkDC = restrainingCondition.SaveOverrideDC;
                    }
                    else
                    {
                        if (RulesetEntity.TryGetEntity(sourceGuid, out RulesetEffect entity1))
                        {
                            checkDC = entity1.SaveDC;
                        }
                        else if (RulesetEntity.TryGetEntity(sourceGuid, out RulesetCharacterMonster entity2))
                        {
                            checkDC = 10 + AttributeDefinitions
                                .ComputeAbilityScoreModifier(entity2.GetAttribute(AttributeDefinitions.Strength)
                                    .CurrentValue);
                        }
                    }

                    break;
                }
            }

            __instance.ActingCharacter.RulesetCharacter.ComputeBaseAbilityCheckBonus(
                abilityScoreName, actionModifier.AbilityCheckModifierTrends, proficiencyName);
            __instance.ActingCharacter.ComputeAbilityCheckActionModifier(
                abilityScoreName, proficiencyName, actionModifier);
            __instance.ActingCharacter.RollAbilityCheck(
                abilityScoreName, proficiencyName, checkDC, RuleDefinitions.AdvantageType.None, actionModifier, false,
                -1, out var outcome, out var successDelta, true);
            __instance.AbilityCheckRollOutcome = outcome;
            __instance.AbilityCheckSuccessDelta = successDelta;

            if (__instance.AbilityCheckRollOutcome == RuleDefinitions.RollOutcome.Failure)
            {
                yield return ServiceRepository.GetService<IGameLocationBattleService>()
                    .HandleFailedAbilityCheck(__instance, __instance.ActingCharacter, actionModifier);
            }

            var breakFreeExecuted = __instance.ActingCharacter.RulesetCharacter.BreakFreeExecuted;

            breakFreeExecuted?.Invoke(__instance.ActingCharacter.RulesetCharacter,
                __instance.AbilityCheckRollOutcome == RuleDefinitions.RollOutcome.Success);

            if (__instance.AbilityCheckRollOutcome == RuleDefinitions.RollOutcome.Success)
            {
                __instance.ActingCharacter.RulesetCharacter.RemoveCondition(restrainingCondition);
            }
        }
    }
}
