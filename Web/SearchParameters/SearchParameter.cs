using System;
using System.Linq.Expressions;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SearchParameters
{
    public abstract record SearchParameter(bool Or)
    {
        public abstract Expression<Func<SearchViewModel, bool>> Evaluate();

        public abstract Type[] Types { get; }
    }
}
