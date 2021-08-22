using System;
using System.Linq.Expressions;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SortParameters
{
    public abstract class SortParameter
    {
        public abstract Type[] Types { get; }
        public abstract Expression<Func<SearchViewModel, object>> Selector { get; }
        public bool Descending { get; set; }
    }
}
