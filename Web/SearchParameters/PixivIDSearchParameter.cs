using System;
using System.Linq;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record PixivIDSearchParameter
        (string PixivID, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<Image, bool>> Evaluate()
        {
            return a => a.Sources.Any(b => b.Source == "Pixiv" && b.PostId == PixivID);
        }
    }
}