using System;
using JetBrains.Annotations;

namespace ImageInfrastructure.Abstractions.Poco.Ingest
{
    public record Post
    {
        [UsedImplicitly] public string Title { get; set; }
        [UsedImplicitly] public string Description { get; set; }
        [UsedImplicitly] public DateTime? PostDate { get; set; }
        [UsedImplicitly] public string ArtistName { get; set; }
        [UsedImplicitly] public string ArtistUrl { get; set; }
    }
}