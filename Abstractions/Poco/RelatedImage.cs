namespace ImageInfrastructure.Abstractions.Poco
{
    public class RelatedImage
    {
        public int RelatedImageId { get; set; }
        
        public Image Image { get; set; }
        public ImageSource ImageSource { get; set; } 
    }
}