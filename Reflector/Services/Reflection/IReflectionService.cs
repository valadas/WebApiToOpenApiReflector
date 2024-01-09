// Copyright (c) Daniel Valadas. All rights reserved. MIT License

namespace Reflector.Services.Reflection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides services related to reflection.
    /// </summary>
    public interface IReflectionService
    {
        /// <summary>
        /// Gets the API controllers in an assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>The list of controllers.</returns>
        IEnumerable<Type> GetControllers(string assemblyPath);
    }
}
