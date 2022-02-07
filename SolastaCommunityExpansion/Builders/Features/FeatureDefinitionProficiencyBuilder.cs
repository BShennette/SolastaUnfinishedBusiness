﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.Builders.Features
{
    public class FeatureDefinitionProficiencyBuilder : BaseDefinitionBuilder<FeatureDefinitionProficiency>
    {
        // TODO: remove these ctors
        public FeatureDefinitionProficiencyBuilder(string name, string guid, RuleDefinitions.ProficiencyType type,
            IEnumerable<string> proficiencies, GuiPresentation guiPresentation) : base(name, guid, guiPresentation)
        {
            Definition.SetProficiencyType(type);
            Definition.Proficiencies.AddRange(proficiencies);
        }

        public FeatureDefinitionProficiencyBuilder(string name, Guid namespaceGuid,
            RuleDefinitions.ProficiencyType type, IEnumerable<string> proficiencies) : base(name, namespaceGuid)
        {
            Definition.SetProficiencyType(type);
            Definition.Proficiencies.AddRange(proficiencies);
        }

        public FeatureDefinitionProficiencyBuilder(string name, Guid namespaceGuid,
            RuleDefinitions.ProficiencyType type, params string[] proficiencies) : this(name, namespaceGuid, type, proficiencies.AsEnumerable())
        {
        }
        //-- to here

        public FeatureDefinitionProficiencyBuilder(string name, string guid)
            : base(name, guid)
        {
        }

        public FeatureDefinitionProficiencyBuilder(string name, Guid namespaceGuid)
            : base(name, namespaceGuid)
        {
        }

        public FeatureDefinitionProficiencyBuilder(FeatureDefinitionProficiency original, string name, string guid)
            : base(original, name, guid)
        {
        }

        public FeatureDefinitionProficiencyBuilder(FeatureDefinitionProficiency original, string name, Guid namespaceGuid)
            : base(original, name, namespaceGuid)
        {
        }

        public static FeatureDefinitionProficiencyBuilder Create(FeatureDefinitionProficiency original, string name, string guid)
        {
            return new FeatureDefinitionProficiencyBuilder(original, name, guid);
        }

        public static FeatureDefinitionProficiencyBuilder Create(string name, Guid namespaceGuid)
        {
            return new FeatureDefinitionProficiencyBuilder(name, namespaceGuid);
        }

        public FeatureDefinitionProficiencyBuilder SetProficiencies(RuleDefinitions.ProficiencyType type, params string[] proficiencies)
        {
            return SetProficiencies(type, proficiencies.AsEnumerable());
        }

        public FeatureDefinitionProficiencyBuilder SetProficiencies(RuleDefinitions.ProficiencyType type, IEnumerable<string> proficiencies)
        {
            Definition.SetProficiencyType(type);
            Definition.Proficiencies.SetRange(proficiencies);
            return this;
        }
    }
}
