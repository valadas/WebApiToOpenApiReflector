// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Tool
{
    using System.Threading.Tasks;
    using Cocona;
    using Microsoft.Extensions.DependencyInjection;
    using Reflector.Services.Generator;
    using Reflector.Services.Reflection;

    /// <summary>
    /// Main program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments passed in the cli.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task Main(string[] args)
        {
            var builder = CoconaApp.CreateBuilder(args);
            builder.Services.AddSingleton<IReflectionService, ReflectionService>();
            builder.Services.AddSingleton<IGeneratorService, GeneratorService>();
            var app = builder.Build();

            app.AddCommands<Commands>();

            await app.RunAsync();
        }
    }
}
