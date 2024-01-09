// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Reflector.Services.Generator
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
        /// <returns>The JSON result of the generation.</returns>
        Task<string> GenerateOpenApiAsync(
            List<string> assemblyPath,
            List<string> controllerNames,
            string defaultUrlTemplate = "api/{controller}/{id}",
            bool addMissingPathParameters = false);
    }
}
