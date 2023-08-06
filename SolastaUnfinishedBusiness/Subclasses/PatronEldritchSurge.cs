﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static ActionDefinitions;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.CustomBuilders.EldritchVersatility;

namespace SolastaUnfinishedBusiness.Subclasses;

internal class PatronEldritchSurge : AbstractSubclass
{
    public const string Name = "PatronEldritchSurge";

    // LEVEL 01 Blast Exclusive
    private static readonly FeatureDefinitionBonusCantrips BonusCantripBlastExclusive =
        FeatureDefinitionBonusCantripsBuilder
            .Create($"BonusCantrips{Name}BlastExclusive")
            .SetGuiPresentation(Category.Feature)
            .SetBonusCantrips(EldritchBlast)
            .SetCustomSubFeatures(new ModifyEffectDescriptionEldritchBlast())
            .AddToDB();

    // LEVEL 06 Blast Pursuit
    public static readonly FeatureDefinition FeatureBlastPursuit = FeatureDefinitionBuilder
        .Create($"Feature{Name}BlastPursuit")
        .SetGuiPresentation(Category.Feature)
        .SetCustomSubFeatures(new OnTargetReducedToZeroHpBlastPursuit())
        .AddToDB();

    // LEVEL 10 Blast Reload;
    public static readonly FeatureDefinitionPower FeatureBlastReload = FeatureDefinitionPowerBuilder
        .Create($"Power{Name}BlastReload")
        .SetGuiPresentation(Category.Feature)
        .SetUsesFixed(ActivationTime.Permanent)
        .SetEffectDescription(EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
            .SetDurationData(DurationType.Permanent)
            .SetEffectForms(EffectFormBuilder
                .Create()
                .SetConditionForm(
                    BlastReloadSupportRulesetCondition.BindingDefinition,
                    ConditionForm.ConditionOperation.Add)
                .Build())
            .Build())
        .SetCustomSubFeatures(PowerVisibilityModifier.Hidden)
        .AddToDB();

