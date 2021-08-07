using System;
using System.Linq;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record PostDateSearchParameter
        (NumberComparator Operator, DateTime Time, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<Image, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Sources.Any(b => b.PostDate.HasValue && b.PostDate.Value != Time);
                case NumberComparator.Equal:
                    return a => a.Sources.Any(b => b.PostDate.HasValue && b.PostDate.Value == Time);
                case NumberComparator.GreaterThan:
                    return a => a.Sources.Any(b => b.PostDate.HasValue && b.PostDate.Value > Time);
                case NumberComparator.LessThan:
                    return a => a.Sources.Any(b => b.PostDate.HasValue && b.PostDate.Value < Time);
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Sources.Any(b => b.PostDate.HasValue && b.PostDate.Value >= Time);
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Sources.Any(b => b.PostDate.HasValue && b.PostDate.Value <= Time);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}