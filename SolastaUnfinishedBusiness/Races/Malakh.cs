﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterRaceDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMoveModes;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSenses;

namespace SolastaUnfinishedBusiness.Races;

internal static class RaceMalakhBuilder
{
    private const string Name = "Malakh";

    internal static CharacterRaceDefinition RaceMalakh { get; } = BuildMalakh();

    [NotNull]
    private static CharacterRaceDefinition BuildMalakh()
    {
        var malakhSpriteReference = Sprites.GetSprite(Name, Resources.Malakh, 1024, 512);
        var featureSetMalakhAbilityScoreIncrease = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}AbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionAttributeModifierBuilder
                    .Create($"AttributeModifier{Name}CharismaAbilityScoreIncrease")
                    .SetGuiPresentationNoContent(true)
                    .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Charisma, 2)
                    .AddToDB(),
                FeatureDefinitionPointPoolBuilder
                    .Create($"PointPool{Name}AbilityScore")
                    .SetGuiPresentationNoContent(true)
                    .SetPool(HeroDefinitions.PointsPoolType.AbilityScore, 1)
                    .RestrictChoices(
                        AttributeDefinitions.Strength,
                        AttributeDefinitions.Dexterity,
                        AttributeDefinitions.Intelligence,
                        AttributeDefinitions.Wisdom,
                        AttributeDefinitions.Constitution)
                    .AddToDB())
            .AddToDB();

        var featureSetMalakhDivineResistance = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}DivineResistance")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionDamageAffinitys.DamageAffinityNecroticResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityRadiantResistance)
            .AddToDB();

        var featureSetMalakhLanguages = FeatureDefinitionFeatureSetBuilder
            .Create(FlexibleRacesContext.FeatureSetLanguageCommonPlusOne, $"FeatureSet{Name}Languages")
            .AddToDB();

        var spellListMalakh = SpellListDefinitionBuilder
            .Create($"SpellList{Name}")
            .SetGuiPresentationNoContent(true)
            .FinalizeSpells()
            .AddToDB();

        // Use instead of bonus cantrip to add spell casting ability
        spellListMalakh.SpellsByLevel[0].Spells = new List<SpellDefinition> { SpellDefinitions.Light };

        var castSpellMalakhMagic = FeatureDefinitionCastSpellBuilder
            .Create(FeatureDefinitionCastSpells.CastSpellTiefling, $"CastSpell{Name}Magic")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetSpellCastingAbility(AttributeDefinitions.Charisma)
            .SetFocusType(EquipmentDefinitions.FocusType.None)
            .SetSlotsPerLevel(FeatureDefinitionCastSpellBuilder.CasterProgression.None)
            .SetSpellKnowledge(SpellKnowledge.FixedList)
            .SetSpellList(spellListMalakh)
            .AddToDB();

        var powerMalakhHealingTouch = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}HealingTouch")
            .SetGuiPresentation(Category.Feature, SpellDefinitions.CureWounds)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Touch, 1, TargetType.IndividualsUnique)
                    .AddEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetHealingForm(
                                HealingComputation.Dice,
                                0,
                                DieType.D1,
                                0,
                                false,
                                HealingCap.MaximumHitPoints)
                            .SetLevelAdvancement(EffectForm.LevelApplianceType.AddBonus, LevelSourceType.CharacterLevel)
                            .Build())
                    .Build())
            .AddToDB();

        var additionalDamageMalakhAngelicForm = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}AngelicForm")
            .SetGuiPresentationNoContent(true)
            .SetNotificationTag("AngelicForm")
            .SetAdditionalDamageType(AdditionalDamageType.Specific)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AlwaysActive)
            .SetSpecificDamageType(DamageTypeRadiant)
            .SetDamageValueDetermination(AdditionalDamageValueDetermination.ProficiencyBonus)
            .SetFrequencyLimit(FeatureLimitedUsage.OnceInMyTurn)
            .AddToDB();

        CreateAngelicFormChoice(BuildAngelicFlight(additionalDamageMalakhAngelicForm));
        CreateAngelicFormChoice(BuildAngelicRadiance(additionalDamageMalakhAngelicForm));
        CreateAngelicFormChoice(BuildAngelicVisage(additionalDamageMalakhAngelicForm));

        var customInvocationPoolAngelicForm =
            CustomInvocationPoolDefinitionBuilder
                .Create($"CustomInvocationPool{Name}AngelicForm")
                .SetGuiPresentation(Category.Feature)
                .Setup(InvocationPoolTypeCustom.Pools.AngelicFormChoice)
                .AddToDB();

        var featureAngelicForm = FeatureDefinitionBuilder
            .Create($"Feature{Name}AngelicForm")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var racePresentation = Human.RacePresentation.DeepCopy();
        // disables the origin image from appearing
        racePresentation.originOptions.RemoveRange(1, racePresentation.originOptions.Count - 1);

        var raceMalakh = CharacterRaceDefinitionBuilder
            .Create(Human, $"Race{Name}")
            .SetGuiPresentation(Category.Race, malakhSpriteReference)
            .SetRacePresentation(racePresentation)
            .SetFeaturesAtLevel(1,
                MoveModeMove6,
                SenseNormalVision,
                SenseDarkvision,
                featureSetMalakhAbilityScoreIncrease,
                featureSetMalakhDivineResistance,
                featureSetMalakhLanguages,
                castSpellMalakhMagic,
                powerMalakhHealingTouch,
                featureAngelicForm)
            .AddFeaturesAtLevel(3, customInvocationPoolAngelicForm)
            .AddToDB();

        return raceMalakh;
    }

    private static void CreateAngelicFormChoice(FeatureDefinition power)
    {
        var name = power.Name.Replace("Power", string.Empty);
        var guiPresentation = power.guiPresentation;

        _ = CustomInvocationDefinitionBuilder
            .Create($"CustomInvocation{name}")
            .SetGuiPresentation(guiPresentation)
            .SetPoolType(InvocationPoolTypeCustom.Pools.AngelicFormChoice)
            .SetGrantedFeature(power)
            .AddCustomSubFeatures(HiddenInvocation.Marker)
            .AddToDB();
    }

    private static FeatureDefinition BuildAngelicVisage(FeatureDefinition additionalDamageMalakhAngelicForm)
    {
        var conditionAngelicVisage = ConditionDefinitionBuilder
            .Create($"Condition{Name}AngelicVisage")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDivineFavor)
            .SetSpecialDuration(DurationType.Minute, 1)
            .SetConditionType(ConditionType.Beneficial)
            .CopyParticleReferences(ConditionDefinitions.ConditionFlyingAdaptive)
            .AddFeatures(additionalDamageMalakhAngelicForm)
            .AddToDB();

        return FeatureDefinitionPowerBuilder
            .Create($"Power{Name}AngelicVisage")
            .SetGuiPresentation(Category.Feature, FeatureDefinitionPowers.PowerDomainOblivionMarkOfFate)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1, TurnOccurenceType.EndOfSourceTurn)
                    .SetTargetingData(Side.Enemy, RangeType.Self, 0, TargetType.Sphere, 2)
                    .SetSavingThrowData(true,
                        AttributeDefinitions.Charisma, true,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                        AttributeDefinitions.Charisma)
                    .SetEffectForms(
                        EffectFormBuilder.Create()
                            .SetConditionForm(ConditionDefinitions.ConditionFrightenedFear,
                                ConditionForm.ConditionOperation.Add)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build(),
                        EffectFormBuilder.Create()
                            .SetConditionForm(conditionAngelicVisage, ConditionForm.ConditionOperation.Add, true, true)
                            .Build())
                    .Build())
            .AddToDB();
    }

    private static FeatureDefinition BuildAngelicFlight(FeatureDefinition additionalDamageMalakhAngelicForm)
    {
        var conditionAngelicFlight = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionFlyingAdaptive, $"Condition{Name}AngelicFlight")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDivineFavor)
            .SetConditionType(ConditionType.Beneficial)
            .AddFeatures(additionalDamageMalakhAngelicForm)
            .AddToDB();

        return FeatureDefinitionPowerBuilder
            .Create($"Power{Name}AngelicFlight")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("FlightSprout", Resources.PowerAngelicFormSprout, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.All, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder.Create()
                            .SetConditionForm(conditionAngelicFlight, ConditionForm.ConditionOperation.Add, true)
                            .Build())
                    .Build())
            .AddToDB();
    }

    private static FeatureDefinition BuildAngelicRadiance(FeatureDefinition additionalDamageMalakhAngelicForm)
    {
        var conditionAngelicRadiance = ConditionDefinitionBuilder
            .Create($"Condition{Name}AngelicRadiance")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDivineFavor)
            .SetConditionType(ConditionType.Beneficial)
            .CopyParticleReferences(ConditionDefinitions.ConditionFlyingAdaptive)
            .AddFeatures(additionalDamageMalakhAngelicForm)
            .AddCustomSubFeatures(new CharacterTurnEndListenerAngelicRadiance())
            .AddToDB();

        var faerieFireLightSource =
            SpellDefinitions.FaerieFire.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.LightSource);
        var powerMalakhAngelicRadiance = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}AngelicRadiance")
            .SetGuiPresentation(Category.Feature, FeatureDefinitionPowers.PowerDomainLawHolyRetribution)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.All, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder.Create()
                            .SetConditionForm(conditionAngelicRadiance, ConditionForm.ConditionOperation.Add, true)
                            .Build(),
                        EffectFormBuilder.Create()
                            .SetLightSourceForm(
                                LightSourceType.Sun, 2, 2,
                                faerieFireLightSource.lightSourceForm.color,
                                faerieFireLightSource.lightSourceForm.graphicsPrefabReference)
                            .Build())
                    .Build())
            .AddToDB();

        return powerMalakhAngelicRadiance;
    }

    private class CharacterTurnEndListenerAngelicRadiance : ICharacterTurnEndListener
    {
        public void OnCharacterTurnEnded(GameLocationCharacter locationCharacter)
        {
            var gameLocationBattleService =
                ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;

            if (gameLocationBattleService == null || gameLocationBattleService is not { IsBattleInProgress: true })
            {
                return;
            }

            var rulesetCharacter = locationCharacter.RulesetCharacter;

            if (rulesetCharacter is not { IsDeadOrDyingOrUnconscious: false })
            {
                return;
            }

            var characterLevel = rulesetCharacter.TryGetAttributeValue(AttributeDefinitions.CharacterLevel);
            var dieType = characterLevel switch
            {
                < 5 => DieType.D4,
                < 9 => DieType.D6,
                < 13 => DieType.D8,
                < 17 => DieType.D10,
                _ => DieType.D12
            };
            var implementationService = ServiceRepository.GetService<IRulesetImplementationService>();

            var damageForm = new DamageForm
            {
                DamageType = DamageTypeRadiant,
                DieType = dieType,
                DiceNumber = 1,
                BonusDamage = 0,
                IgnoreCriticalDoubleDice = true
            };

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var enemy in gameLocationBattleService.Battle.EnemyContenders
                         .Where(enemy => enemy.IsOppositeSide(locationCharacter.Side)
                                         && enemy.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
                         .Where(enemy => gameLocationBattleService.IsWithinXCells(locationCharacter, enemy, 3))
                         .ToList()) // avoid changing enumerator
            {
                var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams
                {
                    sourceCharacter = rulesetCharacter,
                    targetCharacter = enemy.RulesetCharacter,
                    position = enemy.LocationPosition
                };

                implementationService.ApplyEffectForms(
                    new List<EffectForm> { new() { damageForm = damageForm } },
                    applyFormsParams,
                    new List<string> { DamageTypeRadiant },
                    out _,
                    out _);
            }
        }
    }
}
