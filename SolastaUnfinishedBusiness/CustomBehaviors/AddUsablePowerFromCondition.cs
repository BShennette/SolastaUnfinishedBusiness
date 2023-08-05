﻿using System.Linq;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

/**Adds power to character's usable powers when condition is applied and removes it when condition is removed*/
public class AddUsablePowerFromCondition : ICustomConditionFeature
{
    private readonly FeatureDefinitionPower power;

    public AddUsablePowerFromCondition(FeatureDefinitionPower power)
    {
        this.power = power;
    }

    public void OnApplyCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
    {
        if (target.UsablePowers.Any(u => u.PowerDefinition == power))
        {
            return;
        }

        var usablePower = new RulesetUsablePower(power, null, null);

        usablePower.Recharge();
        target.UsablePowers.Add(usablePower);
        target.RefreshUsablePower(usablePower);
    }

    public void OnRemoveCondition(RulesetCharacter target, RulesetCondition rulesetCondition)
    {
        target.UsablePowers.RemoveAll(usablePower => usablePower.PowerDefinition == power);
    }
}
