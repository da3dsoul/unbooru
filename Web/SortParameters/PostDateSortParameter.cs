using System;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SortParameters
{
    public class PostDateSortParameter : SortParameter
    {
        public override Type[] Types { get; } = { typeof(ImageSource) };
        public override Expression<Func<SearchViewModel, object>> Selector { get; } = a => a.PixivSource.PostDate;
    }
}
