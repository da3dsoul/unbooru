using System;

namespace unbooru.Abstractions.Poco;

public class ImageTagSource
{
    public int ImagesImageId { get; set; }
    public int TagsImageTagId { get; set; }
    public string Source { get; set; }
    
    public virtual Image Image { get; set; }
    public virtual ImageTag Tag { get; set; }

    protected bool Equals(ImageTagSource other)
    {
        return ImagesImageId == other.ImagesImageId && TagsImageTagId == other.TagsImageTagId && string.Equals(Source, other.Source, StringComparison.InvariantCultureIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ImageTagSource)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = ImagesImageId;
            hashCode = (hashCode * 397) ^ TagsImageTagId;
            hashCode = (hashCode * 397) ^ (Source != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Source) : 0);
            return hashCode;
        }
    }

    public static bool operator ==(ImageTagSource left, ImageTagSource right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ImageTagSource left, ImageTagSource right)
    {
        return !Equals(left, right);
    }
}