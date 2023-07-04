﻿using System.Collections;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.CustomInterfaces;

// triggers on any magical attack regardless of an attack roll or not
public interface IMagicalAttackFinishedByMeOrAlly
{
    [UsedImplicitly]
    public IEnumerator OnMagicalAttackFinishedByMeOrAlly(
        CharacterActionMagicEffect action,
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        GameLocationCharacter ally);
}
