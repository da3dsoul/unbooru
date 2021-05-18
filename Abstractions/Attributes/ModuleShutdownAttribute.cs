using JetBrains.Annotations;

namespace ImageInfrastructure.Abstractions.Attributes
{
    [MeansImplicitUse]
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ModuleShutdownAttribute : System.Attribute
    {
    }
}