using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SortParameters
{
    public class AspectRatioSortParameter : SortParameter
    {
        public override Type[] Types { get; } = { typeof(Image) };

        public override Expression<Func<SearchViewModel, object>> Selector { get; } =
            a => (double)a.Image.Width / a.Image.Height;
    }
}
