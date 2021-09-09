using System;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SortParameters
{
    public class AspectRatioSortParameter : SortParameter
    {
        public override Type[] Types { get; } = { typeof(Image) };

        public override Expression<Func<SearchViewModel, object>> Selector { get; } =
            a => (double)a.Image.Width / a.Image.Height;
    }
}
