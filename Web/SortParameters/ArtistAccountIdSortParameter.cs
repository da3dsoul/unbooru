using System;
using System.Linq;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SortParameters
{
    public class ArtistAccountIdSortParameter : SortParameter
    {
        public override Type[] Types { get; } = { typeof(ArtistAccount) };

        public override Expression<Func<SearchViewModel, object>> Selector { get; } = a =>
            a.ArtistAccounts.OrderBy(a => a.ArtistAccountId).FirstOrDefault().ArtistAccountId;
    }
}
