﻿using System.Collections;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.WeaponTypeDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class OathOfDemonHunter : AbstractSubclass
{
    internal const string Name = "OathOfDemonHunter";

    public OathOfDemonHunter()
    {
        //
        // LEVEL 03
        //

        // auto prepared spells

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation(Name, Category.Subclass, "Feature/&DomainSpellsDescription")
            .SetAutoTag("Oath")
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, HuntersMark, ProtectionFromEvilGood),
                BuildSpellGroup(5, MagicWeapon, MistyStep),
                BuildSpellGroup(9, Haste, DispelMagic),
                BuildSpellGroup(13, GuardianOfFaith, GreaterInvisibility),
                BuildSpellGroup(17, HoldMonster, DispelEvilAndGood))
            .SetSpellcastingClass(CharacterClassDefinitions.Paladin)
            .AddToDB();

        // Demon Hunter - Level up UI only

        var featureDemonHunter = FeatureDefinitionBuilder
            .Create($"Feature{Name}DemonHunter")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        // Trial Mark

        var conditionTrialMark = ConditionDefinitionBuilder
            .Create(ConditionMarkedByHunter, $"Condition{Name}TrialMark")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetSpecialDuration(DurationType.Round, 10)
            .SetConditionType(ConditionType.Detrimental)
            .SetPossessive()
            .AddToDB();

        var additionalDamageTrialMark = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}TrialMark")
            .SetGuiPresentationNoContent(true)
            .SetNotificationTag("TrialMark")
            .SetDamageDice(DieType.D6, 1)
            .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 1, 1, 4, 3)
            .SetTargetCondition(conditionTrialMark, AdditionalDamageTriggerCondition.TargetHasConditionCreatedByMe)
            .SetSpecificDamageType(DamageTypeRadiant)
            .AddToDB();

        var powerTrialMark = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}TrialMark")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerTrialMark", Resources.PowerTrialMark, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ChannelDivinity)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1, TurnOccurenceType.EndOfSourceTurn)
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.IndividualsUnique)
                    .SetParticleEffectParameters(LightningBolt)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionTrialMark, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddToDB();

        //
        // LEVEL 07
        //

        // Light Energy Crossbow Bolt

        var lightEnergyCrossbowBolt = FeatureDefinitionBuilder
            .Create($"Feature{Name}LightEnergyCrossbowBolt")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(
                new RangedAttackInMeleeDisadvantageRemover(IsOathOfDemonHunterWeapon),
                new PhysicalAttackFinishedByMeLightEnergyCrossbowBolt(conditionTrialMark, powerTrialMark))
            .AddToDB();

        //
        // LEVEL 15
        //

        // Extra Attack (3)

        // Divine Crossbow

        var attributeModifierDivineCrossbow = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}DivineCrossbow")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(new ModifyCrossbowAttackModeDivineCrossbow())
            .SetModifier(AttributeModifierOperation.Additive,
                AttributeDefinitions.CriticalThreshold, -1)
            .AddToDB();

        //
        // LEVEL 20
        //

        // Demon Slayer

        const string DEMON_SLAYER_NAME = $"FeatureSet{Name}DemonSlayer";

        var dieRollModifierDemonSlayerPhysics = FeatureDefinitionDieRollModifierDamageTypeDependentBuilder
            .Create($"Feature{Name}DemonSlayerPhysics")
            .SetGuiPresentation(DEMON_SLAYER_NAME, Category.Feature, hidden: true)
            .SetModifiers(RollContext.AttackDamageValueRoll, 1, 3, 3,
                $"Feature/&DieRollModifier{Name}DemonSlayer", DamageTypeRadiant)
            .AddToDB();

        var dieRollModifierDemonSlayerMagic = FeatureDefinitionDieRollModifierDamageTypeDependentBuilder
            .Create($"Feature{Name}DemonSlayerMagic")
            .SetGuiPresentation(DEMON_SLAYER_NAME, Category.Feature, hidden: true)
            .SetModifiers(RollContext.MagicDamageValueRoll, 1, 3, 3,
                $"Feature/&DieRollModifier{Name}DemonSlayer", DamageTypeRadiant)
            .AddToDB();

        var featureSetDemonSlayer = FeatureDefinitionFeatureSetBuilder
            .Create(DEMON_SLAYER_NAME)
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                dieRollModifierDemonSlayerPhysics,
                dieRollModifierDemonSlayerMagic)
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.OathOfDemonHunter, 256))
            .AddFeaturesAtLevel(3,
                autoPreparedSpells,
                featureDemonHunter,
                additionalDamageTrialMark,
                powerTrialMark)
            .AddFeaturesAtLevel(7,
                lightEnergyCrossbowBolt)
            .AddFeaturesAtLevel(15,
                CommonBuilders.AttributeModifierThirdExtraAttack,
                attributeModifierDivineCrossbow)
            .AddFeaturesAtLevel(20,
                featureSetDemonSlayer)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Paladin;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice => FeatureDefinitionSubclassChoices
        .SubclassChoicePaladinSacredOaths;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    internal static IsWeaponValidHandler IsOathOfDemonHunterWeapon { get; } =
        ValidatorsWeapon.IsOfWeaponType(LightCrossbowType, HeavyCrossbowType, CustomWeaponsContext.HandXbowWeaponType);

    private sealed class PhysicalAttackFinishedByMeLightEnergyCrossbowBolt : IPhysicalAttackFinishedByMe
    {
        private readonly ConditionDefinition _conditionTrialMark;
        private readonly FeatureDefinitionPower _powerTrialMark;

        public PhysicalAttackFinishedByMeLightEnergyCrossbowBolt(
            ConditionDefinition conditionTrialMark,
            FeatureDefinitionPower powerTrialMark)
        {
            _conditionTrialMark = conditionTrialMark;
            _powerTrialMark = powerTrialMark;
        }

        public IEnumerator OnAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackerAttackMode,
            RollOutcome attackRollOutcome,
            int damageAmount)
        {
            if (attackRollOutcome is RollOutcome.Failure or RollOutcome.CriticalFailure)
            {
                yield break;
            }

            if (!IsOathOfDemonHunterWeapon(attackerAttackMode, null, null))
            {
                yield break;
            }

            if (!battleManager.IsBattleInProgress)
            {
                yield break;
            }

            var actionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (actionService == null)
            {
                yield break;
            }

            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                rulesetDefender.HasConditionOfType(_conditionTrialMark))
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            if (!rulesetAttacker.CanUsePower(_powerTrialMark))
            {
                yield break;
            }

            var usablePower = UsablePowersProvider.Get(_powerTrialMark, rulesetAttacker);
            var reactionParams =
                new CharacterActionParams(attacker, (ActionDefinitions.Id)ExtraActionId.DoNothingFree)
                {
                    StringParameter = "Reaction/&CustomReactionLightEnergyCrossbowBoltDescription",
                    UsablePower = usablePower
                };
            var previousReactionCount = actionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("LightEnergyCrossbowBolt", reactionParams);

            actionService.AddInterruptRequest(reactionRequest);

            yield return battleManager.WaitForReactions(attacker, actionService, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            rulesetAttacker.UsePower(usablePower);
            rulesetDefender.InflictCondition(
                _conditionTrialMark.Name,
                _conditionTrialMark.DurationType,
                _conditionTrialMark.DurationParameter,
                _conditionTrialMark.TurnOccurence,
                AttributeDefinitions.TagCombat,
                rulesetAttacker.Guid,
                rulesetDefender.CurrentFaction.Name,
                1,
                null,
                0,
                0,
                0);
        }
    }

    private sealed class ModifyCrossbowAttackModeDivineCrossbow : IModifyWeaponAttackMode
    {
        public void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode)
        {
            if (!IsOathOfDemonHunterWeapon(attackMode, null, null))
            {
                return;
            }

            attackMode.maxRange += 6;
        }
    }
}
