using System;
using System.Linq;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SearchParameters
{
    public record ArtistAccountIDSearchParameter
        (NumberComparator Operator, int ArtistAccountID, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            return a => a.ArtistAccounts.Any(b => b.ArtistAccountId == ArtistAccountID);
        }

        public override Type[] Types { get; } = { typeof(ArtistAccount) };
    }
}
