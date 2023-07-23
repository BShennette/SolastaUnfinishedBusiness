﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class Lunger : AbstractFightingStyle
{
    internal const string Name = "Lunger";

    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create(Name)
        .SetGuiPresentation(Category.FightingStyle, Sprites.GetSprite(Name, Resources.Lunger, 256))
        .SetFeatures(
            FeatureDefinitionBuilder
                .Create("FeatureLunger")
                .SetGuiPresentationNoContent(true)
                .SetCustomSubFeatures(new IncreaseWeaponReach(1, (mode, rulesetItem, _) =>
                {
                    var item = mode?.SourceObject as RulesetItem ?? rulesetItem;
                    return ValidatorsWeapon.IsMelee(item) &&
                           !ValidatorsWeapon.HasAnyWeaponTag(item, TagsDefinitions.WeaponTagHeavy);
                }, Name))
                .AddToDB())
        .AddToDB();

    internal override List<FeatureDefinitionFightingStyleChoice> FightingStyleChoice => new()
    {
        CharacterContext.FightingStyleChoiceMonk,
        FightingStyleChampionAdditional,
        FightingStyleFighter,
        FightingStylePaladin,
        FightingStyleRanger
    };
}
