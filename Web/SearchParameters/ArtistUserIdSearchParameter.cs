using System;
using System.Linq;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SearchParameters
{
    public record ArtistUserIDSearchParameter
        (string ArtistID, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            return a => a.ArtistAccounts.Any(b => b.Id == ArtistID);
        }

        public override Type[] Types { get; } = { typeof(ArtistAccount) };
    }
}
