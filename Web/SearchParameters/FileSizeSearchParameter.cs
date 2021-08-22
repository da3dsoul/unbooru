using System;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record FileSizeSearchParameter
        (NumberComparator Operator, long Size, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Image.Size != Size;
                case NumberComparator.Equal:
                    return a => a.Image.Size == Size;
                case NumberComparator.GreaterThan:
                    return a => a.Image.Size > Size;
                case NumberComparator.LessThan:
                    return a => a.Image.Size < Size;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Image.Size >= Size;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Image.Size <= Size;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override Type[] Types { get; } = { typeof(Image) };
    }
}
