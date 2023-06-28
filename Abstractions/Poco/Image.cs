using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using MoreLinq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace unbooru.Abstractions.Poco
{
    public class Image
    {
        public int ImageId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public DateTime ImportDate { get; set; }
        public long Size { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public virtual byte[] Blob
        {
            get => Blobs[0].Data;
            set
            {
                if (Blobs == null)
                    Blobs = new List<ImageBlob> {new() { Image = this, Data = value}};
                else if(Blobs.Count == 0)
                    Blobs.Add(new ImageBlob { Image = this, Data = value});
                else
                {
                    Blobs[0].Data = value;
                }
            }
        }
        [IgnoreDataMember] public virtual List<ImageBlob> Blobs { get; set; }
        public virtual ImageComposition Composition { get; set; }

        public virtual List<ArtistAccount> ArtistAccounts { get; set; }

        public virtual List<ImageSource> Sources { get; set; }
        [IgnoreDataMember]
        [NotMapped]
        public IReadOnlyList<ImageTag> Tags => TagSources.Select(a => a.Tag).DistinctBy(a => a.ImageTagId).ToList();
        public virtual List<ImageTagSource> TagSources { get; set; }

        [IgnoreDataMember]
        public virtual List<RelatedImage> RelatedImages { get; set; }

        [NotMapped] public List<int> RelatedImageIds =>
            RelatedImages?.Select(a => a.Relation.ImageId).Where(a => a != ImageId).ToList() ?? new List<int>();
    }
}