    internal PatronEldritchSurge()
    {
        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.PatronEldritchSurge, 256))
            .AddFeaturesAtLevel(1,
                BonusCantripBlastExclusive,
                Learn2Versatility,
                PowerEldritchVersatilityPointPool)
            .AddFeaturesAtLevel(6,
                FeatureBlastPursuit,
                Learn1Versatility)
            .AddFeaturesAtLevel(10,
                FeatureBlastReload,
                Learn1Versatility)
            .AddFeaturesAtLevel(14,
                PowerBlastOverload,
                Learn1Versatility)
            .AddToDB();
        BuildVersatilities();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Warlock;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceWarlockOtherworldlyPatrons;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    public static bool IsEldritchBlast(RulesetEffect rulesetEffect)
    {
        return rulesetEffect is RulesetEffectSpell rulesetEffectSpell &&
               rulesetEffectSpell.SpellDefinition == EldritchBlast;
    }

    public sealed class ModifyEffectDescriptionEldritchBlast : IModifyEffectDescription
    {
        public bool IsValid(
            BaseDefinition definition,
            RulesetCharacter character,
            EffectDescription effectDescription)
        {
            return definition == EldritchBlast
                   && character.GetOriginalHero() != null
                   && character.GetSubclassLevel(CharacterClassDefinitions.Warlock, Name) > 0;
        }

        public EffectDescription GetEffectDescription(
            BaseDefinition definition,
            EffectDescription effectDescription,
            RulesetCharacter rulesetCharacter,
            RulesetEffect rulesetEffect)
        {
            var rulesetHero = rulesetCharacter.GetOriginalHero();
            if (rulesetHero == null)
            {
                return effectDescription;
            }

            var totalLevel = rulesetHero.classesHistory.Count;
            var warlockClassLevel = rulesetHero.GetClassLevel(CharacterClassDefinitions.Warlock);
            var additionalBeamCount = ComputeAdditionalBeamCount(totalLevel, warlockClassLevel);
            effectDescription.effectAdvancement.Clear();
            effectDescription.targetParameter = 1 + additionalBeamCount;
            return effectDescription;
        }

        public static int ComputeAdditionalBeamCount(int totalLevel, int warlockClassLevel)
        {
            var determinantLevel = warlockClassLevel - (2 * (totalLevel - warlockClassLevel));
            var increaseLevels = new[] { 3, 8, 13, 18 };
            return increaseLevels.Count(level => determinantLevel >= level);
        }
    }

    private sealed class OnTargetReducedToZeroHpBlastPursuit : IOnTargetReducedToZeroHp
    {
        public IEnumerator HandleCharacterReducedToZeroHp(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode, RulesetEffect activeEffect)
        {
            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            if (!IsEldritchBlast(activeEffect) ||
                !rulesetAttacker.GetVersatilitySupportCondition(out var supportCondition))
            {
                yield break;
            }

            supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.EarnPoints,
                supportCondition.BeamNumber);
        }
    }

    private sealed class BlastReloadCustom :
        IActionExecutionHandled, ICharacterTurnStartListener, IQualifySpellToRepertoireLine
    {
        public void OnActionExecutionHandled(
            GameLocationCharacter gameLocationCharacter,
            CharacterActionParams actionParams,
            ActionScope scope)
        {
            // only collect cantrips
            if (scope != ActionScope.Battle ||
                actionParams.activeEffect is not RulesetEffectSpell rulesetEffectSpell ||
                rulesetEffectSpell.SpellDefinition.SpellLevel != 0)
            {
                return;
            }

            var rulesetCharacter = gameLocationCharacter.RulesetCharacter;

            if (rulesetCharacter is not { IsDeadOrDyingOrUnconscious: false })
            {
                return;
            }

            if (!BlastReloadSupportRulesetCondition.GetCustomConditionFromCharacter(rulesetCharacter,
                    out var supportCondition))
            {
                return;
            }

            supportCondition?.CantripsUsedThisTurn.TryAdd(rulesetEffectSpell.SpellDefinition);
        }

        public void OnCharacterTurnStarted(GameLocationCharacter gameLocationCharacter)
        {
            // clean up cantrips list on every turn start
            // combat condition will be removed automatically after combat
            var rulesetCharacter = gameLocationCharacter.RulesetCharacter;

            if (rulesetCharacter is not { IsDeadOrDyingOrUnconscious: false })
            {
                return;
            }

            if (!BlastReloadSupportRulesetCondition.GetCustomConditionFromCharacter(rulesetCharacter,
                    out var supportCondition))
            {
                return;
            }

            supportCondition?.CantripsUsedThisTurn.Clear();
        }

        public void QualifySpells(
            RulesetCharacter rulesetCharacter,
            SpellRepertoireLine spellRepertoireLine,
            IEnumerable<SpellDefinition> spells)
        {
            // _cantripsUsedThisTurn only has entries for Eldritch Surge of at least level 14
            if (spellRepertoireLine.actionType != ActionType.Bonus ||
                !BlastReloadSupportRulesetCondition.GetCustomConditionFromCharacter(rulesetCharacter,
                    out var supportCondition)
               )
            {
                return;
            }

            if (supportCondition != null)
            {
                spellRepertoireLine.relevantSpells.AddRange(
                    supportCondition.CantripsUsedThisTurn.Intersect(spells));
            }
        }
    }

    private class BlastReloadSupportRulesetCondition : RulesetConditionCustom<BlastReloadSupportRulesetCondition>,
        IBindToRulesetConditionCustom
    {
        public readonly List<SpellDefinition> CantripsUsedThisTurn = new();

        static BlastReloadSupportRulesetCondition()
        {
            Category = AttributeDefinitions.TagCombat;
            Marker = new BlastReloadSupportRulesetCondition();
            BindingDefinition = ConditionDefinitionBuilder
                .Create($"Condition{PatronEldritchSurge.Name}BlastReloadSupport")
                .SetGuiPresentationNoContent(true)
                .SetSilent(Silent.WhenAddedOrRemoved)
                .SetCustomSubFeatures(Marker,
                    new BlastReloadCustom()
                )
                .AddToDB();
        }

        public void ReplaceRulesetCondition(RulesetCondition originalRulesetCondition,
            out RulesetCondition replacedRulesetCondition)
        {
            replacedRulesetCondition = GetFromPoolAndCopyOriginalRulesetCondition(originalRulesetCondition);
        }

        public override void SerializeElements(IElementsSerializer serializer, IVersionProvider versionProvider)
        {
            base.SerializeElements(serializer, versionProvider);

            try
            {
                BaseDefinition.SerializeDatabaseReferenceList(
                    serializer, "CantripsUsedThisTurn", "SpellDefinition", CantripsUsedThisTurn);

                if (serializer.Mode == Serializer.SerializationMode.Read)
                {
                    CantripsUsedThisTurn.RemoveAll(x => x is null);
                }
            }
            catch (Exception ex)
            {
                Trace.LogException(
                    new Exception("Error with EldritchSurgeSupportCondition serialization" + ex.Message, ex));
            }
        }

        protected override void ClearCustomStates()
        {
            CantripsUsedThisTurn.Clear();
        }
    }
}
