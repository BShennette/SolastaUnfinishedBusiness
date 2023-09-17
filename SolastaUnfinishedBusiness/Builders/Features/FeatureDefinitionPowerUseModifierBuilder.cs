﻿using System;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;

namespace SolastaUnfinishedBusiness.Builders.Features;

[UsedImplicitly]
internal class FeatureDefinitionPowerUseModifierBuilder
    : FeatureDefinitionBuilder<FeatureDefinitionModifyPowerPoolAmount, FeatureDefinitionPowerUseModifierBuilder>
{
    internal FeatureDefinitionPowerUseModifierBuilder SetFixedValue(
        FeatureDefinitionPower poolPower,
        int powerPoolModifier)
    {
        // ReSharper disable once InvocationIsSkipped
        PreConditions.ArgumentIsNotNull(poolPower, $"{GetType().Name}[{Definition.Name}] poolPower is null.");

        var modifier = Definition.Modifier;

        modifier.PowerPool = poolPower;
        modifier.Type = PowerPoolBonusCalculationType.Fixed;
        modifier.Value = powerPoolModifier;
        return this;
    }

    internal FeatureDefinitionPowerUseModifierBuilder SetModifier(
        FeatureDefinitionPower poolPower,
        PowerPoolBonusCalculationType type,
        string attribute = "",
        int value = 1)
    {
        // ReSharper disable once InvocationIsSkipped
        PreConditions.ArgumentIsNotNull(poolPower, $"{GetType().Name}[{Definition.Name}] poolPower is null.");

        var modifier = Definition.Modifier;

        modifier.PowerPool = poolPower;
        modifier.Type = type;
        modifier.Attribute = attribute;
        modifier.Value = value;
        return this;
    }

    #region Constructors

    protected FeatureDefinitionPowerUseModifierBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionPowerUseModifierBuilder(FeatureDefinitionModifyPowerPoolAmount original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    #endregion
}
