﻿using System.Linq;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaCommunityExpansion.CustomDefinitions;
using SolastaCommunityExpansion.Utils;
using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static ConditionOperationDescription;
using static SolastaCommunityExpansion.Classes.Warlock.WarlockSpells;
using static SolastaCommunityExpansion.Models.SpellsContext;
using static SolastaModApi.DatabaseHelper.SpellListDefinitions;

namespace SolastaCommunityExpansion.Spells
{
    internal static class EWSpells
    {
        private static SpellDefinition _sunlightBlade, _greenFlameBlade;
        internal static SpellDefinition SunlightBlade => _sunlightBlade ??= BuildSunlightBlade();
        internal static SpellDefinition GreenFlameBlade => _greenFlameBlade ??= BuildGreenFlameBlade();


        internal static void Register()
        {
            RegisterSpell(SunlightBlade, 0, WarlockSpellList, SpellListWizard, SpellListSorcerer);
            RegisterSpell(GreenFlameBlade, 0, WarlockSpellList, SpellListWizard, SpellListSorcerer);
        }

        private static SpellDefinition BuildSunlightBlade()
        {
            var highlight = new ConditionOperationDescription()
                .SetHasSavingThrow(false)
                .SetOperation(ConditionOperation.Add)
                .SetConditionDefinition(ConditionDefinitionBuilder
                    .Create(DatabaseHelper.ConditionDefinitions.ConditionHighlighted, "EWSunlightBladeHighlighted", DefinitionBuilder.CENamespaceGuid)
                    .SetSpecialInterruptions(RuleDefinitions.ConditionInterruption.Attacked)
                    .SetDuration(RuleDefinitions.DurationType.Round, 1)
                    .SetTurnOccurence(RuleDefinitions.TurnOccurenceType.StartOfTurn)
                    .SetSpecialDuration(true)
                    .AddToDB());
            
            var dimLight = new LightSourceForm()
                .SetBrightRange(0)
                .SetDimAdditionalRange(2)
                .SetLightSourceType(RuleDefinitions.LightSourceType.Basic)
                .SetColor(new Color(0.9f, 0.8f, 0.4f));

            dimLight.SetGraphicsPrefabReference(DatabaseHelper.FeatureDefinitionAdditionalDamages
                .AdditionalDamageBrandingSmite.LightSourceForm.GetField<AssetReference>("graphicsPrefabReference"));
            
            return SpellDefinitionBuilder
                .Create("EWSunlightBlade", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation(Category.Spell,
                    CustomIcons.CreateAssetReferenceSprite("SunlightBlade", Properties.Resources.SunlightBlade, 128, 128))
                .SetSpellLevel(0)
                .SetSchoolOfMagic(DatabaseHelper.SchoolOfMagicDefinitions.SchoolEvocation)
                .SetSomaticComponent(true)
                .SetVerboseComponent(false)
                .SetCustomSubFeatures(
                    PerformAttackAfterMagicEffectUse.MeleeAttack,
                    CustomSpellEffectLevel.ByCasterLevel
                )
                .SetCastingTime(RuleDefinitions.ActivationTime.Action)
                .SetEffectDescription(new EffectDescriptionBuilder()
                    .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.ScorchingRay)
                    .SetTargetingData(
                        RuleDefinitions.Side.Enemy,
                        RuleDefinitions.RangeType.Touch,
                        1,
                        RuleDefinitions.TargetType.Individuals
                    )
                    .SetSavingThrowData(
                        false,
                        false,
                        AttributeDefinitions.Dexterity,
                        true,
                        RuleDefinitions.EffectDifficultyClassComputation.SpellCastingFeature,
                        AttributeDefinitions.Charisma
                    )
                    .SetEffectAdvancement(
                        RuleDefinitions.EffectIncrementMethod.CasterLevelTable,
                        additionalDicePerIncrement: 1,
                        incrementMultiplier: 1
                    )
                    .SetDurationData(RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn)
                    .SetEffectForms(new EffectFormBuilder()
                        .HasSavingThrow(RuleDefinitions.EffectSavingThrowType.None)
                        .SetConditionForm(ConditionDefinitionBuilder
                                .Create("EWSunlightBladeDamage", DefinitionBuilder.CENamespaceGuid)
                                .SetGuiPresentation(Category.Condition)
                                .SetSpecialInterruptions(RuleDefinitions.ConditionInterruption.Attacks)
                                .SetSilent(Silent.WhenAddedOrRemoved)
                                .SetTurnOccurence(RuleDefinitions.TurnOccurenceType.EndOfTurn)
                                .SetDuration(RuleDefinitions.DurationType.Round, 1)
                                .SetFeatures(FeatureDefinitionAdditionalDamageBuilder
                                    .Create("EWSunlightBladeDamageBonus", DefinitionBuilder.CENamespaceGuid)
                                    .Configure(
                                        "SunlightBlade",
                                        RuleDefinitions.FeatureLimitedUsage.None,
                                        RuleDefinitions.AdditionalDamageValueDetermination.Die,
                                        RuleDefinitions.AdditionalDamageTriggerCondition.AlwaysActive,
                                        RuleDefinitions.AdditionalDamageRequiredProperty.MeleeWeapon,
                                        true,
                                        RuleDefinitions.DieType.D8,
                                        1,
                                        RuleDefinitions.AdditionalDamageType.Specific,
                                        RuleDefinitions.DamageTypeRadiant,
                                        RuleDefinitions.AdditionalDamageAdvancement.SlotLevel,
                                        DiceByRankMaker.MakeBySteps(start: 0, step: 5, increment: 1)
                                    )
                                    .SetConditionOperations(highlight)
                                    .SetAddLightSource(true)
                                    .SetLightSourceForm(dimLight)
                                    .AddToDB()
                                )
                                .AddToDB(),
                            ConditionForm.ConditionOperation.Add,
                            true,
                            false
                        )
                        .Build()
                    )
                    .Build()
                )
                .AddToDB();
        }

