﻿using System.Collections;
using JetBrains.Annotations;

//This should have default namespace so that it can be properly created by `CharacterActionPatcher`
// ReSharper disable once CheckNamespace
[UsedImplicitly]
#pragma warning disable CA1050
public class CharacterActionCombatRageStart : CharacterAction
#pragma warning restore CA1050
{
    public CharacterActionCombatRageStart(CharacterActionParams actionParams)
        : base(actionParams)
    {
    }

    public override IEnumerator ExecuteImpl()
    {
        var actingCharacter = ActingCharacter;

        if (actingCharacter.Stealthy)
        {
            actingCharacter.SetStealthy(false);
        }

        var actionService = ServiceRepository.GetService<IGameLocationActionService>();
        var newParams = ActionParams.Clone();
        newParams.ActionDefinition = actionService.AllActionDefinitions[ActionDefinitions.Id.PowerNoCost];
        actionService.ExecuteAction(newParams, null, true);
        actingCharacter.RulesetCharacter.SpendRagePoint();

        yield return ServiceRepository.GetService<IGameLocationBattleService>()?
            .HandleReactionToRageStart(actingCharacter);
    }
}
