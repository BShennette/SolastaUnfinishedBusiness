﻿using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Displays;

internal static class RulesDisplay
{
    private static readonly string[] Options = { "0", "1", "2", "3" };

    internal static void DisplayRules()
    {
        UI.Label();
        UI.Label(Gui.Localize("ModUi/&SRD"));
        UI.Label();

        var toggle = Main.Settings.UseOfficialAdvantageDisadvantageRules;
        if (UI.Toggle(Gui.Localize("ModUi/&UseOfficialAdvantageDisadvantageRules"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UseOfficialAdvantageDisadvantageRules = toggle;
            Main.Settings.UseOfficialFlankingRulesAlsoForRanged = false;
        }

        toggle = Main.Settings.FixEldritchBlastRange;
        if (UI.Toggle(Gui.Localize("ModUi/&FixEldritchBlastRange"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.FixEldritchBlastRange = toggle;
            SrdAndHouseRulesContext.SwitchEldritchBlastRange();
        }

        toggle = Main.Settings.UseOfficialFlankingRules;
        if (UI.Toggle(Gui.Localize("ModUi/&UseOfficialFlankingRules"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UseOfficialFlankingRules = toggle;
        }

        if (Main.Settings.UseOfficialFlankingRules)
        {
            toggle = Main.Settings.UseMathFlankingRules;
            if (UI.Toggle(Gui.Localize("ModUi/&UseMathFlankingRules"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.UseMathFlankingRules = toggle;
            }

            toggle = Main.Settings.UseOfficialFlankingRulesAlsoForReach;
            if (UI.Toggle(Gui.Localize("ModUi/&UseOfficialFlankingRulesAlsoForReach"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.UseOfficialFlankingRulesAlsoForReach = toggle;
            }

            toggle = Main.Settings.UseOfficialFlankingRulesAlsoForRanged;
            if (UI.Toggle(Gui.Localize("ModUi/&UseOfficialFlankingRulesAlsoForRanged"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.UseOfficialFlankingRulesAlsoForRanged = toggle;
            }

            toggle = Main.Settings.UseOfficialFlankingRulesButAddAttackModifier;
            if (UI.Toggle(Gui.Localize("ModUi/&UseOfficialFlankingRulesButAddAttackModifier"), ref toggle,
                    UI.AutoWidth()))
            {
                Main.Settings.UseOfficialFlankingRulesButAddAttackModifier = toggle;
            }
        }

        toggle = Main.Settings.UseOfficialFoodRationsWeight;
        if (UI.Toggle(Gui.Localize("ModUi/&UseOfficialFoodRationsWeight"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UseOfficialFoodRationsWeight = toggle;
            SrdAndHouseRulesContext.ApplySrdWeightToFoodRations();
        }

        toggle = Main.Settings.UseOfficialDistanceCalculation;
        if (UI.Toggle(Gui.Localize("ModUi/&UseOfficialDistanceCalculation"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UseOfficialDistanceCalculation = toggle;
        }

        toggle = Main.Settings.DontEndTurnAfterReady;
        if (UI.Toggle(Gui.Localize("ModUi/&DontEndTurnAfterReady"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.DontEndTurnAfterReady = toggle;
        }

        UI.Label();

        toggle = Main.Settings.AttackersWithDarkvisionHaveAdvantageOverDefendersWithout;
        if (UI.Toggle(Gui.Localize("ModUi/&AttackersWithDarkvisionHaveAdvantageOverDefendersWithout"), ref toggle,
                UI.AutoWidth()))
        {
            Main.Settings.AttackersWithDarkvisionHaveAdvantageOverDefendersWithout = toggle;
            SrdAndHouseRulesContext.SwitchDarknessSpell();
        }

        toggle = Main.Settings.BlindedConditionDontAllowAttackOfOpportunity;
        if (UI.Toggle(Gui.Localize("ModUi/&BlindedConditionDontAllowAttackOfOpportunity"), ref toggle,
                UI.AutoWidth()))
        {
            Main.Settings.BlindedConditionDontAllowAttackOfOpportunity = toggle;
            SrdAndHouseRulesContext.ApplyConditionBlindedShouldNotAllowOpportunityAttack();
        }

        UI.Label();

        toggle = Main.Settings.AllowTargetingSelectionWhenCastingChainLightningSpell;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowTargetingSelectionWhenCastingChainLightningSpell"), ref toggle,
                UI.AutoWidth()))
        {
            Main.Settings.AllowTargetingSelectionWhenCastingChainLightningSpell = toggle;
            SrdAndHouseRulesContext.AllowTargetingSelectionWhenCastingChainLightningSpell();
        }

        toggle = Main.Settings.RemoveHumanoidFilterOnHideousLaughter;
        if (UI.Toggle(Gui.Localize("ModUi/&RemoveHumanoidFilterOnHideousLaughter"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RemoveHumanoidFilterOnHideousLaughter = toggle;
            SrdAndHouseRulesContext.SwitchFilterOnHideousLaughter();
        }

        UI.Label();

        toggle = Main.Settings.AddBleedingToLesserRestoration;
        if (UI.Toggle(Gui.Localize("ModUi/&AddBleedingToLesserRestoration"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AddBleedingToLesserRestoration = toggle;
            SrdAndHouseRulesContext.AddBleedingToRestoration();
        }

        toggle = Main.Settings.BestowCurseNoConcentrationRequiredForSlotLevel5OrAbove;
        if (UI.Toggle(Gui.Localize("ModUi/&BestowCurseNoConcentrationRequiredForSlotLevel5OrAbove"), ref toggle,
                UI.AutoWidth()))
        {
            Main.Settings.BestowCurseNoConcentrationRequiredForSlotLevel5OrAbove = toggle;
        }

        toggle = Main.Settings.RemoveRecurringEffectOnEntangle;
        if (UI.Toggle(Gui.Localize("ModUi/&RemoveRecurringEffectOnEntangle"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RemoveRecurringEffectOnEntangle = toggle;
            SrdAndHouseRulesContext.SwitchRecurringEffectOnEntangle();
        }

        toggle = Main.Settings.EnableUpcastConjureElementalAndFey;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableUpcastConjureElementalAndFey"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableUpcastConjureElementalAndFey = toggle;
            Main.Settings.OnlyShowMostPowerfulUpcastConjuredElementalOrFey = false;
            SrdAndHouseRulesContext.SwitchEnableUpcastConjureElementalAndFey();
        }

        if (Main.Settings.EnableUpcastConjureElementalAndFey)
        {
            toggle = Main.Settings.OnlyShowMostPowerfulUpcastConjuredElementalOrFey;
            if (UI.Toggle(Gui.Localize("ModUi/&OnlyShowMostPowerfulUpcastConjuredElementalOrFey"), ref toggle,
                    UI.AutoWidth()))
            {
                Main.Settings.OnlyShowMostPowerfulUpcastConjuredElementalOrFey = toggle;
            }
        }

        UI.Label();

        toggle = Main.Settings.ChangeSleetStormToCube;
        if (UI.Toggle(Gui.Localize("ModUi/&ChangeSleetStormToCube"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.ChangeSleetStormToCube = toggle;
            SrdAndHouseRulesContext.UseCubeOnSleetStorm();
        }

        toggle = Main.Settings.UseHeightOneCylinderEffect;
        if (UI.Toggle(Gui.Localize("ModUi/&UseHeightOneCylinderEffect"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.UseHeightOneCylinderEffect = toggle;
            SrdAndHouseRulesContext.UseHeightOneCylinderEffect();
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&House"));
        UI.Label();

        toggle = Main.Settings.AllowAnyClassToUseArcaneShieldstaff;
        if (UI.Toggle(Gui.Localize("ModUi/&ArcaneShieldstaffOptions"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowAnyClassToUseArcaneShieldstaff = toggle;
            ItemCraftingMerchantContext.SwitchAttuneArcaneShieldstaff();
        }
        
        toggle = Main.Settings.IdentifyAfterRest;
        if (UI.Toggle(Gui.Localize("ModUi/&IdentifyAfterRest"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.IdentifyAfterRest = toggle;
        }

        toggle = Main.Settings.IncreaseMaxAttunedItems;
        if (UI.Toggle(Gui.Localize("ModUi/&IncreaseMaxAttunedItems"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.IncreaseMaxAttunedItems = toggle;
        }

        toggle = Main.Settings.RemoveAttunementRequirements;
        if (UI.Toggle(Gui.Localize("ModUi/&RemoveAttunementRequirements"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.RemoveAttunementRequirements = toggle;
        }
        
        UI.Label();

        toggle = Main.Settings.AllowHasteCasting;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowHasteCasting"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowHasteCasting = toggle;
            SrdAndHouseRulesContext.SwitchHastedCasing();
        }

        toggle = Main.Settings.AllowStackedMaterialComponent;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowStackedMaterialComponent"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowStackedMaterialComponent = toggle;
        }

        toggle = Main.Settings.EnableCantripsTriggeringOnWarMagic;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableCantripsTriggeringOnWarMagic"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableCantripsTriggeringOnWarMagic = toggle;
        }

        UI.Label();

        toggle = Main.Settings.AllowAnyClassToWearSylvanArmor;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowAnyClassToWearSylvanArmor"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowAnyClassToWearSylvanArmor = toggle;
            SrdAndHouseRulesContext.SwitchUniversalSylvanArmorAndLightbringer();
        }

        toggle = Main.Settings.AllowDruidToWearMetalArmor;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowDruidToWearMetalArmor"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowDruidToWearMetalArmor = toggle;
            SrdAndHouseRulesContext.SwitchDruidAllowMetalArmor();
        }

        toggle = Main.Settings.AllowClubsToBeThrown;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowClubsToBeThrown"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowClubsToBeThrown = toggle;
            SrdAndHouseRulesContext.SwitchAllowClubsToBeThrown();
        }

        toggle = Main.Settings.IgnoreHandXbowFreeHandRequirements;
        if (UI.Toggle(Gui.Localize("ModUi/&IgnoreHandXbowFreeHandRequirements"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.IgnoreHandXbowFreeHandRequirements = toggle;
        }

        toggle = Main.Settings.MakeAllMagicStaveArcaneFoci;
        if (UI.Toggle(Gui.Localize("ModUi/&MakeAllMagicStaveArcaneFoci"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.MakeAllMagicStaveArcaneFoci = toggle;
            SrdAndHouseRulesContext.SwitchMagicStaffFoci();
        }

        UI.Label();

        toggle = Main.Settings.AccountForAllDiceOnSavageAttack;
        if (UI.Toggle(Gui.Localize("ModUi/&AccountForAllDiceOnSavageAttack"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AccountForAllDiceOnSavageAttack = toggle;
        }

        toggle = Main.Settings.AllowFlightSuspend;
        if (UI.Toggle(Gui.Localize("ModUi/&AllowFlightSuspend"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.AllowFlightSuspend = toggle;
        }

        if (Main.Settings.AllowFlightSuspend)
        {
            toggle = Main.Settings.FlightSuspendWingedBoots;
            if (UI.Toggle(Gui.Localize("ModUi/&FlightSuspendWingedBoots"), ref toggle, UI.AutoWidth()))
            {
                Main.Settings.FlightSuspendWingedBoots = toggle;
            }
        }

        toggle = Main.Settings.EnableCharactersOnFireToEmitLight;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableCharactersOnFireToEmitLight"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableCharactersOnFireToEmitLight = toggle;
            SrdAndHouseRulesContext.SwitchMagicStaffFoci();
        }

        toggle = Main.Settings.EnableHigherGroundRules;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableHigherGroundRules"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHigherGroundRules = toggle;
        }

        toggle = Main.Settings.FullyControlConjurations;
        if (UI.Toggle(Gui.Localize("ModUi/&FullyControlConjurations"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.FullyControlConjurations = toggle;
            SrdAndHouseRulesContext.SwitchFullyControlConjurations();
        }

        UI.Label();

        var intValue = Main.Settings.IncreaseSenseNormalVision;
        UI.Label(Gui.Localize("ModUi/&IncreaseSenseNormalVision"));
        if (UI.Slider(Gui.Localize("ModUi/&IncreaseSenseNormalVisionHelp"),
                ref intValue,
                SrdAndHouseRulesContext.DefaultVisionRange,
                SrdAndHouseRulesContext.MaxVisionRange,
                SrdAndHouseRulesContext.DefaultVisionRange, "", UI.AutoWidth()))
        {
            Main.Settings.IncreaseSenseNormalVision = intValue;
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Critical"));
        UI.Label();

        UI.Label(Gui.Localize("ModUi/&CriticalOption0"));
        UI.Label(Gui.Localize("ModUi/&CriticalOption1"));
        UI.Label(Gui.Localize("ModUi/&CriticalOption2"));
        UI.Label(Gui.Localize("ModUi/&CriticalOption3"));
        UI.Label();

        using (UI.HorizontalScope())
        {
            UI.Label(Gui.Localize("Caption/&TargetFilteringAllyCreature"), UI.Width((float)100));

            intValue = Main.Settings.CriticalHitModeAllies;
            if (UI.SelectionGrid(ref intValue, Options, Options.Length, 4, UI.Width((float)220)))
            {
                Main.Settings.CriticalHitModeAllies = intValue;
            }
        }

        using (UI.HorizontalScope())
        {
            UI.Label(Gui.Localize("Caption/&TargetFilteringEnemyCreature"), UI.Width((float)100));

            intValue = Main.Settings.CriticalHitModeEnemies;
            if (UI.SelectionGrid(ref intValue, Options, Options.Length, 4, UI.Width((float)220)))
            {
                Main.Settings.CriticalHitModeEnemies = intValue;
            }
        }

        using (UI.HorizontalScope())
        {
            UI.Label(Gui.Localize("Action/&NeutralCreatureTitle"), UI.Width((float)100));

            intValue = Main.Settings.CriticalHitModeNeutral;
            if (UI.SelectionGrid(ref intValue, Options, Options.Length, 4, UI.Width((float)220)))
            {
                Main.Settings.CriticalHitModeNeutral = intValue;
            }
        }

        UI.Label();
    }
}
