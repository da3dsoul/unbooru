using unbooru.Abstractions.Enums;
using JetBrains.Annotations;

namespace unbooru.Abstractions.Attributes
{
    [MeansImplicitUse]
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ModulePostConfigurationAttribute : System.Attribute
    {
        public ModuleInitializationPriority Priority { get; set; } = ModuleInitializationPriority.Metadata;
    }
}