using ImageInfrastructure.Abstractions.Enums;
using JetBrains.Annotations;

namespace ImageInfrastructure.Abstractions.Attributes
{
    [MeansImplicitUse]
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ModulePostConfigurationAttribute : System.Attribute
    {
        public ModuleInitializationPriority Priority { get; set; } = ModuleInitializationPriority.Normal;
    }
}