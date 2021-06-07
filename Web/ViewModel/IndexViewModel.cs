using System.Collections.Generic;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.ViewModel
{
    public class IndexViewModel
    {
        public IReadOnlyList<Image> Images { get; set; }
        public int ImagesPerPage { get; set; }
        public int Page { get; set; }
    }
}