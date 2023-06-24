﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

public static class ActionSwitching
{
    internal static readonly TutorialStepDefinition Tutorial = TutorialStepDefinitionBuilder
        .Create("TutorialActionSwitching")
        .SetGuiPresentation(Category.Tutorial, Sprites.TutorialActionSwitching)
        .AddToDB();

    internal static void Load()
    {
        //Mark Action Surge to track spell flags separately
        FeatureDefinitionAdditionalActions.AdditionalActionSurgedMain
            .AddCustomSubFeatures(ActionWithCustomSpellTracking.Mark);

        //Make Horde Breaker add condition instead of using trigger
        var hordeBreaker = FeatureDefinitionAdditionalActions.AdditionalActionHunterHordeBreaker;
        hordeBreaker.AddCustomSubFeatures(new HordeBreaker(ConditionDefinitionBuilder
            .Create("ConditionHunterHordeBreaker")
            .SetGuiPresentationNoContent()
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(FeatureDefinitionAdditionalActionBuilder
                .Create("AdditionalActionHunterHordeBreaker2")
                .SetGuiPresentation(hordeBreaker.GuiPresentation)
                .SetActionType(ActionDefinitions.ActionType.Main)
                .SetRestrictedActions(ActionDefinitions.Id.AttackMain)
                .SetMaxAttacksNumber(1)
                .AddToDB())
            .AddToDB()));
    }

    private static void EnumerateFeaturesHierarchically<T>(
        ICollection<(FeatureDefinition feature, string origin)> features,
        IEnumerable<FeatureDefinition> parentList, string origin)
    {
        foreach (var feature in parentList
                     .Where(feature => feature.AllowsDuplicate || features.All(x => x.feature != feature)))
        {
            switch (feature)
            {
                case FeatureDefinitionFeatureSet { Mode: FeatureDefinitionFeatureSet.FeatureSetMode.Union } set:
                    EnumerateFeaturesHierarchically<T>(features, set.FeatureSet, origin);
                    break;
                case T:
                    features.Add((feature, origin));
                    break;
            }
        }
    }

    private static List<FeatureDefinition> GetConditionFeatures<T>(
        ConditionDefinition condition,
        List<FeatureDefinition> list = null)
    {
        list ??= new List<FeatureDefinition>();

        if (condition.parentCondition != null)
        {
            GetConditionFeatures<T>(condition.parentCondition, list);
        }

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var f in condition.features)
        {
            if (f is T)
            {
                list.Add(f);
            }
        }

