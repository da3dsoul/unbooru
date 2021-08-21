using System;
using System.Linq.Expressions;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public abstract record SearchParameter(bool Or)
    {
        public virtual Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            return b => true;
        }
    }
}