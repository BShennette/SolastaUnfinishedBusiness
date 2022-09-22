﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

internal static class UserLocationViewPanelPatcher
{
    [HarmonyPatch(typeof(UserLocationViewPanel), "PropOverlap", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class UserLocationViewPanel_PropOverlap_Getter
    {
        internal static void Postfix(ref bool __result)
        {
            //PATCH: Bypasses prop overlap check if CTRL is pressed (DMP)
            var isCtrlPressed = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);

            if (Main.Settings.AllowGadgetsAndPropsToBePlacedAnywhere && isCtrlPressed)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(UserLocationViewPanel), "GadgetOverlap", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class UserLocationViewPanel_GadgetOverlap_Getter
    {
        internal static void Postfix(ref bool __result)
        {
            //PATCH: Bypasses gadget overlap check if CTRL is pressed (DMP)
            var isCtrlPressed = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);

            if (Main.Settings.AllowGadgetsAndPropsToBePlacedAnywhere && isCtrlPressed)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(UserLocationViewPanel), "PropInvalidPlacement", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class UserLocationViewPanel_PropInvalidPlacement_Getter
    {
        internal static void Postfix(ref bool __result)
        {
            //PATCH: Bypasses prop invalid check if CTRL is pressed (DMP)
            var isCtrlPressed = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);

            if (Main.Settings.AllowGadgetsAndPropsToBePlacedAnywhere && isCtrlPressed)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(UserLocationViewPanel), "GadgetInvalidPlacement", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class UserLocationViewPanel_GadgetInvalidPlacement_Getter
    {
        internal static void Postfix(ref bool __result)
        {
            //PATCH: Bypasses gadget invalid check if CTRL is pressed (DMP)
            var isCtrlPressed = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);

            if (Main.Settings.AllowGadgetsAndPropsToBePlacedAnywhere && isCtrlPressed)
            {
                __result = false;
            }
        }
    }
}
