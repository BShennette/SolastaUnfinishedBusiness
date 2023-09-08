﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.CustomInterfaces;

// triggers on any attack, magical or physical, if there is an attack roll
// rulesetEffect != null is a magical attack
public interface IAttackBeforeHitConfirmedOnMe
{
    [UsedImplicitly]
    IEnumerator OnAttackBeforeHitConfirmedOnMe(GameLocationBattleManager battle,
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        ActionModifier attackModifier,
        RulesetAttackMode attackMode,
        bool rangedAttack,
        AdvantageType advantageType,
        List<EffectForm> actualEffectForms,
        RulesetEffect rulesetEffect,
        bool firstTarget,
        bool criticalHit);
}
