using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ImageInfrastructure.Abstractions.Poco
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
        
        public virtual List<ArtistAccount> ArtistAccounts { get; set; }

        public virtual List<ImageSource> Sources { get; set; }
        public virtual List<ImageTag> Tags { get; set; }

        [IgnoreDataMember]
        public virtual List<RelatedImage> RelatedImages { get; set; }
    }
}
