namespace unbooru.Abstractions.Poco;

public class ImageTagSource
{
    public int ImagesImageId { get; set; }
    public int TagsImageTagId { get; set; }
    public string Source { get; set; }
    
    public virtual Image Image { get; set; }
    public virtual ImageTag Tag { get; set; }
}