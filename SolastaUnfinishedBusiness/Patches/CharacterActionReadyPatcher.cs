﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class CharacterActionReadyPatcher
{
    [HarmonyPatch(typeof(CharacterActionReady), nameof(CharacterActionReady.ExecuteImpl))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ExecuteImpl_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(
            [NotNull] CharacterActionReady __instance,
            [NotNull] ref IEnumerator __result)
        {
            //PATCH: Adds support for DontEndTurnAfterReady setting 
            if (!Main.Settings.DontEndTurnAfterReady)
            {
                return true;
            }

            __result = Execute(__instance);
            return false;
        }

        private static IEnumerator Execute(CharacterActionReady action)
        {
            if (!ServiceRepository.GetService<IGameLocationBattleService>().IsBattleInProgress)
            {
                yield break;
            }


            action.ActingCharacter.ReadiedAction = action.readyActionType;
        }
    }
}
