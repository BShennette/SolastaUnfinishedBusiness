﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Subclasses;
using TA;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAdditionalDamages;
using static ActionDefinitions;
using static SolastaUnfinishedBusiness.Subclasses.PatronEldritchSurge;

namespace SolastaUnfinishedBusiness.CustomBuilders;

internal static class EldritchVersatility
{
    public enum PointAction
    {
        Reserve,
        UnReserve,
        Modify,
        Require
    }

    public enum PointUsage
    {
        EarnPoints,
        EldritchAegis,
        EldritchWard,
        BattlefieldConversionSuccess,
        BattlefieldConversionFailure,
        BlastBreakthroughOrEmpower
    }

    private const string Name = "EldritchVersatility";

    public static readonly FeatureDefinitionPower PowerEldritchVersatilityPointPool = FeatureDefinitionPowerBuilder
        .Create($"Power{Name}PointPool")
        .SetGuiPresentation(Category.Feature)
        .SetUsesFixed(ActivationTime.Permanent)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(
                            VersatilitySupportRulesetCondition.BindingDefinition,
                            ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
        .SetCustomSubFeatures(PowerVisibilityModifier.Hidden)
        .AddToDB();

    public static readonly FeatureDefinitionCustomInvocationPool Learn1Versatility =
        CustomInvocationPoolDefinitionBuilder
            .Create($"Feature{Name}Learn1")
            .SetGuiPresentation(Category.Feature)
            .Setup(InvocationPoolTypeCustom.Pools.EldritchVersatilityPool)
            .AddToDB();

    public static readonly FeatureDefinitionCustomInvocationPool Learn2Versatility =
        CustomInvocationPoolDefinitionBuilder
            .Create($"Feature{Name}Learn2")
            .SetGuiPresentation(Category.Feature)
            .Setup(InvocationPoolTypeCustom.Pools.EldritchVersatilityPool, 2)
            .AddToDB();

    public static readonly FeatDefinition FeatEldritchVersatilityAdept = FeatDefinitionBuilder
        .Create($"Feat{Name}Adept")
        .SetGuiPresentation(Category.Feat)
        .AddFeatures(Learn1Versatility)
        .SetCustomSubFeatures(new EldritchVersatilityAdeptCustom())
        .AddToDB();

    private static readonly ConditionDefinition ConditionEldritchSurgeBlastOverload = ConditionDefinitionBuilder
        .Create($"Condition{PatronEldritchSurge.Name}BlastOverload")
        .SetGuiPresentation(Category.Condition, ConditionRaging)
        .CopyParticleReferences(ConditionDefinitions.ConditionRaging)
        .SetFeatures(
            FeatureDefinitionAdditionalActionBuilder
                .Create($"AdditionalAction{PatronEldritchSurge.Name}BlastOverload")
                .SetGuiPresentationNoContent(true)
                .SetActionType(ActionType.Main)
                .AddToDB())
        .SetCustomSubFeatures(new ConditionBlastOverloadCustom())
        .AddToDB();

    public static readonly FeatureDefinitionPower PowerBlastOverload = FeatureDefinitionPowerBuilder
        .Create($"Power{PatronEldritchSurge.Name}BlastOverload")
        .SetGuiPresentation(Category.Feature, Sprites.GetSprite("BlastOverload", Resources.BlastOverload, 128))
        .SetUsesFixed(ActivationTime.NoCost, RechargeRate.LongRest)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.All, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                .SetParticleEffectParameters(FeatureDefinitionPowers.PowerBarbarianRageStart)
                .SetEffectForms(
                    EffectFormBuilder.ConditionForm(ConditionEldritchSurgeBlastOverload))
                .Build()
        ).AddToDB();

    internal static void BuildVersatilities()
    {
        #region Strength Power

        var name = "BlastEmpower";
        var sprite = Sprites.GetSprite(name, Resources.BlastEmpower, 128);
        FeatureDefinition featureOrPower = FeatureDefinitionAdditionalDamageBuilder
            .Create(AdditionalDamageInvocationAgonizingBlast,
                $"Feature{Name}{name}")
            .SetNotificationTag("BlastEmpower")
            .SetDamageValueDetermination(ExtraAdditionalDamageValueDetermination.AbilityScoreModifier)
            .SetCustomSubFeatures(
                new AbilityScoreNameProvider(() => AttributeDefinitions.Strength),
                new BlastEmpowerCustom($"Invocation{Name}{name}"))
            .AddToDB();
        BuildFeatureInvocation(name, sprite, AttributeDefinitions.Strength, featureOrPower);

        name = "BlastBreakthrough";
        sprite = Sprites.GetSprite(name, Resources.BlastBreakthrough, 128);
        featureOrPower = FeatureDefinitionBuilder
            .Create($"Feature{Name}{name}")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(
                new BlastBreakthroughCustom($"Invocation{Name}{name}"))
            .AddToDB();
        BuildFeatureInvocation(name, sprite, AttributeDefinitions.Strength, featureOrPower);

        #endregion

        #region Intelligence Power

        name = "BattlefieldShorthand";
        sprite = Sprites.GetSprite(name, Resources.BattlefieldShorthand, 128);
        featureOrPower = FeatureDefinitionBuilder
            .Create($"Feature{Name}{name}")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(
                new BattlefieldShorthandCopySpells())
            .AddToDB();
        BuildFeatureInvocation(name, sprite, AttributeDefinitions.Intelligence, featureOrPower);

        name = "BattlefieldConversion";
        sprite = Sprites.GetSprite(name, Resources.BattlefieldConversion, 128);
        featureOrPower = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}{name}")
            .SetGuiPresentation(Category.Feature)
            .SetEffectDescription(
                EffectDescriptionBuilder.Create()
                    .SetParticleEffectParameters(SpellDefinitions.Haste)
                    .Build())
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetCustomSubFeatures(
                PowerFromInvocation.Marker
            )
            .AddToDB();
        featureOrPower.AddCustomSubFeatures(new BattlefieldConversionRestoreSlot(featureOrPower));
        BuildFeatureInvocation(name, sprite, AttributeDefinitions.Intelligence, featureOrPower);

        #endregion

        #region Wisdom Power

        name = "EldritchAegis";
        sprite = Sprites.GetSprite(name, Resources.EldritchAegis, 128);
        featureOrPower = FeatureDefinitionBuilder
            .Create($"Feature{Name}{name}")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(
                new EldritchAegisTwistHit())
            .AddToDB();
        BuildFeatureInvocation(name, sprite, AttributeDefinitions.Wisdom, featureOrPower);

