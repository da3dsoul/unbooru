// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace unbooru.Abstractions.Poco
{
    public class RelatedImage
    {
        public int RelatedImageId { get; set; }
        
        public virtual Image Image { get; set; }
        public virtual Image Relation { get; set; } 
    }
}
