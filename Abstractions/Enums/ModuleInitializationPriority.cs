namespace ImageInfrastructure.Abstractions.Enums
{
    public enum ModuleInitializationPriority
    {
        PreProcessing,
        SourceData,
        Metadata,
        PostProcess,
        Database,
        Saving
    }
}