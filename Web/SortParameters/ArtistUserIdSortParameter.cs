using System;
using System.Linq;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SortParameters
{
    public class ArtistUserIdSortParameter : SortParameter
    {
        public override Type[] Types { get; } = { typeof(ArtistAccount) };

        public override Expression<Func<SearchViewModel, object>> Selector { get; } = a =>
            a.ArtistAccounts.OrderBy(a => a.Id).FirstOrDefault().Id;
    }
}
