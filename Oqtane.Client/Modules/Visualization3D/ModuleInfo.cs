using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Modules.Visualization3D
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "3D Visualization",
            Description = "Provides 3D visualization capabilities with a C# wrapper for creating and manipulating 3D objects",
            Version = "1.0.0",
            ReleaseVersions = "1.0.0",
            Resources = new List<Resource>()
            {
                new Script("~/Module.js")
            }
        };
    }
}
