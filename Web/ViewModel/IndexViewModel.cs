using System.Collections.Generic;
using unbooru.Abstractions.Poco;

namespace unbooru.Web.ViewModel
{
    public class IndexViewModel
    {
        public IReadOnlyList<Image> Images { get; set; }
        public int ImagesPerPage { get; set; }
        public int Page { get; set; }
        public int Pages { get; set; }
    }
}