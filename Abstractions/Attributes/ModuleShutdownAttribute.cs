using JetBrains.Annotations;

namespace unbooru.Abstractions.Attributes
{
    [MeansImplicitUse]
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ModuleShutdownAttribute : System.Attribute
    {
    }
}