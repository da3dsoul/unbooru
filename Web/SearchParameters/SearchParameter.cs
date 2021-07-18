using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public abstract record SearchParameter(bool Or)
    {
        public virtual Expression<Func<Image,bool>> Evaluate()
        {
            return b => true;
        }
    }
}