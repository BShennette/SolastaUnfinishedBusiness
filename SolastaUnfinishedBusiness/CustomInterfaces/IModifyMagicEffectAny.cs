﻿namespace SolastaUnfinishedBusiness.CustomInterfaces;

public interface IModifyMagicEffectAny
{
    public EffectDescription ModifyEffect(
        BaseDefinition definition,
        EffectDescription effectDescription,
        RulesetCharacter character,
        RulesetEffect rulesetEffect);
}
