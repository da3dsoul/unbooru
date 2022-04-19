using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace unbooru.Abstractions.Poco
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class ArtistAccount
    {
        public int ArtistAccountId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        
        [IgnoreDataMember]
        public byte[] Avatar { get; set; }
        
        [IgnoreDataMember]
        public virtual List<Image> Images { get; set; }

        protected bool Equals(ArtistAccount other)
        {
            return Id == other.Id && Name == other.Name && Url == other.Url;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArtistAccount) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id != null ? Id.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Url != null ? Url.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ArtistAccount left, ArtistAccount right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ArtistAccount left, ArtistAccount right)
        {
            return !Equals(left, right);
        }
    }
}