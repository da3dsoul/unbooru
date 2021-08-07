using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record HeightSearchParameter
        (NumberComparator Operator, int Height, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<Image, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Height != Height;
                case NumberComparator.Equal:
                    return a => a.Height == Height;
                case NumberComparator.GreaterThan:
                    return a => a.Height > Height;
                case NumberComparator.LessThan:
                    return a => a.Height < Height;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Height >= Height;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Height <= Height;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}