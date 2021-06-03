namespace ImageInfrastructure.Abstractions.Poco
{
    public class RelatedImage
    {
        public int RelatedImageId { get; set; }
        
        public virtual Image Image { get; set; }
        public virtual ImageSource ImageSource { get; set; } 
    }
}