        name = "EldritchWard";
        sprite = Sprites.GetSprite(name, Resources.EldritchWard, 128);
        featureOrPower = FeatureDefinitionBuilder
            .Create($"Feature{Name}{name}")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(
                new EldritchWardAidSave())
            .AddToDB();
        BuildFeatureInvocation(name, sprite, AttributeDefinitions.Wisdom, featureOrPower);

        #endregion
    }

    public static bool GetVersatilitySupportCondition(
        this RulesetCharacter rulesetCharacter,
        out VersatilitySupportRulesetCondition supportCondition)
    {
        return VersatilitySupportRulesetCondition.GetCustomConditionFromCharacter(
            rulesetCharacter, out supportCondition);
    }

    private static void BuildFeatureInvocation(
        string name,
        AssetReferenceSprite sprite,
        string abilityScore,
        FeatureDefinition feature)
    {
        _ = CustomInvocationDefinitionBuilder
            .Create($"Invocation{Name}{name}")
            .SetGuiPresentation(name, Category.Feature, sprite)
            .SetPoolType(InvocationPoolTypeCustom.Pools.EldritchVersatilityPool)
            .SetGrantedFeature(feature)
            .SetRequirements(0)
            .AddToDB();

        feature.AddCustomSubFeatures(new AddAbilityScoreBonus(abilityScore));
    }

    private static int GetAbilityScoreModifier(RulesetEntity ownerCharacter, string abilityScore)
    {
        return AttributeDefinitions.ComputeAbilityScoreModifier(ownerCharacter.TryGetAttributeValue(abilityScore));
    }

    private static void RequestCustomReaction(
        IGameLocationActionService actionService, string type, CharacterActionParams actionParams, int requestPoints)
    {
        var reactionRequest = new ReactionRequestCustom(type, actionParams)
        {
            Resource = new ReactionResourceEldritchVersatilityPoints(requestPoints)
        };

        (actionService as GameLocationActionManager)?.AddInterruptRequest(reactionRequest);
    }

    private static void InflictCondition(
        BaseDefinition condition,
        RulesetCharacter sourceCharacter,
        RulesetActor targetCharacter)
    {
        targetCharacter.InflictCondition(
            condition.Name,
            DurationType.Round,
            1,
            TurnOccurenceType.StartOfTurn,
            AttributeDefinitions.TagCombat,
            sourceCharacter.guid,
            sourceCharacter.CurrentFaction.Name,
            1,
            null,
            0,
            0,
            0);
    }

    private static bool IsInvocationActive(
        RulesetCharacter featureOwner, string invocationName, out RulesetInvocation rulesetInvocation)
    {
        foreach (var invocation in featureOwner.GetOriginalHero()!.Invocations.Where(invocation =>
                     invocation.Active && invocation.invocationDefinition.Name == invocationName))
        {
            rulesetInvocation = invocation;

            return true;
        }

        rulesetInvocation = null;

        return false;
    }

    private static void ApplyMyFeatures(RulesetCharacterHero characterHero, params FeatureDefinition[] definitionsToAdd)
    {
        characterHero.ActiveFeatures.TryAdd(Name, new List<FeatureDefinition>());

        foreach (var feature in definitionsToAdd)
        {
            characterHero.ActiveFeatures[Name].TryAdd(feature);
        }
    }

    private static void RemoveMyFeatures(
        RulesetCharacterHero characterHero, params FeatureDefinition[] definitionsToRemove)
    {
        if (characterHero.ActiveFeatures.TryGetValue(Name, out var features))
        {
            features.RemoveAll(definitionsToRemove.Contains);
        }
    }

    private static void TurnOffPointReservingPower(RulesetCharacter target,
        VersatilitySupportRulesetCondition supportCondition)
    {
        var hero = target.GetOriginalHero();
        var features = hero.GetSubFeaturesByType<ToggleableInvocation>();

        foreach (var feature in features)
        {
            if (hero != null)
            {
                var invocation = hero.Invocations.Find(y =>
                    y.InvocationDefinition.Name == feature.InvocationName);

                if (!invocation.Active)
                {
                    continue;
                }

                invocation.Toggle();
                feature.HandleToggle(hero, supportCondition, invocation, true);
            }

            target.RefreshAll();
        }
    }

    internal class VersatilitySupportRulesetCondition :
        RulesetConditionCustom<VersatilitySupportRulesetCondition>, IBindToRulesetConditionCustom
    {
        static VersatilitySupportRulesetCondition()
        {
            Category = "EldritchVersatility";
            Marker = new VersatilitySupportRulesetCondition();
            BindingDefinition = ConditionDefinitionBuilder
                .Create("ConditionEldritchVersatility")
                .SetGuiPresentationNoContent(true)
                .SetSpecialInterruptions(ConditionInterruption.LevelUp, ConditionInterruption.LongRest)
                .SetCustomSubFeatures(
                    Marker,
                    new VersatilitySupportConditionCustom())
                .SetSilent(Silent.WhenAddedOrRemoved)
                .AddToDB();
        }

        public List<SpellDefinition> CopiedSpells { get; } = new();
        public int CurrentPoints { get; private set; }
        public bool IsValidBlastBreakthrough { get; set; }
        public int PointSpentOnAddingAC { get; private set; }
        public int CreateSlotDC { get; private set; }
        public int MaxPoints { get; private set; }
        public int ReservedPoints { get; private set; }
        private int EarnedPointsInThisTurn { get; set; }
        private int SlotLevel { get; set; }
        public int BeamNumber { get; private set; }
        public bool IsOverload { get; set; }
        private bool HasBlastPursuit { get; set; }

        public void ReplaceRulesetCondition(
            RulesetCondition originalRulesetCondition, out RulesetCondition replacedRulesetCondition)
        {
            replacedRulesetCondition = GetFromPoolAndCopyOriginalRulesetCondition(originalRulesetCondition);
        }

        public bool TryEarnOrSpendPoints(PointAction action, PointUsage usage = PointUsage.EarnPoints, int value = 0)
        {
            if (value < 0)
            {
                return false;
            }

            var modifyCurrent = 0;
            var modifyReserve = 0;

            switch (action, usage)
            {
                case (_, PointUsage.EarnPoints):
                    modifyCurrent = Math.Min(value, MaxPoints - CurrentPoints);
                    modifyCurrent = Math.Min(modifyCurrent, MaxPoints - EarnedPointsInThisTurn);
                    break;
                case (_, PointUsage.EldritchAegis):
                case (_, PointUsage.EldritchWard):
                    modifyCurrent = -value;
                    break;
                case (_, PointUsage.BattlefieldConversionSuccess):
                    modifyCurrent = -Math.Min(2 * SlotLevel, MaxPoints);
                    break;
                case (_, PointUsage.BattlefieldConversionFailure):
                    modifyCurrent = -SlotLevel;
                    break;
                case (PointAction.Reserve, PointUsage.BlastBreakthroughOrEmpower):
                    modifyReserve = BeamNumber;
                    break;
                case (PointAction.UnReserve, PointUsage.BlastBreakthroughOrEmpower):
                    modifyReserve = -BeamNumber;
                    break;
                case (PointAction.Modify, PointUsage.BlastBreakthroughOrEmpower):
                    modifyReserve = -BeamNumber;
                    modifyCurrent = -BeamNumber;
                    break;
            }

            if (IsOverload)
            {
                modifyCurrent = Math.Max(0, modifyCurrent);
            }

            switch (action)
            {
                case PointAction.Require:
                    return CurrentPoints + modifyCurrent >= 0;
                case PointAction.Reserve:
                    if (!IsOverload && CurrentPoints + modifyCurrent - ReservedPoints - modifyReserve < 0)
                    {
                        return false;
                    }

                    ReservedPoints += modifyReserve;

                    return true;

                case PointAction.Modify:
                    if (CurrentPoints + modifyCurrent < 0)
                    {
                        return false;
                    }

                    if (!IsOverload && CurrentPoints + modifyCurrent - ReservedPoints - modifyReserve < 0)
                    {
                        TurnOffPointReservingPower(GetEntity<RulesetCharacter>(SourceGuid), this);
                    }

                    if (usage == PointUsage.EldritchAegis)
                    {
                        PointSpentOnAddingAC += value;
                    }

                    ReservedPoints += modifyReserve;
                    CurrentPoints += modifyCurrent;

                    return true;
                case PointAction.UnReserve:
                    // To prevent possible bugs
                    ReservedPoints = Math.Max(modifyReserve + ReservedPoints, 0);

                    return true;
                default:
                    return false;
            }
        }

        private void InitSupportCondition([NotNull] RulesetCharacter ownerCharacter)
        {
            var ownerHero = ownerCharacter.GetOriginalHero();

            MaxPoints = ownerHero.GetClassLevel(CharacterClassDefinitions.Warlock);

            if (ownerHero != null)
            {
                var classLevel = ownerHero.ClassesHistory.Count;

                if (ownerHero.HasSubFeatureOfType<ModifyEffectDescriptionEldritchBlast>())
                {
                    BeamNumber = 1 + ModifyEffectDescriptionEldritchBlast
                        .ComputeAdditionalBeamCount(classLevel, MaxPoints);
                }
                else
                {
                    BeamNumber = new[] { 1, 5, 11, 17 }.Count(x => classLevel >= x);
                }
            }

            if (ownerHero != null)
            {
                var proficiencyBonus = ownerHero.GetAttribute(AttributeDefinitions.ProficiencyBonus).CurrentValue;

                if (ownerHero.TrainedFeats.Contains(FeatEldritchVersatilityAdept))
                {
                    MaxPoints += proficiencyBonus;
                }

                CurrentPoints = 0;
                ReservedPoints = 0;
                SlotLevel = SharedSpellsContext.GetWarlockSpellLevel(ownerHero);
                // Reset DC
                CreateSlotDC = 8 + proficiencyBonus;
            }

            EarnedPointsInThisTurn = 0;
            PointSpentOnAddingAC = 0;
            IsValidBlastBreakthrough = false;
            IsOverload = false;
            HasBlastPursuit = ownerHero.HasAnyFeature(FeatureBlastReload);
            CopiedSpells.Clear();
        }

        private void ClearEarnedPointsInThisTurn()
        {
            EarnedPointsInThisTurn = 0;
        }

        public void ClearPointSpentOnAddingAC()
        {
            PointSpentOnAddingAC = 0;
        }

        public void ModifySlotDC(bool createSuccess, RulesetCharacterHero characterHero)
        {
            if (createSuccess)
            {
                CreateSlotDC += SlotLevel;
            }
            else
            {
                CreateSlotDC = Math.Max(
                    CreateSlotDC - SlotLevel,
                    8 + characterHero.GetAttribute(AttributeDefinitions.ProficiencyBonus).CurrentValue);
            }
        }

        [UsedImplicitly]
        public override void SerializeAttributes(IAttributesSerializer serializer, IVersionProvider versionProvider)
        {
            base.SerializeAttributes(serializer, versionProvider);
            try
            {
                MaxPoints = serializer.SerializeAttribute("MaxPoints", MaxPoints);
                CurrentPoints = serializer.SerializeAttribute("CurrentPoints", CurrentPoints);
                ReservedPoints = serializer.SerializeAttribute("ReservedPoints", ReservedPoints);
                EarnedPointsInThisTurn =
                    serializer.SerializeAttribute("EarnedPointsInThisTurn", EarnedPointsInThisTurn);
                CreateSlotDC = serializer.SerializeAttribute("CreateSlotDC", CreateSlotDC);
                SlotLevel = serializer.SerializeAttribute("SlotLevel", SlotLevel);
                BeamNumber = serializer.SerializeAttribute("BeamNumber", BeamNumber);
                PointSpentOnAddingAC = serializer.SerializeAttribute("PointSpentOnAddingAC", PointSpentOnAddingAC);
                IsValidBlastBreakthrough =
                    serializer.SerializeAttribute("IsValidBlastBreakthrough", IsValidBlastBreakthrough);
                IsOverload = serializer.SerializeAttribute("IsOverload", IsOverload);
                HasBlastPursuit = serializer.SerializeAttribute("HasBlastPursuit", HasBlastPursuit);
            }
            catch (Exception ex)
            {
                Trace.LogException(new Exception(
                    "[TACTICAL INVISIBLE FOR PLAYERS] error with VersatilityPointsRulesetCondition serialization (may be caused by mods or bad versioning implementation) " +
                    ex.Message, ex));

                Gui.GuiService.ShowMessage(MessageModal.Severity.Serious3, "Message/&CorruptedCharacterErrorTitle",
                    "Message/&CorruptedCharacterErrorDescription", "Message/&MessageOkTitle", null, null, null);
            }
        }

        [UsedImplicitly]
        public override void SerializeElements(IElementsSerializer serializer, IVersionProvider versionProvider)
        {
            base.SerializeElements(serializer, versionProvider);
            try
            {
                BaseDefinition.SerializeDatabaseReferenceList(
                    serializer, "CopiedSpells", "SpellDefinition", CopiedSpells);
                if (serializer.Mode == Serializer.SerializationMode.Read)
                {
                    CopiedSpells.RemoveAll(x => x is null);
                }
            }
            catch (Exception ex)
            {
                Trace.LogException(new Exception(
                    "[TACTICAL INVISIBLE FOR PLAYERS] error with VersatilityPointsRulesetCondition serialization (may be caused by mods or bad versioning implementation) " +
                    ex.Message, ex));
                Gui.GuiService.ShowMessage(MessageModal.Severity.Serious3, "Message/&CorruptedCharacterErrorTitle",
                    "Message/&CorruptedCharacterErrorDescription", "Message/&MessageOkTitle", null, null, null);
            }
        }

        protected override void ClearCustomStates()
        {
            CopiedSpells.Clear();
            MaxPoints = 0;
            CurrentPoints = 0;
            ReservedPoints = 0;
            CreateSlotDC = 0;
            EarnedPointsInThisTurn = 0;
            SlotLevel = 0;
            BeamNumber = 0;
            PointSpentOnAddingAC = 0;
            IsValidBlastBreakthrough = false;
            IsOverload = false;
            HasBlastPursuit = false;
        }

        private sealed class VersatilitySupportConditionCustom :
            ICharacterTurnStartListener, ICustomConditionFeature, IMagicalAttackBeforeHitConfirmedOnEnemy,
            ICharacterBattleEndedListener
        {
            public void OnCharacterBattleEnded(GameLocationCharacter locationCharacter)
            {
                var rulesetCharacter = locationCharacter.RulesetCharacter;

                if (!rulesetCharacter.GetVersatilitySupportCondition(out var supportCondition))
                {
                    return;
                }

                var copiedSpells = supportCondition.CopiedSpells;
                var warlockRepertoire =
                    rulesetCharacter.GetOriginalHero()!.SpellRepertoires.Find(x =>
                        x.SpellCastingClass == CharacterClassDefinitions.Warlock);

                warlockRepertoire.KnownCantrips.RemoveAll(x => copiedSpells.Contains(x));
                warlockRepertoire.KnownSpells.RemoveAll(x => copiedSpells.Contains(x));
            }

            public void OnCharacterTurnStarted(GameLocationCharacter locationCharacter)
            {
                if (locationCharacter.RulesetCharacter.GetVersatilitySupportCondition(out var supportCondition))
                {
                    supportCondition.ClearEarnedPointsInThisTurn();
                }
            }

            // Do first time init
            public void OnApplyCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
            {
                if (rulesetCondition is not VersatilitySupportRulesetCondition supportCondition)
                {
                    return;
                }

                supportCondition.InitSupportCondition(target);
                TurnOffPointReservingPower(target, supportCondition);
            }

            // The clearing is called when next time this condition is added
            // So we can do some cleaning here
            // This condition is removed on level up and long rest, so we reapply it.
            // But don't do this right here because if we are respec the character, this should cause a dead loop
            // We just remove the power from power used by me list.
            public void OnRemoveCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
            {
                Main.Info("Condition Versatility interrupted");

                if (target.CurrentHitPoints > 0)
                {
                    target.PowersUsedByMe.RemoveAll(x => x.PowerDefinition == PowerEldritchVersatilityPointPool);
                }
            }

            public IEnumerator OnMagicalAttackBeforeHitConfirmedOnEnemy(
                GameLocationCharacter attacker,
                GameLocationCharacter defender,
                ActionModifier magicModifier,
                RulesetEffect rulesetEffect,
                List<EffectForm> actualEffectForms,
                bool firstTarget,
                bool criticalHit)
            {
                var characterAttacker = attacker.RulesetCharacter;

                if (!IsEldritchBlast(rulesetEffect) ||
                    !characterAttacker.GetVersatilitySupportCondition(out var supportCondition))
                {
                    yield break;
                }

                var posOwner = attacker.locationPosition;
                var posDefender = defender.locationPosition;

                supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.EarnPoints,
                    int3.Distance(posOwner, posDefender) <= 6f && characterAttacker.HasAnyFeature(FeatureBlastPursuit)
                        ? 2
                        : 1
                );
            }
        }
    }

    private sealed class BlastBreakthroughCustom :
        ToggleableInvocation, ISpellCast, IModifyDamageAffinity, IActionExecutionHandled
    {
        private static readonly ConditionDefinition ConditionBlastBreakthroughRemoveImmunity =
            ConditionDefinitionBuilder.Create("ConditionBlastBreakthroughRemoveImmunity")
                .SetGuiPresentationNoContent(true)
                .AddFeatures(
                    FeatureDefinitionBuilder
                        .Create("FeatureBlastBreakthroughRemoveImmunity")
                        .SetGuiPresentationNoContent(true)
                        .SetCustomSubFeatures(new BlastBreakthroughRemoveImmunityCustom())
                        .AddToDB()
                )
                .AddToDB();

        private static readonly FeatureDefinitionMagicAffinity FeatureBlastBreakthroughNoPenalty =
            FeatureDefinitionMagicAffinityBuilder
                .Create("FeatureBlastBreakthroughNoPenalty")
                .SetGuiPresentationNoContent(true)
                .SetCastingModifiers(noProximityPenalty: true)
                .AddToDB();

        private static readonly FeatureDefinitionCombatAffinity FeatureBlastBreakthroughIgnoreCover =
            FeatureDefinitionCombatAffinityBuilder
                .Create(FeatureDefinitionCombatAffinitys.CombatAffinityWandOfWarMageCover,
                    "FeatureBlastBreakthroughIgnoreCover")
                .SetGuiPresentationNoContent(true)
                .AddToDB();

        public BlastBreakthroughCustom(string invocationName)
        {
            InvocationName = invocationName;
        }

        // Reserve points when finish cast EB, if fail turn it off
        public void OnActionExecutionHandled(GameLocationCharacter character, CharacterActionParams actionParams,
            ActionScope scope)
        {
            var rulesetCharacter = character.RulesetCharacter;

            if (Gui.Battle is null ||
                !IsEldritchBlast(actionParams.RulesetEffect) ||
                !rulesetCharacter.GetVersatilitySupportCondition(out var supportCondition))
            {
                return;
            }

            if (!IsInvocationActive(rulesetCharacter, InvocationName, out var rulesetInvocation))
            {
                return;
            }

            // Invalidate damage affinity and spell immunity
            supportCondition.IsValidBlastBreakthrough = false;
            actionParams.targetCharacters.ForEach(x =>
                x.RulesetCharacter
                    .RemoveAllConditionsOfCategoryAndType(AttributeDefinitions.TagCombat,
                        ConditionBlastBreakthroughRemoveImmunity.Name)
            );

            HandleToggle(rulesetCharacter, supportCondition, rulesetInvocation, false);

            if (!rulesetInvocation.Active)
            {
                rulesetCharacter.RefreshAll();
            }
        }

        public void ModifyDamageAffinity([UsedImplicitly] RulesetActor defender, RulesetActor attacker,
            List<FeatureDefinition> features)
        {
            if (attacker is not RulesetCharacter attackCharacter ||
                !attackCharacter.GetVersatilitySupportCondition(out var supportCondition) ||
                !supportCondition.IsValidBlastBreakthrough)
            {
                return;
            }

            var modifier = GetAbilityScoreModifier(attackCharacter, AttributeDefinitions.Strength);

            if (modifier < 3)
            {
                return;
            }

            features.RemoveAll(x => x is IDamageAffinityProvider { DamageAffinityType: DamageAffinityType.Resistance });

            if (modifier < 4)
            {
                return;
            }

            features.RemoveAll(x => x is IDamageAffinityProvider { DamageAffinityType: DamageAffinityType.Immunity });
        }

        // Spend reserved points on cast EB if success
        public IEnumerator OnSpellCast(RulesetCharacter featureOwner, GameLocationCharacter caster,
            CharacterActionCastSpell castAction, RulesetEffectSpell selectEffectSpell,
            RulesetSpellRepertoire selectedRepertoire, SpellDefinition selectedSpellDefinition)
        {
            if (Gui.Battle is null ||
                caster.RulesetCharacter != featureOwner ||
                selectedSpellDefinition != SpellDefinitions.EldritchBlast ||
                !IsInvocationActive(featureOwner, InvocationName, out _) ||
                !featureOwner.GetVersatilitySupportCondition(out var supportCondition))
            {
                yield break;
            }

            supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.BlastBreakthroughOrEmpower);
            supportCondition.IsValidBlastBreakthrough = true;

            var modifier = GetAbilityScoreModifier(featureOwner, AttributeDefinitions.Strength);

            if (modifier < 5)
            {
                yield break;
            }

            castAction.actionParams.targetCharacters.ForEach(
                x =>
                    InflictCondition(ConditionBlastBreakthroughRemoveImmunity, featureOwner, x.RulesetCharacter));
        }

        // Handle toggled in UI
        public override void OnInvocationToggled(GameLocationCharacter locCharacter, RulesetInvocation invocation)
        {
            var rulesetCharacter = locCharacter.RulesetCharacter;

            if (!rulesetCharacter.GetVersatilitySupportCondition(out var supportCondition))
            {
                return;
            }

            HandleToggle(rulesetCharacter, supportCondition, invocation, true);
        }

        // Handle Toggle
        public override void HandleToggle(RulesetCharacter character,
            VersatilitySupportRulesetCondition supportCondition, RulesetInvocation invocation, bool toggledInUI)
        {
            while (true)
            {
                if (invocation.Active)
                {
                    // Reserve for next use
                    if (!supportCondition.TryEarnOrSpendPoints(
                            PointAction.Reserve, PointUsage.BlastBreakthroughOrEmpower))
                    {
                        invocation.Toggle();
                        toggledInUI = false;

                        continue;
                    }

                    if (toggledInUI)
                    {
                        var modifier = GetAbilityScoreModifier(character, AttributeDefinitions.Strength);
                        var hero = character.GetOriginalHero();

                        if (modifier < 1)
                        {
                            return;
                        }

                        ApplyMyFeatures(hero, FeatureBlastBreakthroughIgnoreCover);

                        if (modifier < 2)
                        {
                            return;
                        }

                        ApplyMyFeatures(hero, FeatureBlastBreakthroughNoPenalty);
                    }
                }
                else
                {
                    if (toggledInUI)
                    {
                        supportCondition.TryEarnOrSpendPoints(PointAction.UnReserve,
                            PointUsage.BlastBreakthroughOrEmpower);
                    }

                    RemoveMyFeatures(
                        character.GetOriginalHero(),
                        FeatureBlastBreakthroughIgnoreCover,
                        FeatureBlastBreakthroughNoPenalty);
                }

                break;
            }
        }
    }

    private class BlastBreakthroughRemoveImmunityCustom : IRemoveSpellOrSpellLevelImmunity
    {
        public bool IsValid(RulesetCharacter character, RulesetCondition holdingCondition)
        {
            return true;
        }

        public bool ShouldRemoveImmunity(Func<SpellDefinition, bool> isImmuneToSpell)
        {
            return isImmuneToSpell(SpellDefinitions.EldritchBlast);
        }

        public bool ShouldRemoveImmunityLevel(Func<int, int, bool> isImmuneToSpellLevel)
        {
            return isImmuneToSpellLevel(0, 0);
        }
    }

    private sealed class BattlefieldShorthandCopySpells : ISpellCast
    {
        public IEnumerator OnSpellCast(
            RulesetCharacter featureOwner,
            GameLocationCharacter caster,
            CharacterActionCastSpell castAction,
            RulesetEffectSpell selectEffectSpell,
            RulesetSpellRepertoire selectedRepertoire,
            SpellDefinition selectedSpellDefinition)
        {
            if (!featureOwner.GetVersatilitySupportCondition(out var supportCondition))
            {
                yield break;
            }

            if (featureOwner != caster.RulesetCharacter)
            {
                // Nobody identified the spell
                if (string.IsNullOrEmpty(castAction.ActiveSpell.IdentifiedBy))
                {
                    yield break;
                }

                var owner = GameLocationCharacter.GetFromActor(featureOwner);
                var posOwner = owner.locationPosition;
                var posCaster = caster.locationPosition;
                var battleManager = ServiceRepository.GetService<IGameLocationBattleService>();

                if (int3.Distance(posOwner, posCaster) > 12f ||
                    !battleManager.CanAttackerSeeCharacterFromPosition(posCaster, posOwner, caster, owner))
                {
                    yield break;
                }
            }

            var warlockRepertoire = featureOwner.GetOriginalHero()!
                .SpellRepertoires.Find(x => x.SpellCastingClass == CharacterClassDefinitions.Warlock);

            if (warlockRepertoire is null)
            {
                yield break;
            }

            var spellLevel = selectedSpellDefinition.SpellLevel;
            var allKnownSpells = warlockRepertoire.EnumerateAvailableExtraSpells();

            allKnownSpells.AddRange(warlockRepertoire.KnownCantrips);
            allKnownSpells.AddRange(warlockRepertoire.knownSpells);
            warlockRepertoire.ExtraSpellsByTag.TryAdd("BattlefieldShorthand", new List<SpellDefinition>());

            // If the caster has this feature, check if the spell is copied, if so, remove it both from copied list and repertoire
            if (featureOwner == caster.RulesetCharacter)
            {
                if (supportCondition.CopiedSpells.Remove(selectedSpellDefinition))
                {
                    warlockRepertoire.ExtraSpellsByTag["BattlefieldShorthand"].Remove(selectedSpellDefinition);
                }

                yield break;
            }
            // Maximum copy-able spell level is half pool size

            if (allKnownSpells.Contains(selectedSpellDefinition) || spellLevel > supportCondition.MaxPoints / 2)
            {
                yield break;
            }

            // You yourself should pass a check again to copy it if not overload
            if (!supportCondition.IsOverload)
            {
                var checkModifier = new ActionModifier();

                GameLocationCharacter.GetFromActor(featureOwner).RollAbilityCheck(AttributeDefinitions.Intelligence,
                    SkillDefinitions.Arcana,
                    15 + spellLevel + Math.Max(0, spellLevel - supportCondition.CurrentPoints),
                    AdvantageType.None, checkModifier, false, -1, out var abilityCheckRollOutcome,
                    out _, true);

                // Fails check
                if (abilityCheckRollOutcome > RollOutcome.Success)
                {
                    yield break;
                }
            }

            var console = Gui.Game.GameConsole;
            var entry = new GameConsoleEntry("Feedback/BattlefieldShorthandCopySpellSuccess",
                console.consoleTableDefinition) { Indent = true };

            console.AddCharacterEntry(featureOwner, entry);
            entry.AddParameter(
                ConsoleStyleDuplet.ParameterType.Positive, selectedSpellDefinition.GuiPresentation.Title);
            console.AddEntry(entry);
            supportCondition.CopiedSpells.Add(selectedSpellDefinition);
            warlockRepertoire.ExtraSpellsByTag["BattlefieldShorthand"].Add(selectedSpellDefinition);
        }
    }

    private sealed class BattlefieldConversionRestoreSlot : IUsePowerFinishedByMe, IPowerUseValidity
    {
        private readonly FeatureDefinition _triggerPower;

        public BattlefieldConversionRestoreSlot(FeatureDefinition triggerPower)
        {
            _triggerPower = triggerPower;
        }

        public bool CanUsePower(RulesetCharacter character, FeatureDefinitionPower power)
        {
            var rulesetHero = character.GetOriginalHero();

            if (rulesetHero is not { IsDeadOrDyingOrUnconscious: false })
            {
                return false;
            }

            // Have spent pact slot && have enough points
            return SharedSpellsContext.GetWarlockUsedSlots(rulesetHero) > 0 &&
                   character.GetVersatilitySupportCondition(out var supportCondition) &&
                   supportCondition.TryEarnOrSpendPoints(PointAction.Require, PointUsage.BattlefieldConversionSuccess);
        }

        public IEnumerator OnUsePowerFinishedByMe(CharacterActionUsePower action, FeatureDefinitionPower power)
        {
            if (power != _triggerPower)
            {
                yield break;
            }

            var gameLocationCharacter = action.ActingCharacter;
            var rulesetCharacter = gameLocationCharacter.RulesetCharacter;
            var rulesetHero = rulesetCharacter.GetOriginalHero();

            rulesetCharacter.GetVersatilitySupportCondition(out var supportCondition);

            if (!supportCondition.IsOverload)
            {
                // Check to restore slot
                var checkModifier = new ActionModifier();

                gameLocationCharacter.RollAbilityCheck("Intelligence", "Arcana", supportCondition.CreateSlotDC,
                    AdvantageType.None, checkModifier, false, -1, out var abilityCheckRollOutcome,
                    out _, true);

                // If success increase DC, other wise decrease DC
                var createSuccess = abilityCheckRollOutcome <= RollOutcome.Success;

                supportCondition.ModifySlotDC(createSuccess, rulesetHero);

                // Log to notify outcome and new DC
                var console = Gui.Game.GameConsole;
                var entry = createSuccess
                    ? new GameConsoleEntry("Feedback/&BattlefieldConversionCreateSlotSuccess",
                        console.consoleTableDefinition) { Indent = true }
                    : new GameConsoleEntry("Feedback/&BattlefieldConversionCreateSlotFailure",
                        console.consoleTableDefinition) { Indent = true };

                console.AddCharacterEntry(rulesetCharacter, entry);
                entry.AddParameter(ConsoleStyleDuplet.ParameterType.AbilityInfo,
                    supportCondition.CreateSlotDC.ToString());
                console.AddEntry(entry);

                // If fails
                if (abilityCheckRollOutcome > RollOutcome.Success)
                {
                    supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.BattlefieldConversionFailure);
                    yield break;
                }
            }

            supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.BattlefieldConversionSuccess);

            if (rulesetHero == null)
            {
                yield break;
            }

            var warlockRepertoire =
                rulesetHero.SpellRepertoires.Find(x => x.SpellCastingClass == CharacterClassDefinitions.Warlock);
            var slotLevelIndex = SharedSpellsContext.IsMulticaster(rulesetHero)
                ? -1
                : SharedSpellsContext.GetWarlockSpellLevel(rulesetHero);

            // Restore Slot and refresh
            if (!warlockRepertoire.usedSpellsSlots.TryGetValue(slotLevelIndex, out var warlockUsedSlots))
            {
                yield break;
            }

            warlockRepertoire.usedSpellsSlots[slotLevelIndex] = Math.Max(0, warlockUsedSlots - 1);
            warlockRepertoire.RepertoireRefreshed?.Invoke(warlockRepertoire);
        }
    }

    private sealed class EldritchAegisTwistHit : IAttackBeforeHitPossibleOnMeOrAlly
    {
        private static readonly ConditionDefinition ConditionEldritchAegisAddAC = ConditionDefinitionBuilder
            .Create("ConditionEldritchAegisAddAC")
            .SetGuiPresentation(Category.Condition, ConditionMagicallyArmored)
            .SetPossessive()
            .SetCustomSubFeatures(new EldritchAegisAddACCustom())
            .AddToDB();

        public IEnumerator OnAttackBeforeHitPossibleOnMeOrAlly(GameLocationBattleManager battleManager,
            GameLocationCharacter featureOwner, GameLocationCharacter attacker, GameLocationCharacter defender,
            RulesetAttackMode attackMode, RulesetEffect rulesetEffect, ActionModifier attackModifier, int attackRoll)
        {
            var ownerCharacter = featureOwner.RulesetCharacter;
            var defenderCharacter = defender.RulesetCharacter;
            var alreadyBlocked = defenderCharacter.HasConditionOfType(ConditionEldritchAegisAddAC);
            var posOwner = featureOwner.locationPosition;
            var posDefender = defender.locationPosition;

            if (!alreadyBlocked &&
                (int3.Distance(posOwner, posDefender) > 6f ||
                 !battleManager.CanAttackerSeeCharacterFromPosition(posDefender, posOwner, defender, featureOwner))
               )
            {
                yield break;
            }

            // This function also adjust AC to just enough block the attack, so if alreadyBlocked, we should not abort.
            if ((!featureOwner.CanReact(true) && !alreadyBlocked) ||
                !featureOwner.RulesetCharacter.GetVersatilitySupportCondition(out var supportCondition))
            {
                yield break;
            }

            // Get attack roll outcome
            var totalAttack = attackRoll
                              + (attackMode?.ToHitBonus ?? rulesetEffect?.MagicAttackBonus ?? 0)
                              + attackModifier.AttackRollModifier;
            var wisdomModifier = GetAbilityScoreModifier(ownerCharacter, AttributeDefinitions.Wisdom);
            // Get AC
            var currentValue = defenderCharacter.RefreshArmorClass(false, true).CurrentValue;
            defenderCharacter.GetAttribute("ArmorClass").ReleaseCopy();
            // Compute difference
            var requiredACAddition = totalAttack - currentValue + 1;
            // If other actions already blocked it
            if (requiredACAddition <= 0)
            {
                yield break;
            }

            var console = Gui.Game.GameConsole;
            var entry =
                new GameConsoleEntry("Feedback/EldritchAegisGiveACBonus", console.consoleTableDefinition)
                {
                    Indent = true
                };
            console.AddCharacterEntry(ownerCharacter, entry);
            entry.AddParameter(ConsoleStyleDuplet.ParameterType.Positive, $"{requiredACAddition}");

            // If already applied, we just auto add AC if it's not enough.
            if (alreadyBlocked)
            {
                // maximum AC bonus is wisdom modifier
                if (supportCondition.PointSpentOnAddingAC + requiredACAddition > wisdomModifier)
                {
                    yield break;
                }

                if (supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.EldritchAegis,
                        requiredACAddition))
                {
                    console.AddEntry(entry);
                }

                yield break;
            }

            // If not, can we prevent hit?
            if (requiredACAddition > wisdomModifier ||
                !supportCondition.TryEarnOrSpendPoints(PointAction.Require, PointUsage.EldritchAegis,
                    requiredACAddition))
            {
                yield break;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var count = actionService.PendingReactionRequestGroups.Count;

            var actionParams = new CharacterActionParams(featureOwner, (Id)ExtraActionId.DoNothingReaction)
            {
                StringParameter = "CustomReactionEldritchAegis"
                    .Formatted(Category.Reaction, defender.Name)
            };

            RequestCustomReaction(actionService, "EldritchAegis", actionParams, requiredACAddition);

            yield return battleManager.WaitForReactions(featureOwner, actionService, count);

            if (!actionParams.ReactionValidated)
            {
                yield break;
            }

            //Spend points
            supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.EldritchAegis,
                requiredACAddition);
            InflictCondition(ConditionEldritchAegisAddAC, ownerCharacter, defenderCharacter);
            console.AddEntry(entry);
        }

        private sealed class EldritchAegisAddACCustom : IModifyAC, ICustomConditionFeature
        {
            public void OnApplyCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
            {
                // empty
            }

            public void OnRemoveCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
            {
                RulesetEntity.TryGetEntity<RulesetCharacter>(rulesetCondition.SourceGuid, out var sourceCharacter);
                sourceCharacter.GetVersatilitySupportCondition(out var supportCondition);
                supportCondition?.ClearPointSpentOnAddingAC();
            }

            public void GetAC(RulesetCharacter owner, bool callRefresh, bool dryRun, FeatureDefinition dryRunFeature,
                out RulesetAttributeModifier attributeModifier, out TrendInfo trendInfo)
            {
                owner.TryGetConditionOfCategoryAndType(AttributeDefinitions.TagCombat,
                    ConditionEldritchAegisAddAC.Name, out var activeCondition);
                RulesetEntity.TryGetEntity<RulesetCharacter>(activeCondition.SourceGuid, out var sourceCharacter);
                sourceCharacter.GetVersatilitySupportCondition(out var supportCondition);
                var acBonus = supportCondition.PointSpentOnAddingAC;
                attributeModifier = RulesetAttributeModifier.BuildAttributeModifier(AttributeModifierOperation.Additive,
                    acBonus, AttributeDefinitions.TagCombat);
                trendInfo = new TrendInfo(acBonus, FeatureSourceType.Condition, activeCondition.Name, null,
                    attributeModifier);
            }
        }
    }

    private class ReactionResourceEldritchVersatilityPoints : ICustomReactionResource, ICustomReactionCustomResourceUse
    {
        private readonly int _requestPoints;

        public ReactionResourceEldritchVersatilityPoints(int requestPoints)
        {
            _requestPoints = requestPoints;
        }

        public string GetRequestPoints(RulesetCharacter character)
        {
            character.GetVersatilitySupportCondition(out _);

            return $"{_requestPoints}";
        }

        public AssetReferenceSprite Icon => Sprites.EldritchVersatilityResourceIcon;

        public string GetUses(RulesetCharacter character)
        {
            character.GetVersatilitySupportCondition(out var supportCondition);

            return $"{supportCondition.CurrentPoints}";
        }
    }

    private class EldritchWardAidSave : ITryAlterOutcomeFailedSavingThrow
    {
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
            if (!ShouldTrigger(battleManager, action, defender, helper))
            {
                yield break;
            }

            var ownerCharacter = helper.RulesetCharacter;

            if (!ownerCharacter.GetVersatilitySupportCondition(out var supportCondition))
            {
                yield break;
            }

            var requiredSaveAddition = -action.SaveOutcomeDelta;
            var wisdomModifier = GetAbilityScoreModifier(ownerCharacter, AttributeDefinitions.Wisdom);

            // bonus > modifier or points not enough
            if (requiredSaveAddition > wisdomModifier ||
                !supportCondition.TryEarnOrSpendPoints(PointAction.Require, PointUsage.EldritchWard,
                    requiredSaveAddition))
            {
                yield break;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var count = actionService.PendingReactionRequestGroups.Count;

            var actionParams = new CharacterActionParams(helper, (Id)ExtraActionId.DoNothingReaction)
            {
                StringParameter = "CustomReactionEldritchWard"
                    .Formatted(Category.Reaction, defender.Name)
            };

            RequestCustomReaction(actionService, "EldritchWard", actionParams, requiredSaveAddition);

            yield return battleManager.WaitForReactions(helper, actionService, count);

            if (!actionParams.ReactionValidated)
            {
                yield break;
            }

            // Spend resources
            supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.EldritchWard,
                requiredSaveAddition);
            // Change outcome
            action.SaveOutcome = RollOutcome.Success;
            action.saveOutcomeDelta = 0;
            // Log to console
            var console = Gui.Game.GameConsole;
            var entry =
                new GameConsoleEntry("Feedback/&EldritchWardGivesSaveBonus", console.consoleTableDefinition)
                {
                    Indent = true
                };

            console.AddCharacterEntry(ownerCharacter, entry);
            entry.AddParameter(ConsoleStyleDuplet.ParameterType.Positive, $"{requiredSaveAddition}");

            var dc = action.GetSaveDC().ToString();

            entry.AddParameter(ConsoleStyleDuplet.ParameterType.SuccessfulRoll,
                Gui.Format(GameConsole.SaveSuccessOutcome, dc));
            console.AddEntry(entry);
        }

        private static bool ShouldTrigger(
            IGameLocationBattleService gameLocationBattleService,
            CharacterAction action,
            GameLocationCharacter defender,
            GameLocationCharacter helper)
        {
            return action.RolledSaveThrow
                   && action.SaveOutcome is RollOutcome.Failure
                   && helper.CanReact()
                   && !defender.IsOppositeSide(helper.Side)
                   && gameLocationBattleService.IsWithinXCells(helper, defender, 7)
                   && gameLocationBattleService.CanAttackerSeeCharacterFromPosition(
                       defender.LocationPosition, helper.LocationPosition, defender, helper);
        }
    }

    private class BlastEmpowerCustom : ToggleableInvocation, IActionExecutionHandled, ISpellCast
    {
        public BlastEmpowerCustom(string name)
        {
            InvocationName = name;
        }

        // Reserve points when finish cast EB, if fail turn it off
        public void OnActionExecutionHandled(GameLocationCharacter character, CharacterActionParams actionParams,
            ActionScope scope)
        {
            var rulesetCharacter = character.RulesetCharacter;
            if (Gui.Battle is null ||
                !IsEldritchBlast(actionParams.RulesetEffect) ||
                !rulesetCharacter.GetVersatilitySupportCondition(out var supportCondition))
            {
                return;
            }

            if (!IsInvocationActive(rulesetCharacter, InvocationName, out var rulesetInvocation))
            {
                return;
            }

            HandleToggle(rulesetCharacter, supportCondition, rulesetInvocation, false);
            if (!rulesetInvocation.Active)
            {
                rulesetCharacter.RefreshAll();
            }
        }

        // Spend reserved points on cast EB
        public IEnumerator OnSpellCast(RulesetCharacter featureOwner, GameLocationCharacter caster,
            CharacterActionCastSpell castAction, RulesetEffectSpell selectEffectSpell,
            RulesetSpellRepertoire selectedRepertoire, SpellDefinition selectedSpellDefinition)
        {
            if (Gui.Battle is null ||
                caster.RulesetCharacter != featureOwner ||
                selectedSpellDefinition != SpellDefinitions.EldritchBlast ||
                !IsInvocationActive(featureOwner, InvocationName, out _) ||
                !featureOwner.GetVersatilitySupportCondition(out var supportCondition)
               )
            {
                yield break;
            }

            supportCondition.TryEarnOrSpendPoints(PointAction.Modify, PointUsage.BlastBreakthroughOrEmpower);
        }

        // Handle toggled in UI
        public override void OnInvocationToggled(GameLocationCharacter locCharacter, RulesetInvocation invocation)
        {
            var rulesetCharacter = locCharacter.RulesetCharacter;
            if (!rulesetCharacter.GetVersatilitySupportCondition(out var supportCondition))
            {
                return;
            }

            HandleToggle(rulesetCharacter, supportCondition, invocation, true);
        }

        // Handle Toggle
        public override void HandleToggle(RulesetCharacter character,
            VersatilitySupportRulesetCondition supportCondition, RulesetInvocation invocation, bool toggledInUI)
        {
            while (true)
            {
                if (invocation.Active)
                {
                    // Reserve for next use
                    if (!supportCondition.TryEarnOrSpendPoints(PointAction.Reserve,
                            PointUsage.BlastBreakthroughOrEmpower))
                    {
                        invocation.Toggle();
                        toggledInUI = false;
                        continue;
                    }
                }
                else
                {
                    if (toggledInUI)
                    {
                        supportCondition.TryEarnOrSpendPoints(PointAction.UnReserve,
                            PointUsage.BlastBreakthroughOrEmpower);
                    }
                }

                break;
            }
        }
    }

    private class ConditionBlastOverloadCustom : ICustomConditionFeature
    {
        public void OnApplyCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            if (target.GetVersatilitySupportCondition(out var supportCondition))
            {
                supportCondition.IsOverload = true;
            }
        }

        public void OnRemoveCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            if (!target.GetVersatilitySupportCondition(out var supportCondition))
            {
                return;
            }

            supportCondition.IsOverload = false;
            supportCondition.TryEarnOrSpendPoints(PointAction.Modify);
        }
    }

    private class AddAbilityScoreBonus : IDefinitionCustomCode
    {
        private readonly string _abilityScore;

        public AddAbilityScoreBonus(string abilityScore)
        {
            _abilityScore = abilityScore;
        }

        public void ApplyFeature(RulesetCharacterHero hero, [UsedImplicitly] string tag)
        {
            hero.Attributes[_abilityScore].ActiveModifiers.TryAdd(
                RulesetAttributeModifier
                    .BuildAttributeModifier(AttributeModifierOperation.Additive, 1f, Name));
        }

        public void RemoveFeature(RulesetCharacterHero hero, [UsedImplicitly] string tag)
        {
            var modifiers = hero.Attributes[_abilityScore].ActiveModifiers;
            modifiers.Remove(modifiers.Find(x => x.Tags.Contains(Name)));
        }
    }

    private class EldritchVersatilityAdeptCustom : IDefinitionCustomCode
    {
        public void ApplyFeature(RulesetCharacterHero hero, [UsedImplicitly] string tag)
        {
            if (hero.HasAnyFeature(PowerEldritchVersatilityPointPool))
            {
                return;
            }

            hero.ActiveFeatures[tag].TryAdd(PowerEldritchVersatilityPointPool);
        }

        public void RemoveFeature(RulesetCharacterHero hero, [UsedImplicitly] string tag)
        {
            if (hero.GetSubclassLevel(CharacterClassDefinitions.Warlock, PatronEldritchSurge.Name) < 1)
            {
                hero.ActiveFeatures.Values.Do(x => x.RemoveAll(y => y == PowerEldritchVersatilityPointPool));
            }
        }
    }

    private abstract class ToggleableInvocation : IInvocationToggled
    {
        public string InvocationName;
        public abstract void OnInvocationToggled(GameLocationCharacter character, RulesetInvocation rulesetInvocation);

        public abstract void HandleToggle(RulesetCharacter character,
            VersatilitySupportRulesetCondition supportCondition, RulesetInvocation invocation, bool toggledInUI);
    }
}
