﻿using System.Collections;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class MartialRoyalKnight : AbstractSubclass
{
    private const string Name = "RoyalKnight";
    private const string ConditionInspiringSurge = $"Condition{Name}InspiringSurge";
    private const string ConditionSpiritedSurge = $"Condition{Name}SpiritedSurge";

    public MartialRoyalKnight()
    {
        // LEVEL 03

        var powerRallyingCry = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}RallyingCry")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerRallyingCry", Resources.PowerRallyingCry, 256, 128))
            .SetUsesAbilityBonus(ActivationTime.BonusAction, RechargeRate.ShortRest, AttributeDefinitions.Charisma)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(PowerDomainLifePreserveLife.EffectDescription)
                    .SetTargetFiltering(TargetFilteringMethod.CharacterOnly, TargetFilteringTag.No, 5, DieType.D8)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetHealingForm(
                                HealingComputation.Dice,
                                0,
                                DieType.D1,
                                4,
                                false,
                                HealingCap.MaximumHitPoints,
                                EffectForm.LevelApplianceType.MultiplyBonus)
                            .Build())
                    .Build())
            .SetOverriddenPower(PowerFighterSecondWind)
            .AddToDB();

        // LEVEL 07

        var abilityCheckAffinityRoyalEnvoy = FeatureDefinitionAbilityCheckAffinityBuilder
            .Create($"AbilityCheckAffinity{Name}RoyalEnvoy")
            .SetGuiPresentation($"FeatureSet{Name}RoyalEnvoy", Category.Feature)
            .BuildAndSetAffinityGroups(
                CharacterAbilityCheckAffinity.HalfProficiencyWhenNotProficient,
                DieType.D1,
                0,
                (AttributeDefinitions.Charisma, null))
            .AddToDB();

        var featureSetRoyalEnvoy = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}RoyalEnvoy")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                abilityCheckAffinityRoyalEnvoy,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfSolasta)
            .AddToDB();

        // LEVEL 10

        var conditionInspiringSurge = ConditionDefinitionBuilder
            .Create(ConditionInspiringSurge)
            .SetGuiPresentation($"Power{Name}InspiringSurge", Category.Feature, ConditionDefinitions.ConditionSunbeam)
            .AddFeatures(FeatureDefinitionAdditionalActions.AdditionalActionSurgedMain)
            .AddToDB();

        var powerInspiringSurge = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}InspiringSurge")
            .SetGuiPresentation(Category.Feature, SpellDefinitions.Heroism)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetDurationData(DurationType.Round, 1)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionInspiringSurge, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .SetParticleEffectParameters(PowerFighterActionSurge)
                    .Build())
            .AddToDB();

        powerInspiringSurge.EffectDescription.effectParticleParameters.targetParticleReference =
            SpellDefinitions.Heroism.EffectDescription.effectParticleParameters.conditionStartParticleReference;

        // LEVEL 15

        const string TEXT = "PowerRoyalKnightInspiringProtection";

        var powerRoyalKnightInspiringProtection = FeatureDefinitionPowerBuilder
            .Create("PowerRoyalKnightInspiringProtection")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Reaction, RechargeRate.LongRest, 1, 3)
            .SetReactionContext(ReactionTriggerContext.None)
            .AddToDB();

        var powerRoyalKnightInspiringProtectionAura = FeatureDefinitionPowerBuilder
            .Create("PowerRoyalKnightInspiringProtectionAura")
            .SetGuiPresentation(TEXT, Category.Feature)
            .SetUsesFixed(ActivationTime.PermanentUnlessIncapacitated)
            .SetCustomSubFeatures(PowerVisibilityModifier.Hidden)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Sphere, 12)
                    .SetDurationData(DurationType.Permanent)
                    .SetRecurrentEffect(
                        RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(
                        ConditionDefinitionBuilder
                            .Create("ConditionRoyalKnightInspiringProtectionAura")
                            .SetGuiPresentationNoContent(true)
                            .SetSilent(Silent.WhenAddedOrRemoved)
                            .SetCustomSubFeatures(
                                new TryAlterOutcomeFailedSavingThrowInspiringProtection(
                                    powerRoyalKnightInspiringProtection,
                                    "RoyalKnightInspiringProtection",
                                    "ConditionRoyalKnightInspiringProtectionAura"))
                            .AddToDB()))
                    .Build())
            .AddToDB();

        var featureSetRoyalKnightInspiringProtection = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetRoyalKnightInspiringProtection")
            .SetGuiPresentation(TEXT, Category.Feature)
            .AddFeatureSet(powerRoyalKnightInspiringProtectionAura, powerRoyalKnightInspiringProtection)
            .AddToDB();

        // LEVEL 18

        const string POWER_SPIRITED_SURGE = $"Power{Name}SpiritedSurge";

        var savingThrowAffinitySpiritedSurge = FeatureDefinitionSavingThrowAffinityBuilder
            .Create($"SavingThrowAffinity{Name}SpiritedSurge")
            .SetGuiPresentation(POWER_SPIRITED_SURGE, Category.Feature)
            .SetAffinities(CharacterSavingThrowAffinity.Advantage, false,
                AttributeDefinitions.Strength,
                AttributeDefinitions.Dexterity,
                AttributeDefinitions.Constitution,
                AttributeDefinitions.Intelligence,
                AttributeDefinitions.Wisdom,
                AttributeDefinitions.Charisma)
            .AddToDB();

        var combatAffinitySpiritedSurge = FeatureDefinitionCombatAffinityBuilder
            .Create($"CombatAffinity{Name}SpiritedSurge")
            .SetGuiPresentation(POWER_SPIRITED_SURGE, Category.Feature)
            .SetMyAttackAdvantage(AdvantageType.Advantage)
            .AddToDB();

        var abilityCheckAffinitySpiritedSurge = FeatureDefinitionAbilityCheckAffinityBuilder
            .Create($"AbilityCheckAffinity{Name}SpiritedSurge")
            .SetGuiPresentation(POWER_SPIRITED_SURGE, Category.Feature)
            .BuildAndSetAffinityGroups(CharacterAbilityCheckAffinity.Advantage,
                AttributeDefinitions.Strength,
                AttributeDefinitions.Dexterity,
                AttributeDefinitions.Constitution,
                AttributeDefinitions.Intelligence,
                AttributeDefinitions.Wisdom,
                AttributeDefinitions.Charisma)
            .AddToDB();

        var conditionSpiritedSurge = ConditionDefinitionBuilder
            .Create(ConditionSpiritedSurge)
            .SetGuiPresentation(POWER_SPIRITED_SURGE, Category.Feature, ConditionDefinitions.ConditionSunbeam)
            .SetFeatures(
                FeatureDefinitionAdditionalActions.AdditionalActionSurgedMain,
                abilityCheckAffinitySpiritedSurge,
                combatAffinitySpiritedSurge,
                savingThrowAffinitySpiritedSurge)
            .AddToDB();

        var powerSpiritedSurge = FeatureDefinitionPowerBuilder
            .Create(powerInspiringSurge, POWER_SPIRITED_SURGE)
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetDurationData(DurationType.Round, 1)
                    .SetParticleEffectParameters(PowerFighterActionSurge)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionSpiritedSurge, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .SetOverriddenPower(powerInspiringSurge)
            .AddToDB();

        powerSpiritedSurge.EffectDescription.effectParticleParameters.targetParticleReference =
            SpellDefinitions.Heroism.EffectDescription.effectParticleParameters.conditionStartParticleReference;

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create($"Martial{Name}")
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.MartialRoyalKnight, 256))
            .AddFeaturesAtLevel(3, powerRallyingCry)
            .AddFeaturesAtLevel(7, featureSetRoyalEnvoy)
            .AddFeaturesAtLevel(10, powerInspiringSurge)
            .AddFeaturesAtLevel(15, featureSetRoyalKnightInspiringProtection)
            .AddFeaturesAtLevel(18, powerSpiritedSurge)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Fighter;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private class TryAlterOutcomeFailedSavingThrowInspiringProtection : ITryAlterOutcomeFailedSavingThrow
    {
        internal TryAlterOutcomeFailedSavingThrowInspiringProtection(
            FeatureDefinitionPower power, string reactionName, string auraConditionName)
        {
            Power = power;
            ReactionName = reactionName;
            AuraConditionName = auraConditionName;
        }

        private FeatureDefinitionPower Power { get; }
        private string ReactionName { get; }
        private string AuraConditionName { get; }

        public IEnumerator OnFailedSavingTryAlterOutcome(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier saveModifier,
            bool hasHitVisual,
            bool hasBorrowedLuck)
        {
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            rulesetDefender.TryGetConditionOfCategoryAndType(
                AttributeDefinitions.TagEffect,
                AuraConditionName,
                out var activeCondition);

            if (activeCondition == null)
            {
                yield break;
            }

            RulesetEntity.TryGetEntity<RulesetCharacter>(activeCondition.SourceGuid, out var rulesetOriginalHelper);

            var originalHelper = GameLocationCharacter.GetFromActor(rulesetOriginalHelper);

            if (!ShouldTrigger(defender, originalHelper))
            {
                yield break;
            }

            if (!rulesetOriginalHelper.CanUsePower(Power))
            {
                yield break;
            }

            var usablePower = UsablePowersProvider.Get(Power, rulesetOriginalHelper);
            var rulesService = ServiceRepository.GetService<IRulesetImplementationService>();
            var reactionParams = new CharacterActionParams(originalHelper, ActionDefinitions.Id.SpendPower)
            {
                StringParameter = ReactionName,
                StringParameter2 = FormatReactionDescription(action, attacker, defender, originalHelper),
                RulesetEffect = rulesService
                    .InstantiateEffectPower(rulesetOriginalHelper, usablePower, false)
                    .AddAsActivePowerToSource()
            };
            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var count = actionService.PendingReactionRequestGroups.Count;

            actionService.ReactToSpendPower(reactionParams);

            yield return battleManager.WaitForReactions(originalHelper, actionService, count);

            if (reactionParams.ReactionValidated)
            {
                rulesetOriginalHelper.LogCharacterUsedPower(Power, indent: true);
                rulesetOriginalHelper.UsePower(usablePower);
                action.RolledSaveThrow = TryModifyRoll(action, attacker, defender, saveModifier, hasHitVisual);
            }

            reactionParams.RulesetEffect.Terminate(true);
        }

        private static bool ShouldTrigger(
            GameLocationCharacter defender,
            GameLocationCharacter helper)
        {
            return helper.CanReact()
                   && !defender.IsOppositeSide(helper.Side);
        }

        private static bool TryModifyRoll(
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier saveModifier,
            bool hasHitVisual)
        {
            // ReSharper disable once MergeConditionalExpression
            action.RolledSaveThrow = action.ActionParams.RulesetEffect == null
                ? action.ActionParams.AttackMode.TryRollSavingThrow(attacker.RulesetCharacter, defender.RulesetActor,
                    saveModifier, action.ActionParams.AttackMode.EffectDescription.EffectForms, out var saveOutcome,
                    out var saveOutcomeDelta)
                : action.ActionParams.RulesetEffect.TryRollSavingThrow(attacker.RulesetCharacter, attacker.Side,
                    defender.RulesetActor, saveModifier,
                    action.ActionParams.RulesetEffect.EffectDescription.EffectForms,
                    hasHitVisual, out saveOutcome, out saveOutcomeDelta);

            action.SaveOutcome = saveOutcome;
            action.SaveOutcomeDelta = saveOutcomeDelta;

            return true;
        }

        private static string FormatReactionDescription(
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper)
        {
            var text = defender == helper
                ? "Reaction/&SpendPowerRoyalKnightInspiringProtectionDescriptionSelf"
                : "Reaction/&SpendPowerRoyalKnightInspiringProtectionDescriptionAlly";

            return Gui.Format(text, defender.Name, attacker.Name, action.FormatTitle());
        }
    }
}
