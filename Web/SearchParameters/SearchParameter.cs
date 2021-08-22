using System;
using System.Linq.Expressions;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public abstract record SearchParameter(bool Or)
    {
        public abstract Expression<Func<SearchViewModel, bool>> Evaluate();

        public abstract Type[] Types { get; }
    }
}
