﻿using System;
using System.Collections.Generic;
using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Features
{
    public class FeatureDefinitionProficiencyBuilder : BaseDefinitionBuilder<FeatureDefinitionProficiency>
    {
        public FeatureDefinitionProficiencyBuilder(string name, string guid, RuleDefinitions.ProficiencyType type,
            IEnumerable<string> proficiencies, GuiPresentation guiPresentation) : base(name, guid, guiPresentation)
        {
            Definition.SetProficiencyType(type);
            Definition.Proficiencies.AddRange(proficiencies);
        }

        public FeatureDefinitionProficiencyBuilder(string name, Guid namespaceGuid,
            RuleDefinitions.ProficiencyType type, IEnumerable<string> proficiencies, string keyPrefix) : base(name, namespaceGuid, keyPrefix)
        {
            Definition.SetProficiencyType(type);
            Definition.Proficiencies.AddRange(proficiencies);
        }
    }
}
