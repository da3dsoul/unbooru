using System.ComponentModel.DataAnnotations.Schema;
namespace unbooru.Abstractions.Poco;

public class ImageHistogramColor
{
    public long ImageHistogramColorId { get; set; }
    public long ColorKey { get; set; }

    [NotMapped]
    public byte[] RGBA
    {
        get
        {
            var key = ColorKey << 32 >> 32;
            return new[]
            {
                (byte)(key >> 24),
                (byte)(key >> 16),
                (byte)(key >> 8),
                (byte)key
            };
        }
        set => ColorKey = value[0] << 24 | value[1] << 16 | value[2] << 8 | value[3];
    }

    public int Value { get; set; }

    public virtual ImageComposition Composition { get; set; }
}
