using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Lykke.Service.BlockchainApi.Sdk
{
    internal class ApplicationFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public ApplicationFeatureProvider(TypeInfo[] controllerTypes) => 
            ControllerTypes = controllerTypes;

        public TypeInfo[] ControllerTypes { get; }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var type in ControllerTypes)
            {
                feature.Controllers.Add(type);
            }
        }
    }
}