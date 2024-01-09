// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Reflector.Services.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides services related to reflection.
    /// </summary>
    public class ReflectionService : IReflectionService
    {
        /// <inheritdoc/>
        public IEnumerable<Type> GetControllers(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var controllers = assembly
                .GetTypes()
                .Where(t => IsController(t));
            foreach (var controller in controllers)
            {
                Console.WriteLine(controller.Name);
            }

            return controllers;
        }

        private static bool IsController(Type type)
        {
            if (!type.Name.Contains("Controller"))
            {
                return false;
            }

            if (type.IsAbstract)
            {
                return false;
            }

            if (InheritsFromByName(type, "ApiController"))
            {
                return true;
            }

            return false;
        }

        private static bool InheritsFromByName(Type type, string baseTypeName)
        {
            while (type != null && type != typeof(object))
            {
                if (type.Name == baseTypeName || type.FullName == baseTypeName)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
