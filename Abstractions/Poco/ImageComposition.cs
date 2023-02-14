using System.Collections.Generic;
namespace unbooru.Abstractions.Poco;

public class ImageComposition
{
    public int ImageCompositionId { get; set; }
    public int ImageId { get; set; }
    public Image Image { get; set; }
    public List<ImageHistogramColor> Histogram { get; set; }
    public bool IsGrayscale { get; set; }
    public bool IsBlackAndWhite { get; set; }
    public bool IsMonochrome { get; set; }
}
