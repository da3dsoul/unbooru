using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record WidthSearchParameter
        (NumberComparator Operator, int Width, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<Image, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Width != Width;
                case NumberComparator.Equal:
                    return a => a.Width == Width;
                case NumberComparator.GreaterThan:
                    return a => a.Width > Width;
                case NumberComparator.LessThan:
                    return a => a.Width < Width;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Width >= Width;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Width <= Width;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}