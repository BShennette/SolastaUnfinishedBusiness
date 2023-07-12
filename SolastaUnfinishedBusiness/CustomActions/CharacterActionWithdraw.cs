﻿using System.Collections;
using JetBrains.Annotations;
using static RuleDefinitions;

//This should have default namespace so that it can be properly created by `CharacterActionPatcher`
// ReSharper disable once CheckNamespace
[UsedImplicitly]
#pragma warning disable CA1050
public class CharacterActionWithdraw : CharacterAction
#pragma warning restore CA1050
{
    private bool wasAlreadyDisengaging;

    public CharacterActionWithdraw(CharacterActionParams actionParams) : base(actionParams)
    {
    }

    public override IEnumerator ExecuteImpl()
    {
        var rulesetCharacter = ActingCharacter.RulesetCharacter;

        wasAlreadyDisengaging = rulesetCharacter.HasConditionOfType(ConditionDisengaging);

        if (!wasAlreadyDisengaging)
        {
            rulesetCharacter.InflictCondition(
                ConditionDisengaging,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagCombat,
                ActingCharacter.RulesetCharacter.Guid,
                ActingCharacter.RulesetCharacter.CurrentFaction.Name,
                0,
                string.Empty,
                0,
                0,
                0);
        }

        var myActionParams = new CharacterActionParams(
            ActingCharacter,
            ActionDefinitions.Id.ExplorationMove,
            ActingCharacter.GetMoveStance(),
            ActionParams.Positions[0],
            ActionParams.Orientation);

        ServiceRepository.GetService<IGameLocationActionService>()
            .ExecuteActionChain(myActionParams, MoveComplete, true);

        yield return null;
    }

    private void MoveComplete(bool aborted)
    {
        var rulesetCharacter = ActingCharacter.RulesetCharacter;

        rulesetCharacter.RemoveAllConditionsOfCategoryAndType(AttributeDefinitions.TagEffect,
            "ConditionRogueCunningStrikeWithdraw");

        if (!wasAlreadyDisengaging)
        {
            rulesetCharacter.RemoveAllConditionsOfCategoryAndType(AttributeDefinitions.TagCombat, ConditionDisengaging);
        }
    }
}
