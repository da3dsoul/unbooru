using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace unbooru.Abstractions.Poco.Ingest
{
    public record Post
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? PostDate { get; set; }
        public string ArtistName { get; set; }
        public string ArtistUrl { get; set; }
    }
}