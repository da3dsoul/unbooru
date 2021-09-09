using System;
using System.Linq.Expressions;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SortParameters
{
    public abstract class SortParameter
    {
        public abstract Type[] Types { get; }
        public abstract Expression<Func<SearchViewModel, object>> Selector { get; }
        public bool Descending { get; set; }
    }
}
