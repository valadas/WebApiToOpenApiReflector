// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Reflector.Services.Generator
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NJsonSchema.Generation;

    /// <summary>
    /// Service for generating specifications.
    /// </summary>
    public interface IGeneratorService
    {
        /// <summary>
        /// Generates the open API.
        /// </summary>
        /// <param name="assemblyPath">The assembly path to extract the controllers from.</param>
        /// <param name="controllerNames">If provided, limits the extraction to a some controllers.</param>
        /// <param name="defaultUrlTemplate">
        /// The Web API default URL template:
        /// (default for Web API: 'api/{controller}/{id}';
        /// (default for MVC: '{controller}/{action}/{id?}').
        /// </param>
        /// <param name="addMissingPathParameters">If true, adds missing path parameters which are missing in the action method.</param>
        /// <param name="defaultResponseReferenceTypeNullHandling">Specifies the default null handling for reference types when no nullability information is available.</param>
        /// <param name="generateOriginalParameterNames">Generate x-originalName properties when parameter name is different in .NET and HTTP.</param>
        /// <param name="title">Specifies the title of the Swagger specification.</param>
        /// <param name="description">Specifies the description of the Swagger specification.</param>
        /// <param name="version">Specifies the version of the Swagger specification (default: 1.0.0).</param>
        /// <param name="documentTemplate">Specifies the Swagger document template (may be a path or JSON, default: none).</param>
        /// <returns>The JSON result of the generation.</returns>
        Task<string> GenerateOpenApiAsync(
            List<string> assemblyPath,
            List<string> controllerNames,
            string defaultUrlTemplate = "api/{controller}/{id}",
            bool addMissingPathParameters = false,
            ReferenceTypeNullHandling defaultResponseReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
            bool generateOriginalParameterNames = true,
            string title = "",
            string description = "",
            string version = "1.0.0",
            string documentTemplate = "");
    }
}
