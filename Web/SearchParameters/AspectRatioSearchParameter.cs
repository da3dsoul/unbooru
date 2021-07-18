using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record AspectRatioSearchParameter
        (NumberComparator Operator, decimal AspectRatio, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<Image, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => Math.Abs((decimal) a.Width / a.Height - AspectRatio) > 0.01M;
                case NumberComparator.Equal:
                    return a => Math.Abs((decimal) a.Width / a.Height - AspectRatio) < 0.01M;
                case NumberComparator.GreaterThan:
                    return a => (decimal) a.Width / a.Height > AspectRatio;
                case NumberComparator.LessThan:
                    return a => (decimal) a.Width / a.Height < AspectRatio;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => (decimal) a.Width / a.Height > AspectRatio || Math.Abs((decimal) a.Width / a.Height - AspectRatio) < 0.01M;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => (decimal) a.Width / a.Height < AspectRatio || Math.Abs((decimal) a.Width / a.Height - AspectRatio) < 0.01M;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}