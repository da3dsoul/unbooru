using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SortParameters
{
    public class PixivIdSortParameter : SortParameter
    {
        public override Type[] Types { get; } = { typeof(ImageSource) };
        public override Expression<Func<SearchViewModel, object>> Selector { get; } = a => a.PixivSource.PostId;
    }
}
