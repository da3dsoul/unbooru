using System;
using System.Linq.Expressions;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record ImportDateSearchParameter
        (NumberComparator Operator, DateTime Time, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Image.ImportDate != Time;
                case NumberComparator.Equal:
                    return a => a.Image.ImportDate == Time;
                case NumberComparator.GreaterThan:
                    return a => a.Image.ImportDate > Time;
                case NumberComparator.LessThan:
                    return a => a.Image.ImportDate < Time;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Image.ImportDate >= Time;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Image.ImportDate <= Time;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}