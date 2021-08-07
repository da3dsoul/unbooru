using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record ImportDateSearchParameter
        (NumberComparator Operator, DateTime Time, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<Image, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.ImportDate != Time;
                case NumberComparator.Equal:
                    return a => a.ImportDate == Time;
                case NumberComparator.GreaterThan:
                    return a => a.ImportDate > Time;
                case NumberComparator.LessThan:
                    return a => a.ImportDate < Time;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.ImportDate >= Time;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.ImportDate <= Time;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}