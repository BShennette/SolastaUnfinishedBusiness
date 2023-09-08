﻿#if DEBUG
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.DataMiner.ItemDefinitionVerification;

namespace SolastaUnfinishedBusiness.Patches;

//PATCH: These patches are for item usage diagnostics
[UsedImplicitly]
public static class ItemDefinitionPatcher
{
    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.ArmorDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ArmorDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref ArmorDescription __result)
        {
            VerifyUsage(__instance, __instance.IsArmor, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.WeaponDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class WeaponDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref WeaponDescription __result)
        {
            VerifyUsage(__instance, __instance.IsWeapon, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.AmmunitionDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class AmmunitionDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref AmmunitionDescription __result)
        {
            VerifyUsage(__instance, __instance.IsAmmunition, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.UsableDeviceDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class UsableDeviceDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref UsableDeviceDescription __result)
        {
            VerifyUsage(__instance, __instance.IsUsableDevice, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.ToolDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ToolDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref ToolDescription __result)
        {
            VerifyUsage(__instance, __instance.IsTool, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.StarterPackDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class StarterPackDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref StarterPackDescription __result)
        {
            VerifyUsage(__instance, __instance.IsStarterPack, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.ContainerItemDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class ContainerItemDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref ContainerItemDescription __result)
        {
            VerifyUsage(__instance, __instance.IsContainerItem, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.LightSourceItemDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class LightSourceItemDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref LightSourceItemDescription __result)
        {
            VerifyUsage(__instance, __instance.IsLightSourceItem, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.FocusItemDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class FocusItemDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref FocusItemDescription __result)
        {
            VerifyUsage(__instance, __instance.IsFocusItem, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.WealthPileDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class WealthPileDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref WealthPileDescription __result)
        {
            VerifyUsage(__instance, __instance.IsWealthPile, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.SpellbookDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class SpellbookDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref SpellbookDescription __result)
        {
            VerifyUsage(__instance, __instance.IsSpellbook, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.FoodDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class FoodDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref FoodDescription __result)
        {
            VerifyUsage(__instance, __instance.IsFood, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.FactionRelicDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class FactionRelicDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref FactionRelicDescription __result)
        {
            VerifyUsage(__instance, __instance.IsFactionRelic, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), nameof(ItemDefinition.DocumentDescription), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class DocumentDescription_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(ItemDefinition __instance, ref DocumentDescription __result)
        {
            VerifyUsage(__instance, __instance.IsDocument, ref __result);
        }
    }
}
#endif
