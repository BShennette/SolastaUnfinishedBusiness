﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class CharacterActionAttackPatcher
{
    //PATCH: Adds support to IReactToAttackOnEnemyFinished, IReactToMyAttackFinished, IReactToAttackOnMeFinished, IReactToAttackOnMeOrAllyFinished
    [HarmonyPatch(typeof(CharacterActionAttack), nameof(CharacterActionAttack.ExecuteImpl))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ExecuteImpl_Patch
    {
        [UsedImplicitly]
        public static IEnumerator Postfix(
            [NotNull] IEnumerator values,
            [NotNull] CharacterActionAttack __instance)
        {
            var actingCharacter = __instance.ActingCharacter;
            var character = actingCharacter.RulesetCharacter;
            var outcome = RollOutcome.Neutral;
            var found = false;
            GameLocationCharacter defender = null;
            CharacterActionParams actionParams = null;
            RulesetAttackMode mode = null;
            ActionModifier modifier = null;

            // ReSharper disable InconsistentNaming
            void AttackImpactStartHandler(
                GameLocationCharacter _attacker,
                GameLocationCharacter _defender,
                RollOutcome _outcome,
                CharacterActionParams _params,
                RulesetAttackMode _mode,
                ActionModifier _modifier)
            {
                found = true;
                defender = _defender;
                outcome = _outcome;
                actionParams = _params;
                mode = _mode;
                modifier = _modifier;
            }
            // ReSharper enable InconsistentNaming

            actingCharacter.AttackImpactStart += AttackImpactStartHandler;

            while (values.MoveNext())
            {
                yield return values.Current;
            }

            actingCharacter.AttackImpactStart -= AttackImpactStartHandler;

            if (!found || Gui.Battle == null)
            {
                yield break;
            }

            //
            // IReactToMyAttackFinished
            //

            var attackerFeatures = character?.GetSubFeaturesByType<IReactToMyAttackFinished>();

            if (attackerFeatures != null)
            {
                foreach (var feature in attackerFeatures)
                {
                    yield return feature.HandleReactToMyAttackFinished(
                        actingCharacter, defender, outcome, actionParams, mode, modifier);
                }
            }

            //
            // IReactToAttackOnEnemyFinished
            //

            foreach (var gameLocationAlly in Gui.Battle.AllContenders
                         .Where(x =>
                             (x.RulesetCharacter is { IsDeadOrUnconscious: false } &&
                              x.RulesetCharacter.HasAnyConditionOfType(ConditionMindControlledByCaster)) ||
                             x.Side == actingCharacter.Side)
                         .ToList())
            {
                var allyFeatures =
                    gameLocationAlly.RulesetCharacter?.GetSubFeaturesByType<IReactToAttackOnEnemyFinished>();

                if (allyFeatures == null)
                {
                    continue;
                }

                foreach (var feature in allyFeatures)
                {
                    yield return feature.HandleReactToAttackOnEnemyFinished(
                        actingCharacter, gameLocationAlly, defender, outcome, actionParams, mode, modifier);

                    if (Gui.Battle == null)
                    {
                        yield break;
                    }
                }
            }

            //
            // IReactToAttackOnMeFinished
            //
            var defenderFeatures = defender.RulesetCharacter?.GetSubFeaturesByType<IReactToAttackOnMeFinished>();

            if (defenderFeatures != null)
            {
                foreach (var feature in defenderFeatures)
                {
                    yield return feature.HandleReactToAttackOnMeFinished(
                        actingCharacter, defender, outcome, actionParams, mode, modifier);
                }
            }

            //
            // IReactToAttackOnMeOrAllyFinished
            //
            foreach (var gameLocationAlly in Gui.Battle.GetOpposingContenders(actingCharacter.Side).ToList())
            {
                var allyFeatures =
                    gameLocationAlly.RulesetCharacter?.GetSubFeaturesByType<IReactToAttackOnMeOrAllyFinished>();

                if (allyFeatures == null)
                {
                    continue;
                }

                foreach (var feature in allyFeatures)
                {
                    yield return feature.HandleReactToAttackOnAllyFinished(
                        actingCharacter, gameLocationAlly, defender, outcome, actionParams, mode, modifier);
                }
            }
        }
    }
}
