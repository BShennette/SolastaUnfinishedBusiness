﻿using System.Collections;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.Models;

internal static class PowerBundleContext
{
    internal const string UseCustomRestPowerFunctorName = "UseCustomRestPower";

    internal static void Load()
    {
        ServiceRepository.GetService<IFunctorService>()
            .RegisterFunctor(UseCustomRestPowerFunctorName, new FunctorUseCustomRestPower());
    }

    private sealed class FunctorUseCustomRestPower : Functor
    {
        private bool powerUsed;

        public override IEnumerator Execute(
            [NotNull] FunctorParametersDescription functorParameters,
            FunctorExecutionContext context)
        {
            var functor = this;
            var powerName = functorParameters.StringParameter;
            var power = PowerBundle.GetPower(powerName);

            if (power == null && !DatabaseHelper.TryGetDefinition(powerName, out power))
            {
                yield break;
            }

            var ruleChar = functorParameters.RestingHero;
            var usablePower = UsablePowersProvider.Get(power, ruleChar);

            if (power.EffectDescription.TargetType == RuleDefinitions.TargetType.Self)
            {
                GameLocationCharacter fromActor = null;
                var retarget = power.GetFirstSubFeatureOfType<IRetargetCustomRestPower>();
                if (retarget != null)
                {
                    fromActor = retarget.GetTarget(ruleChar);
                }

                fromActor ??= GameLocationCharacter.GetFromActor(ruleChar);

                var rules = ServiceRepository.GetService<IRulesetImplementationService>();

                if (fromActor != null)
                {
                    functor.powerUsed = false;
                    ServiceRepository.GetService<IGameLocationActionService>();

                    var actionParams = new CharacterActionParams(fromActor, ActionDefinitions.Id.PowerMain);

                    actionParams.TargetCharacters.Add(fromActor);
                    actionParams.ActionModifiers.Add(new ActionModifier());
                    actionParams.RulesetEffect = rules
                            .InstantiateEffectPower(fromActor.RulesetCharacter, usablePower, true)
                            .AddAsActivePowerToSource();
                    actionParams.SkipAnimationsAndVFX = true;

                    ServiceRepository.GetService<ICommandService>()
                        .ExecuteAction(actionParams, functor.ActionExecuted, false);

                    while (!functor.powerUsed)
                    {
                        yield return null;
                    }
                }
                else
                {
                    var formsParams = new RulesetImplementationDefinitions.ApplyFormsParams();

                    formsParams.FillSourceAndTarget(ruleChar, ruleChar);
                    formsParams.FillFromActiveEffect(rules
                        .InstantiateEffectPower(ruleChar, usablePower, false)
                        .AddAsActivePowerToSource());
                    formsParams.effectSourceType = RuleDefinitions.EffectSourceType.Power;

                    //rules.ApplyEffectForms(power.EffectDescription.EffectForms, formsParams);
                    ruleChar.UpdateUsageForPower(usablePower, power.CostPerUse);

                    ruleChar.LogCharacterUsedPower(power, $"Feedback/&{power.Name}UsedWhileTravellingFormat");
                }
            }

            Trace.LogWarning("Unable to assign targets to power");
        }

        private void ActionExecuted(CharacterAction action)
        {
            powerUsed = true;
        }
    }
}
