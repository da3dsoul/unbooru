using System;
using System.Linq.Expressions;
using unbooru.Abstractions.Poco;
using unbooru.Web.ViewModel;

namespace unbooru.Web.SearchParameters
{
    public record HeightSearchParameter
        (NumberComparator Operator, int Height, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Image.Height != Height;
                case NumberComparator.Equal:
                    return a => a.Image.Height == Height;
                case NumberComparator.GreaterThan:
                    return a => a.Image.Height > Height;
                case NumberComparator.LessThan:
                    return a => a.Image.Height < Height;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Image.Height >= Height;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Image.Height <= Height;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Type[] Types { get; } = { typeof(Image) };
    }
}
