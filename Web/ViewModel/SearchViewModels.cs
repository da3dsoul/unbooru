using System.Collections.Generic;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.ViewModel
{
    public class SearchViewModel
    {
        public Image Image { get; set; }
        public IEnumerable<ImageTag> Tags { get; set; }
        public IEnumerable<ArtistAccount> ArtistAccounts { get; set; }
        public IEnumerable<ImageSource> Sources { get; set; }
        public ImageBlob Blob { get; set; }
    }
}