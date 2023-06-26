﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class InventoryShortcutsPanelPatcher
{
    [HarmonyPatch(typeof(InventoryShortcutsPanel), nameof(InventoryShortcutsPanel.OnConfigurationSwitched))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class OnConfigurationSwitched_Patch
    {
        [UsedImplicitly]
        public static void Prefix(ref int rank)
        {
            var isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (Main.Settings.EnableCtrlClickOnlySwapsMainHand && isCtrlPressed)
            {
                rank += 100;
            }
        }

        [UsedImplicitly]
        public static void Postfix(InventoryShortcutsPanel __instance, int rank)
        {
            if (rank < 100)
            {
                return;
            }

            rank -= 100;

            var itemsConfigurations = __instance.GuiCharacter.RulesetCharacterHero.CharacterInventory
                .WieldedItemsConfigurations;

            for (var index = 0; index < itemsConfigurations.Count; ++index)
            {
                __instance.configurationsTable.GetChild(index).GetComponent<WieldedConfigurationSelector>().Selected =
                    index == rank;
            }
        }
    }
}
