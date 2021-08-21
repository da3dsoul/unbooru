using System;
using System.Linq.Expressions;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record WidthSearchParameter
        (NumberComparator Operator, int Width, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Image.Width != Width;
                case NumberComparator.Equal:
                    return a => a.Image.Width == Width;
                case NumberComparator.GreaterThan:
                    return a => a.Image.Width > Width;
                case NumberComparator.LessThan:
                    return a => a.Image.Width < Width;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Image.Width >= Width;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Image.Width <= Width;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}