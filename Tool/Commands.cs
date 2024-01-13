// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Tool
{
    using Cocona;
    using Microsoft.Extensions.Logging;
    using NJsonSchema.Generation;
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
        /// <param name="defaultResponseReferenceTypeNullHandling">Specifies the default null handling for reference types when no nullability information is available. NotNull (default) or Null.</param>
        /// <param name="generateOriginalParameterNames">Generate x-originalName properties when parameter name is different in .NET and HTTP. (default: true).</param>
        /// <param name="title">Specifies the title of the Swagger specification.</param>
        /// <param name="description">Specifies the description of the Swagger specification.</param>
        /// <param name="infoVersion">Specifies the version of the Swagger specification (default: 1.0.0).</param>
        /// <param name="documentTemplate">Specifies the Swagger document template (may be a path or JSON, default: none).</param>
        /// <param name="output">The path to the file to write the specifications to.</param>
        /// <returns>An awaitable task.</returns>
        [PrimaryCommand]
        [Command(Description = "Generates a Swagger/OpenAPI specification for a controller or controllers contained in a .NET Web API assembly.")]
        public async Task WebApiToSwagger(
            [Argument(Description = "The assembly or assemblies to process.")]List<string> assemblyPaths,
            [Option('c', Description = "Can optionally be used to limit the results to some controllers.")] List<string>? controllerNames,
            [Option('u', Description = "The Web API default URL template: (default for Web API: 'api/{controller}/{id}'; (default for MVC: '{controller}/{action}/{id?}').")] string defaultUrlTemplate = "api/{controller}/{id}",
            [Option('a', Description = "If true, adds missing path parameters which are missing in the action method.")] bool addMissingPathParameters = false,
            [Option('n', Description = "Specifies the default null handling for reference types when no nullability information is available. NotNull (default) or Null.")] ReferenceTypeNullHandling defaultResponseReferenceTypeNullHandling = ReferenceTypeNullHandling.Null,
            [Option('g', Description = "Generate x-originalName properties when parameter name is different in .NET and HTTP.")] bool generateOriginalParameterNames = true,
            [Option('t', Description = "Specifies the title of the Swagger specification, ignored when the document template is provided.")] string title = "",
            [Option('d', Description = "Specifies the description of the Swagger specification, ignored when the document template is provided.")] string description = "",
            [Option('v', Description = "Specifies the version of the Swagger specification (default: 1.0.0).")]string infoVersion = "1.0.0",
            [Option(Description = "Specifies the Swagger document template (may be a path or JSON, default: none).")] string documentTemplate = "",
            [Option('o', Description = "The path to the file to write the specifications to.")] string output = "swagger.json")
        {
            this.logger.LogInformation("Generating...");
            var json = await this.generatorService.GenerateOpenApiAsync(
                assemblyPaths,
                controllerNames,
                defaultUrlTemplate,
                addMissingPathParameters,
                defaultResponseReferenceTypeNullHandling,
                generateOriginalParameterNames,
                title,
                description,
                infoVersion,
                documentTemplate);

            // Write the output to the file.
            await File.WriteAllTextAsync(output, json);
        }
    }
}
