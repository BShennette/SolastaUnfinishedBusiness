﻿using System.Collections;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal class DoNotTerminateWhileUnconscious
{
    public static readonly DoNotTerminateWhileUnconscious Marker = new();

    private DoNotTerminateWhileUnconscious()
    {
    }

    internal static IEnumerator TerminateAllSpellsAndEffects(IEnumerator baseMethodCalls, RulesetCharacter character,
        bool wasConscious, bool stillConscious, bool massiveDamage)
    {
        if (!wasConscious || (stillConscious && !massiveDamage))
        {
            yield return baseMethodCalls;
            yield break;
        }

        //store and remove spell effects that don't need termination
        var spells = character.spellsCastByMe.Where(Match).ToList();
        character.spellsCastByMe.RemoveAll(Match);

        //store and remove power effects that don't need termination
        var powers = character.powersUsedByMe.Where(Match).ToList();
        character.powersUsedByMe.RemoveAll(Match);

        //call default method
        yield return baseMethodCalls;

        //restore stored spell and power effects
        character.spellsCastByMe.AddRange(spells);
        character.powersUsedByMe.AddRange(powers);
    }

    private static bool Match(RulesetEffect e)
    {
        return e.SourceDefinition.HasSubFeatureOfType<DoNotTerminateWhileUnconscious>();
    }
}
