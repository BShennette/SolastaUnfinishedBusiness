﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using static SolastaUnfinishedBusiness.Models.SaveByLocationContext;

namespace SolastaUnfinishedBusiness.Patches;

internal static class LoadPanelPatcher
{
    [HarmonyPatch(typeof(LoadPanel), "OnBeginShow")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class LoadPanel_OnBeginShow
    {
        public static bool Prefix([NotNull] LoadPanel __instance, [HarmonyArgument("instant")] bool _ = false)
        {
            //PATCH: EnableSaveByLocation
            if (Main.Settings.EnableSaveByLocation)
            {
                return LoadPanelOnBeginShowSaveByLocationBehavior(__instance);
            }

            if (Dropdown != null)
            {
                Dropdown.SetActive(false);
            }

            return true;
        }
    }
}
