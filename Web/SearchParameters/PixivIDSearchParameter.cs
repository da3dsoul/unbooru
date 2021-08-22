using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record PixivIDSearchParameter
        (string PixivID, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            return a => a.PixivSource.PostId == PixivID;
        }

        public override Type[] Types { get; } = { typeof(ImageSource) };
    }
}
