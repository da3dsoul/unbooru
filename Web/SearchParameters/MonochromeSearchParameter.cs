using System;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SearchParameters
{
    public record MonochromeSearchParameter(bool invert = false, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            return a => a.Image.Composition != null && a.Image.Composition.IsMonochrome != invert;
        }

        public override Type[] Types { get; } = { typeof(ImageComposition) };
    }
}
