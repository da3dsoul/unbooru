using System.Collections.Generic;
using unbooru.Abstractions.Poco;

namespace unbooru.Web.ViewModel
{
    public class SearchViewModel
    {
        public Image Image { get; set; }
        public IEnumerable<ImageTagSource> TagSources { get; set; }
        public IEnumerable<ArtistAccount> ArtistAccounts { get; set; }
        public ImageSource PixivSource { get; set; }
        public ImageBlob Blob { get; set; }
    }
}
