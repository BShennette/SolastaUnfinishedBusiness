﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;

namespace SolastaCommunityExpansion.Patches.CustomFeatures.CustomEffectForm;

[HarmonyPatch(typeof(RulesetImplementationManager), "ApplyEffectForms")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class RulesetImplementationManager_ApplyEffectForms
{
    public static void Postfix([NotNull] List<EffectForm> effectForms,
        RulesetImplementationDefinitions.ApplyFormsParams formsParams,
        bool retargeting = false,
        bool proxyOnly = false,
        bool forceSelfConditionOnly = false,
        RuleDefinitions.EffectApplication effectApplication = RuleDefinitions.EffectApplication.All,
        [CanBeNull] List<EffectFormFilter> filters = null)
    {
        foreach (var customEffect in effectForms.OfType<CustomDefinitions.CustomEffectForm>())
        {
            customEffect.ApplyForm(formsParams, retargeting, proxyOnly, forceSelfConditionOnly, effectApplication,
                filters);
        }
    }
}
