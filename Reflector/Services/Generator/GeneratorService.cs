﻿// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Reflector.Services.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using NSwag.Generation.WebApi;
    using Reflector.Services.Reflection;

    /// <summary>
    /// Service for generating specifications.
    /// </summary>
    public class GeneratorService : IGeneratorService
    {
        private readonly IReflectionService reflectionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorService"/> class.
        /// </summary>
        /// <param name="reflectionService">Provides services related to reflection.</param>
        public GeneratorService(IReflectionService reflectionService)
        {
            this.reflectionService = reflectionService;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateOpenApiAsync(
            List<string> assemblyPaths,
            List<string> controllerNames,
            string defaultUrlTemplate,
            bool addMissingPathParameters = false)
        {
            var controllers = Enumerable.Empty<Type>();
            foreach (var assemblyPath in assemblyPaths)
            {
                controllers = controllers.Concat(this.reflectionService.GetControllers(assemblyPath));
            }

            if (controllerNames != null && controllerNames.Any())
            {
                controllers = controllers.Where(c => ShouldIncludeController(c, controllerNames));
            }

            var settings = new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = defaultUrlTemplate ?? "api/{controller}/{id}",
                AddMissingPathParameters = addMissingPathParameters,
            };
            var generator = new WebApiOpenApiDocumentGenerator(settings);
            var document = await generator.GenerateForControllersAsync(controllers);
            return document.ToJson();
        }

        private static bool ShouldIncludeController(Type c, List<string> controllerNames)
        {
            foreach (var controllerName in controllerNames)
            {
                if (c.Name == controllerName)
                {
                    return true;
                }

                if (controllerName.IndexOf("*") > -1)
                {
                    var pattern = controllerName.Replace("*", ".*");
                    if (Regex.IsMatch(c.Name, pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
