using System;
using System.Linq;
using System.Linq.Expressions;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record PixivIDSearchParameter
        (string PixivID, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            return a => a.Sources.Any(b => b.Source == "Pixiv" && b.PostId == PixivID);
        }
    }
}