        return list;
    }

    internal static List<(FeatureDefinition feature, string origin)> EnumerateActorFeatures<T>(RulesetActor actor)
    {
        return actor switch
        {
            RulesetCharacterHero hero => EnumerateHeroFeatures<T>(hero),
            RulesetCharacterMonster monster => EnumerateMonsterFeatures<T>(monster),
            _ => null
        };
    }

    private static List<(FeatureDefinition feature, string origin)> EnumerateHeroFeatures<T>(
        RulesetCharacterHero hero,
        bool skipConditions = false)
    {
        List<(FeatureDefinition feature, string origin)> features = new();

        //Character features
        foreach (var activeFeature in hero.activeFeatures)
        {
            EnumerateFeaturesHierarchically<T>(features, activeFeature.Value, activeFeature.Key);
        }

        //Equipment
        foreach (var activeItemFeature in hero.activeItemFeatures)
        {
            EnumerateFeaturesHierarchically<T>(features, activeItemFeature.Value, activeItemFeature.Key.Name);
        }

        //Fighting Styles
        foreach (var activeFightingStyle in hero.activeFightingStyles)
        {
            EnumerateFeaturesHierarchically<T>(features, activeFightingStyle.Features, activeFightingStyle.Name);
        }

        //Feats
        foreach (var trainedFeat in hero.trainedFeats)
        {
            EnumerateFeaturesHierarchically<T>(features, trainedFeat.Features, trainedFeat.Name);
        }

        //Conditions
        if (!skipConditions)
        {
            hero.GetAllConditions(hero.AllConditionsForEnumeration);
            hero.AllConditionsForEnumeration.Sort((a, b) => a.Guid.CompareTo(b.Guid));
            foreach (var rulesetCondition in hero.AllConditionsForEnumeration)
            {
                EnumerateFeaturesHierarchically<T>(features,
                    GetConditionFeatures<T>(rulesetCondition.ConditionDefinition),
                    $"{rulesetCondition.Name}:{rulesetCondition.Guid}");
            }
        }

        //Metamagic
        features.AddRange(hero.MetamagicFeatures.Where(f => f.Value is T).Select(x => (x.Value, x.Key.Name)));

        foreach (var invocation in hero.Invocations)
        {
            if (!invocation.Active)
            {
                continue;
            }

            var grantedFeature = invocation.InvocationDefinition.GrantedFeature;
            if (grantedFeature != null && grantedFeature is T)
            {
                features.Add((grantedFeature, invocation.InvocationDefinition.Name));
            }
            else if (grantedFeature != null &&
                     grantedFeature is FeatureDefinitionFeatureSet set)
            {
                EnumerateFeaturesHierarchically<T>(features, set.FeatureSet, invocation.InvocationDefinition.Name);
            }
        }

        return features;
    }

    private static List<(FeatureDefinition feature, string origin)> EnumerateMonsterFeatures<T>(
        RulesetCharacterMonster monster)
    {
        List<(FeatureDefinition feature, string origin)> features = new();

        //Monster features
        EnumerateFeaturesHierarchically<T>(features, monster.activeFeatures, monster.monsterDefinition.Name);

        //Original hero features
        if (Main.Settings.EnumerateOriginSubFeatures && monster.originalFormCharacter is RulesetCharacterHero hero)
        {
            features.AddRange(EnumerateHeroFeatures<T>(hero, true));
        }

        //Conditions
        monster.GetAllConditions(monster.AllConditionsForEnumeration);
        monster.AllConditionsForEnumeration.Sort((a, b) => a.Guid.CompareTo(b.Guid));
        foreach (var rulesetCondition in monster.AllConditionsForEnumeration)
        {
            EnumerateFeaturesHierarchically<T>(features, GetConditionFeatures<T>(rulesetCondition.ConditionDefinition),
                $"{rulesetCondition.Name}:{rulesetCondition.Guid}");
        }

        return features;
    }

    internal static void PrioritizeAction(GameLocationCharacter character, ActionDefinitions.ActionType type, int index)
    {
        Main.Info($"PrioritizeAction [{character.Name}] {type}");
        var service = ServiceRepository.GetService<IGameLocationActionService>();
        var actionParams = new CharacterActionParams
        {
            ActingCharacter = character,
            ActionDefinition = service.AllActionDefinitions[(ActionDefinitions.Id)ExtraActionId.PrioritizeAction],
            IntParameter = (int)type,
            IntParameter2 = index
        };
        service.ExecuteAction(actionParams, null, true);
    }

    internal static void DoPrioritizeAction(GameLocationCharacter character, ActionDefinitions.ActionType type,
        int index)
    {
        Main.Info($"DoPrioritizeAction [{character.Name}] {type}");
        var rank = character.CurrentActionRankByType[type];
        if (index <= rank)
        {
            return;
        }

        var map = character.UsedSpecialFeatures;
        var filters = character.ActionPerformancesByType[type];
        var list = LoadIndexes(map, type, filters.Count);

        var k = list[index];

        //Store current action attacks
        var data = PerformanceFilterExtraData.GetData(filters[rank]);
        data?.StoreAttacks(character, type);
        data?.StoreSpellcasting(character, type);

        list.RemoveAt(index);
        list.Insert(rank, k);

        SaveIndexes(map, type, list);

        character.RefreshActionPerformances();

        data = PerformanceFilterExtraData.GetData(filters[rank]);
        data?.LoadAttacks(character, type);
        data?.LoadSpellcasting(character, type);

        character.RulesetCharacter?.RefreshAttackModes();
    }

    internal static void CheckIfActionSwitched(GameLocationCharacter character,
        CharacterActionParams actionParams,
        ActionDefinitions.ActionScope scope,
        int mainRank, int mainAttacks, int bonusRank, int bonusAttacks)
    {
        if (!Main.Settings.EnableActionSwitching)
        {
            return;
        }

        if (scope != ActionDefinitions.ActionScope.Battle)
        {
            return;
        }

        if (character.Side != RuleDefinitions.Side.Ally)
        {
            return;
        }

        var type = actionParams.ActionDefinition.ActionType;

        var switched = type switch
        {
            ActionDefinitions.ActionType.Main => CheckIfActionSwitched(character, type, mainRank, mainAttacks),
            ActionDefinitions.ActionType.Bonus => CheckIfActionSwitched(character, type, bonusRank, bonusAttacks),
            _ => false
        };

        if (switched)
        {
            character.RulesetCharacter.RefreshAttackModes();
        }
    }

    private static bool CheckIfActionSwitched(GameLocationCharacter character, ActionDefinitions.ActionType type,
        int wasRank, int wasAttacks)
    {
        var rank = character.CurrentActionRankByType[type];
        var filters = character.ActionPerformancesByType[type];
        var newData = rank < filters.Count ? PerformanceFilterExtraData.GetData(filters[rank]) : null;

        if (rank == wasRank)
        {
            Main.Info($"CheckIfActionSwitched [{character.Name}] {type} rank: {rank} - NO CHANGE");
            newData?.StoreAttacks(character, type);
            newData?.StoreSpellcasting(character, type);
            return false;
        }

        Main.Info($"CheckIfActionSwitched [{character.Name}] {type} was: {wasRank} new: {rank}");

        var wasData = PerformanceFilterExtraData.GetData(filters[wasRank]);

        //TODO: has potential to spend extra attack for sorcerers who initiate Flexible Casting (slot-sorcerer point conversion) and then cancel it.
        wasData?.StoreAttacks(character, type, wasAttacks + 1);
        wasData?.StoreSpellcasting(character, type);
        newData?.LoadAttacks(character, type);
        newData?.LoadSpellcasting(character, type);
        return true;
    }

    private static List<int> LoadIndexes(IReadOnlyDictionary<string, int> map, ActionDefinitions.ActionType type,
        int max)
    {
        var list = new List<int>();

        for (var i = 0; i < max; i++)
        {
            list.Add(map.TryGetValue($"ActionIndex{type}:{i}", out var k) ? k : i);
        }

        return list;
    }

    private static void SaveIndexes(
        IDictionary<string, int> map,
        ActionDefinitions.ActionType type,
        IReadOnlyList<int> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            map.AddOrReplace($"ActionIndex{type}:{i}", list[i]);
        }
    }

    public static void ResortPerformances(GameLocationCharacter character)
    {
        if (!Main.Settings.EnableActionSwitching)
        {
            return;
        }

        if (character.Side != RuleDefinitions.Side.Ally)
        {
            return;
        }

        if (character.RulesetCharacter is not (RulesetCharacterHero or RulesetCharacterMonster))
        {
            return;
        }

        var features = EnumerateActorFeatures<IAdditionalActionsProvider>(character.RulesetCharacter);

        if (features == null)
        {
            return;
        }

        var onKill = features.FindAll(x => x.feature is IAdditionalActionsProvider
        {
            TriggerCondition: RuleDefinitions.AdditionalActionTriggerCondition.HasDownedAnEnemy
        });

        if (!onKill.Empty())
        {
            features.RemoveAll(x => onKill.Contains(x));
            features.AddRange(onKill);
        }

        features.RemoveAll(x =>
        {
            var validator = x.feature.GetFirstSubFeatureOfType<IDefinitionApplicationValidator>();

            return validator != null && !validator.IsValid(x.feature, character.RulesetCharacter);
        });

        //remove triggered features - all such features are reworked to grant conditions
        features.RemoveAll(x =>
            x.feature is IAdditionalActionsProvider
            {
                TriggerCondition: RuleDefinitions.AdditionalActionTriggerCondition.HasDownedAnEnemy
            });

        ResortPerformancesOfType(character, features, ActionDefinitions.ActionType.Main);
        ResortPerformancesOfType(character, features, ActionDefinitions.ActionType.Bonus);

        character.dirtyActions = true;
        character.ActionsRefreshed?.Invoke(character);
    }

    private static void ResortPerformancesOfType(GameLocationCharacter character,
        IEnumerable<(FeatureDefinition feature, string origin)> allFeatures, ActionDefinitions.ActionType type)
    {
        var features = allFeatures
            .Where(x => x.feature is IAdditionalActionsProvider f && f.ActionType == type)
            .ToList();
        var filters = character.ActionPerformancesByType[type];
        var filtersCount = filters.Count;

        Main.Info(
            $"ResortPerformancesOfType [{character.Name}] {type} filters: {filtersCount} features: [{string.Join(", ", features.Select(x => $"<{x.feature.Name}|{x.origin}>"))}]");

        for (var i = 0; i < filtersCount; i++)
        {
            if (i == 0)
            {
                PerformanceFilterExtraData.AddData(filters[i], null, null);
            }
            else
            {
                var (feature, origin) = features[i - 1];
                PerformanceFilterExtraData.AddData(filters[i], feature, origin);
            }
        }

        var rank = character.CurrentActionRankByType[type];
        var list = LoadIndexes(character.UsedSpecialFeatures, type, filtersCount);

        Main.Info($"ResortPerformancesOfType [{character.Name}] {type} : [{string.Join(", ", list)}] rank: {rank}");

        var sorted = list.Select(k => filters[k]).ToList();

        if (rank < sorted.Count)
        {
            var data = PerformanceFilterExtraData.GetData(sorted[rank]);
            data?.LoadSpellcasting(character, type);
        }

        filters.SetRange(sorted);
    }

    public static void AccountRemovedCondition(RulesetCharacter character, RulesetCondition condition)
    {
        if (!Main.Settings.EnableActionSwitching)
        {
            return;
        }

        var locCharacter = GameLocationCharacter.GetFromActor(character);

        if (locCharacter == null)
        {
            Main.Info($"AccountRemovedCondition [{character.Name}] '{condition.Name}' NO LOC CHAR");
            return;
        }

        var conditionFeatures = new List<FeatureDefinition>();

        condition.ConditionDefinition.EnumerateFeaturesToBrowse<IAdditionalActionsProvider>(conditionFeatures);

        if (conditionFeatures.Empty())
        {
            Main.Info($"AccountRemovedCondition [{character.Name}] '{condition.Name}' NO ACTIONS");
            return;
        }

        var features = EnumerateActorFeatures<IAdditionalActionsProvider>(character);

        if (features == null)
        {
            Main.Info($"AccountRemovedCondition [{character.Name}] '{condition.Name}' NO FEATURES");
            return;
        }

        var origin = $"{condition.Name}:{condition.Guid}";

        foreach (var conditionFeature in conditionFeatures)
        {
            if (conditionFeature is not IAdditionalActionsProvider provider)
            {
                //Shouldn't happen, in place so Rider won't complain
                continue;
            }

            var type = provider.ActionType;
            var filters = locCharacter.ActionPerformancesByType[type];
            var max = filters.Count;
            var list = LoadIndexes(locCharacter.UsedSpecialFeatures, type, max);

            Main.Info($"AccountRemovedCondition [{character.Name}] '{conditionFeature.Name}' from '{origin}'");
            Main.Info($"AccountRemovedCondition [{character.Name}] WAS: [{string.Join(", ", list)}]");

            var k = -1;

            for (var i = 0; i < max; i++)
            {
                var filter = filters[i];
                var data = PerformanceFilterExtraData.GetData(filter);

                if (data == null) { continue; }

                if (data.Feature != conditionFeature || data.Origin != origin)
                {
                    continue;
                }

                k = i;
                break;
            }

            if (k < 0) { continue; }

            var rank = locCharacter.CurrentActionRankByType[type];

            Main.Info($"AccountRemovedCondition [{character.Name}] remove at: {k} rank was: {rank}");

            if (rank > k)
            {
                locCharacter.CurrentActionRankByType[type]--;
            }

            for (var i = max - 1; i >= 0; i--)
            {
                if (list[i] == k)
                {
                    list.RemoveAt(i);
                }
                else if (list[i] > k)
                {
                    list[i]--;
                }
            }

            Main.Info($"AccountRemovedCondition [{character.Name}] BECAME: [{string.Join(", ", list)}]");

            SaveIndexes(locCharacter.UsedSpecialFeatures, type, list);
        }
    }

    public static void SpendActionType(GameLocationCharacter character, ActionDefinitions.ActionType type)
    {
        if (!Main.Settings.EnableActionSwitching)
        {
            return;
        }

        if (type is not (ActionDefinitions.ActionType.Main or ActionDefinitions.ActionType.Bonus))
        {
            return;
        }

        var filters = character.actionPerformancesByType[type];
        var rank = character.currentActionRankByType[type];

        if (rank >= filters.Count)
        {
            return;
        }

        var data = PerformanceFilterExtraData.GetData(filters[rank]);

        if (data == null)
        {
            return;
        }

        Main.Info($"SpendActionType [{character.Name}] {type} rank: {rank}, filters: {filters.Count}");

        data.StoreAttacks(character, type);
        data.StoreSpellcasting(character, type);

        if (rank + 1 >= filters.Count)
        {
            return;
        }

        data = PerformanceFilterExtraData.GetData(filters[rank + 1]);
        if (data == null)
        {
            return;
        }

        data.LoadAttacks(character, type);
        data.LoadSpellcasting(character, type);
    }

    public static void RefundActionUse(GameLocationCharacter character, ActionDefinitions.ActionType type)
    {
        if (!Main.Settings.EnableActionSwitching)
        {
            return;
        }

        if (type is not (ActionDefinitions.ActionType.Main or ActionDefinitions.ActionType.Bonus))
        {
            return;
        }

        var rank = character.currentActionRankByType[type];

        if (rank <= 0)
        {
            return;
        }

        var filters = character.actionPerformancesByType[type];

        Main.Info($"RefundActionUse [{character.Name}] {type} rank: {rank}, filters: {filters.Count}");

        var data = PerformanceFilterExtraData.GetData(filters[rank]);

        if (data != null)
        {
            data.StoreAttacks(character, type);
            data.StoreSpellcasting(character, type);
        }

        data = PerformanceFilterExtraData.GetData(filters[rank - 1]);

        if (data == null)
        {
            return;
        }

        data.LoadAttacks(character, type);
        data.LoadSpellcasting(character, type);
    }

    private sealed class HordeBreaker : ITargetReducedToZeroHp
    {
        private readonly ConditionDefinition condition;

        public HordeBreaker(ConditionDefinition condition)
        {
            this.condition = condition;
        }

        public IEnumerator HandleCharacterReducedToZeroHp(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            if (!Main.Settings.EnableActionSwitching)
            {
                yield break;
            }

            if (attacker.RulesetCharacter.HasAnyConditionOfType(condition.Name))
            {
                yield break;
            }

            if (Gui.Battle?.ActiveContender != attacker)
            {
                yield break;
            }

            attacker.RulesetCharacter.InflictCondition(
                condition.Name,
                RuleDefinitions.DurationType.Round,
                0,
                RuleDefinitions.TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagCombat,
                attacker.RulesetCharacter.guid,
                attacker.RulesetCharacter.CurrentFaction.Name,
                1,
                null,
                0,
                0,
                0);
        }
    }
}
