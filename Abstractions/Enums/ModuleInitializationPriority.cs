namespace ImageInfrastructure.Abstractions.Enums
{
    public enum ModuleInitializationPriority
    {
        PreDatabase,
        Database,
        SourceData,
        Metadata,
        PostProcess,
        Saving
    }
}