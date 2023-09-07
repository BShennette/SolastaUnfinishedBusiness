﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

// supports Sunlit Blade and Resonating Strike
internal sealed class AttackAfterMagicEffect : IAttackAfterMagicEffect
{
    internal const string CantripWeaponAttack = "CantripWeaponAttack";

    private const RuleDefinitions.RollOutcome MinOutcomeToAttack = RuleDefinitions.RollOutcome.Success;
    private const RuleDefinitions.RollOutcome MinSaveOutcomeToAttack = RuleDefinitions.RollOutcome.Failure;

    internal static readonly IAttackAfterMagicEffect BoomingBladeAttack =
        new AttackAfterMagicEffect(2);

    internal static readonly IAttackAfterMagicEffect ResonatingStrikeAttack =
        new AttackAfterMagicEffect(1);

    internal static readonly IAttackAfterMagicEffect SunlitBladeAttack =
        new AttackAfterMagicEffect(2);

    private readonly int _maxAttacks;

    private AttackAfterMagicEffect(int maxAttacks)
    {
        _maxAttacks = maxAttacks;
        CanAttack = CanMeleeAttack;
        CanBeUsedToAttack = DefaultCanUseHandler;
        PerformAttackAfterUse = DefaultAttackHandler;
    }

    public IAttackAfterMagicEffect.CanUseHandler CanBeUsedToAttack { get; }
    public IAttackAfterMagicEffect.GetAttackAfterUseHandler PerformAttackAfterUse { get; }
    public IAttackAfterMagicEffect.CanAttackHandler CanAttack { get; }

    private static bool CanMeleeAttack([NotNull] GameLocationCharacter caster, GameLocationCharacter target)
    {
        var attackMode = caster.FindActionAttackMode(ActionDefinitions.Id.AttackMain);

        if (attackMode == null)
        {
            return false;
        }

        var gameLocationBattleService = ServiceRepository.GetService<IGameLocationBattleService>();

        if (gameLocationBattleService == null)
        {
            return false;
        }

        var attackModifier = new ActionModifier();
        var evalParams = new BattleDefinitions.AttackEvaluationParams();

        evalParams.FillForPhysicalReachAttack(
            caster, caster.LocationPosition, attackMode, target, target.LocationPosition, attackModifier);

        return gameLocationBattleService.CanAttack(evalParams);
    }

    [NotNull]
    private List<CharacterActionParams> DefaultAttackHandler([CanBeNull] CharacterActionMagicEffect effect)
    {
        var attacks = new List<CharacterActionParams>();
        var actionParams = effect?.ActionParams;

        if (actionParams == null)
        {
            return attacks;
        }

        //Spell got countered or it failed
        if (effect.Countered || effect.ExecutionFailed)
        {
            return attacks;
        }

        //Attack outcome is worse that required
        if (effect.AttackRollOutcome > MinOutcomeToAttack)
        {
            return attacks;
        }

        //Target rolled saving throw and got better result
        if (effect.RolledSaveThrow && effect.SaveOutcome < MinSaveOutcomeToAttack)
        {
            return attacks;
        }

        var caster = actionParams.ActingCharacter;
        var targets = actionParams.TargetCharacters;

        if (caster == null || targets.Empty())
        {
            return attacks;
        }

        var attackMode = caster.FindActionAttackMode(ActionDefinitions.Id.AttackMain);

        if (attackMode == null)
        {
            return attacks;
        }

        //get copy to be sure we don't break existing mode
        var rulesetAttackModeCopy = RulesetAttackMode.AttackModesPool.Get();

        rulesetAttackModeCopy.Copy(attackMode);
        attackMode = rulesetAttackModeCopy;

        //set action type to be same as the one used for the magic effect
        attackMode.ActionType = effect.ActionType;

        //PATCH: add tag so it can be identified by War Magic
        attackMode.AddAttackTagAsNeeded(CantripWeaponAttack);

        //PATCH: ensure we flag cantrip used if action switch enabled
        if (Main.Settings.EnableActionSwitching)
        {
            caster.UsedMainCantrip = true;
        }

        var attackModifier = new ActionModifier();

        foreach (var target in targets.Where(t => CanMeleeAttack(caster, t)))
        {
            var attackActionParams =
                new CharacterActionParams(caster, ActionDefinitions.Id.AttackFree) { AttackMode = attackMode };

            attackActionParams.TargetCharacters.Add(target);
            attackActionParams.ActionModifiers.Add(attackModifier);
            attacks.Add(attackActionParams);

            if (attackActionParams.TargetCharacters.Count >= _maxAttacks)
            {
                break;
            }
        }

        return attacks;
    }

    private static bool DefaultCanUseHandler(
        [NotNull] CursorLocationSelectTarget targeting,
        GameLocationCharacter caster,
        GameLocationCharacter target,
        [NotNull] out string failure)
    {
        failure = String.Empty;

        var maxTargets = targeting.maxTargets;
        var remainingTargets = targeting.remainingTargets;
        var selectedTargets = maxTargets - remainingTargets;

        if (selectedTargets > 0)
        {
            return true;
        }

        var canAttack = CanMeleeAttack(caster, target);

        if (!canAttack)
        {
            failure = "Failure/&FailureFlagTargetMeleeWeaponError";
        }

        return canAttack;
    }
}