        private static SpellDefinition BuildGreenFlameBlade()
        {
            var flameHighLevel = new EffectDescriptionBuilder()
                .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.AcidSplash)
                .SetTargetFiltering(RuleDefinitions.TargetFilteringMethod.CharacterOnly)
                .SetTargetingData(RuleDefinitions.Side.Enemy, RuleDefinitions.RangeType.Touch, 1,
                    RuleDefinitions.TargetType.Individuals)
                .SetEffectForms(new EffectFormBuilder()
                    .SetBonusMode(RuleDefinitions.AddBonusMode.AbilityBonus)
                    .SetDamageForm(
                        dieType: RuleDefinitions.DieType.D8,
                        diceNumber: 0,
                        bonusDamage: 0,
                        damageType: RuleDefinitions.DamageTypeFire
                    )
                    .Build()
                )
                .SetEffectAdvancement(
                    effectIncrementMethod: RuleDefinitions.EffectIncrementMethod.PerAdditionalSlotLevel,
                    incrementMultiplier: 5, additionalDicePerIncrement: 1)
                .Build();
            
            var flameLeap = SpellDefinitionBuilder
                .Create("EWGreenFlameBladeFlame", DefinitionBuilder.CENamespaceGuid)
                // .SetGuiPresentation(Category.Spell)
                .SetGuiPresentationNoContent()
                .SetSpellLevel(0)
                .SetSchoolOfMagic(DatabaseHelper.SchoolOfMagicDefinitions.SchoolEvocation)
                .SetSomaticComponent(false)
                .SetVerboseComponent(false)
                .SetCustomSubFeatures(
                    CustomSpellEffectLevel.ByCasterLevel,
                    new UpgradeEffectFromLvel(flameHighLevel, 5)
                )
                .SetCastingTime(RuleDefinitions.ActivationTime.Action)
                .SetEffectDescription(new EffectDescriptionBuilder()
                    .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.AcidSplash)
                    .SetTargetFiltering(RuleDefinitions.TargetFilteringMethod.CharacterOnly)
                    .SetTargetingData(RuleDefinitions.Side.Enemy,RuleDefinitions.RangeType.Touch,1, RuleDefinitions.TargetType.Individuals)
                    .SetEffectForms(new EffectFormBuilder()
                        .SetBonusMode(RuleDefinitions.AddBonusMode.AbilityBonus)
                        .SetDamageForm(
                            dieType: RuleDefinitions.DieType.D1,
                            diceNumber: 0,
                            bonusDamage: 0,
                            damageType: RuleDefinitions.DamageTypeFire
                        )
                        .Build()
                    )
                    .Build()
                )
                .AddToDB();

            
            return SpellDefinitionBuilder
                .Create("EWGreenFlameBlade", DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation(Category.Spell,
                    CustomIcons.CreateAssetReferenceSprite("GreenFlameBlade", Properties.Resources.GreenFlameBlade, 128, 128))//TODO: replace sprite with actual image
                .SetSpellLevel(0)
                .SetSchoolOfMagic(DatabaseHelper.SchoolOfMagicDefinitions.SchoolEvocation)
                .SetSomaticComponent(true)
                .SetVerboseComponent(false)
                .SetCustomSubFeatures(
                    PerformAttackAfterMagicEffectUse.MeleeAttack,
                    CustomSpellEffectLevel.ByCasterLevel,
                    new ChainSpell(flameLeap)
                )
                .SetCastingTime(RuleDefinitions.ActivationTime.Action)
                .SetEffectDescription(new EffectDescriptionBuilder()
                    .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.ScorchingRay)
                    .SetTargetingData(
                        RuleDefinitions.Side.Enemy,
                        RuleDefinitions.RangeType.Touch,
                        1,
                        RuleDefinitions.TargetType.IndividualsUnique,
                        2
                    )
                    .SetTargetProximityData(true, 1)
                    .SetSavingThrowData(
                        false,
                        false,
                        AttributeDefinitions.Dexterity,
                        true,
                        RuleDefinitions.EffectDifficultyClassComputation.SpellCastingFeature,
                        AttributeDefinitions.Charisma
                    )
                    .SetEffectAdvancement(
                        RuleDefinitions.EffectIncrementMethod.CasterLevelTable,
                        additionalDicePerIncrement: 1,
                        incrementMultiplier: 1
                    )
                    .SetDurationData(RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn)
                    .SetEffectForms(new EffectFormBuilder()
                        .HasSavingThrow(RuleDefinitions.EffectSavingThrowType.None)
                        .SetConditionForm(ConditionDefinitionBuilder
                                .Create("EWGreenFlameBladeDamage", DefinitionBuilder.CENamespaceGuid)
                                .SetGuiPresentation(Category.Condition)
                                .SetSpecialInterruptions(RuleDefinitions.ConditionInterruption.Attacks)
                                .SetSilent(Silent.WhenAddedOrRemoved)
                                .SetTurnOccurence(RuleDefinitions.TurnOccurenceType.EndOfTurn)
                                .SetDuration(RuleDefinitions.DurationType.Round, 1)
                                .SetFeatures(FeatureDefinitionAdditionalDamageBuilder
                                    .Create("EWGreenFlameBladeDamageBonus", DefinitionBuilder.CENamespaceGuid)
                                    .Configure(
                                        "GreenFlameBlade",
                                        RuleDefinitions.FeatureLimitedUsage.None,
                                        RuleDefinitions.AdditionalDamageValueDetermination.Die,
                                        RuleDefinitions.AdditionalDamageTriggerCondition.AlwaysActive,
                                        RuleDefinitions.AdditionalDamageRequiredProperty.MeleeWeapon,
                                        true,
                                        RuleDefinitions.DieType.D8,
                                        1,
                                        RuleDefinitions.AdditionalDamageType.Specific,
                                        RuleDefinitions.DamageTypeFire,
                                        RuleDefinitions.AdditionalDamageAdvancement.SlotLevel,
                                        DiceByRankMaker.MakeBySteps(start: 0, step: 5, increment: 1)
                                    )
                                    .AddToDB()
                                )
                                .AddToDB(),
                            ConditionForm.ConditionOperation.Add,
                            true,
                            false
                        )
                        .Build()
                    )
                    .Build()
                )
                .AddToDB();
        }
    }

    internal class ChainSpell : IChainMagicEffect
    {
        public SpellDefinition spell;

        public ChainSpell(SpellDefinition spell)
        {
            this.spell = spell;
        }

        public CharacterActionMagicEffect GetNextMagicEffect(CharacterActionMagicEffect baseEffect, CharacterActionAttack triggeredAttack)
        {
            if (baseEffect == null) { return null; }

            var spellEffect = baseEffect as CharacterActionCastSpell;

            var repertore = spellEffect?.ActiveSpell.SpellRepertoire;

            var actionParams = baseEffect.GetField<CharacterActionParams>("actionParams");
            if (actionParams == null) { return null; }

            if (baseEffect.Countered || baseEffect.GetProperty<bool>("ExecutionFailed"))
            {
                return null;
            }

            var outcome = baseEffect.GetProperty<RuleDefinitions.RollOutcome>("Outcome");
            //if (outcome < minOutcomeToAttack) { return null;}

            //if (effect.SaveOutcome < minSaveOutcomeToAttack) { return null;}

            var caster = actionParams.ActingCharacter;
            var targets = actionParams.TargetCharacters;

            if (caster == null || targets.Count < 2) { return null; }
            
            var rulesetCaster = caster.RulesetCharacter;
            //baseEffect.ActionParams.MemberwiseClone
            var rules = ServiceRepository.GetService<IRulesetImplementationService>();
            var casterLevel = rulesetCaster.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;

            // var spellEff = rules.InstantiateEffectSpell(rulesetCaster, repertore, spell, casterLevel, false);
            //
            // var formParams = new RulesetImplementationDefinitions.ApplyFormsParams();
            // formParams.FillSourceAndTarget(rulesetCaster, targets[1].RulesetCharacter);
            // formParams.FillSpecialParameters(
            //     false,
            //     0,
            //     0,
            //     0,
            //     casterLevel,
            //     actionParams.ActionModifiers[1],
            //     RuleDefinitions.RollOutcome.Neutral,
            //     0,
            //     false,
            //     0,
            //     1,
            //     null
            // );
            // formParams.FillFromActiveEffect(spellEff);
            //
            // rules.ApplyEffectForms(spell.EffectDescription.EffectForms, formParams);
            
            var p = new CharacterActionParams(caster, ActionDefinitions.Id.CastNoCost)
            {
                RulesetEffect = rules.InstantiateEffectSpell(
                    rulesetCaster,
                    repertore,
                    spell,
                    rulesetCaster.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue,
                    false
                ),
                AttackMode = actionParams.AttackMode,
                SpellRepertoire = repertore,
                SkipAnimationsAndVFX = true,
                StringParameter = spell.Name
            };
            
            
            p.TargetCharacters.AddRange(targets.Skip(1));
            p.ActionModifiers.AddRange(actionParams.ActionModifiers.Skip(1));

            var castSpell = new CharacterActionCastSpell(p);
            return castSpell;
        }
    }

    internal class UpgradeEffectFromLvel : ICustomMagicEffectBasedOnCaster
    {
        private EffectDescription upgraded;
        private int level;

        public UpgradeEffectFromLvel(EffectDescription upgraded, int level)
        {
            this.upgraded = upgraded;
            this.level = level;
        }

        public EffectDescription GetCustomEffect(RulesetCharacter caster)
        {
            var casterLevel = caster.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;
            if (casterLevel < level) { return null; }

            return upgraded;

        }
    }
}
