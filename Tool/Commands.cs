// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Tool
{
    using Cocona;
    using Microsoft.Extensions.Logging;
    using Reflector.Services.Generator;

    /// <summary>
    /// Contains the commands that can be executed by the tool.
    /// </summary>
    public class Commands
    {
        private readonly ILogger<Commands> logger;
        private readonly IGeneratorService generatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Commands"/> class.
        /// </summary>
        /// <param name="logger">Allows logging.</param>
        /// <param name="generatorService">The service used to generate the Swagger specifications.</param>
        public Commands(
            ILogger<Commands> logger,
            IGeneratorService generatorService)
        {
            this.logger = logger;
            this.generatorService = generatorService;
        }

        /// <summary>
        /// Extracts Swagger specifications from assemblies using reflection.
        /// </summary>
        /// <param name="assemblyPaths">The assembly or assemblies to process.</param>
        /// <param name="controllerNames">Can optionally be used to limit the results to a single controller.</param>
        /// <param name="defaultUrlTemplate">The Web API default URL template: (default for Web API: 'api/{controller}/{id}'; (default for MVC: '{controller}/{action}/{id?}').</param>
        /// <param name="addMissingPathParameters">If true, adds missing path parameters which are missing in the action method.</param>
        /// <returns>An awaitable task.</returns>
        [PrimaryCommand]
        [Command(Description = "Generates a Swagger/OpenAPI specification for a controller or controllers contained in a .NET Web API assembly.")]
        public async Task WebApiToSwagger(
            [Argument(Description = "The assembly or assemblies to process.")]List<string> assemblyPaths,
            [Option('c', Description = "Can optionally be used to limit the results to some controllers.")] List<string>? controllerNames,
            [Option('u', Description = "The Web API default URL template: (default for Web API: 'api/{controller}/{id}'; (default for MVC: '{controller}/{action}/{id?}').")] string defaultUrlTemplate = "api/{controller}/{id}",
            [Option('a', Description = "If true, adds missing path parameters which are missing in the action method.")] bool addMissingPathParameters = false)
        {
            this.logger.LogInformation("Generating...");
            var json = await this.generatorService.GenerateOpenApiAsync(
                assemblyPaths,
                controllerNames,
                defaultUrlTemplate,
                addMissingPathParameters);
            Console.WriteLine(json);
        }
    }
}
