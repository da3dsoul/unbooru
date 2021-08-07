using System;
using System.Linq;
using System.Linq.Expressions;
using ImageInfrastructure.Abstractions.Poco;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record FileSizeSearchParameter
        (NumberComparator Operator, long Size, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<Image, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => a.Blobs.Max(b => b.Data.LongLength) != Size;
                case NumberComparator.Equal:
                    return a => a.Blobs.Max(b => b.Data.LongLength) == Size;
                case NumberComparator.GreaterThan:
                    return a => a.Blobs.Max(b => b.Data.LongLength) > Size;
                case NumberComparator.LessThan:
                    return a => a.Blobs.Max(b => b.Data.LongLength) < Size;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => a.Blobs.Max(b => b.Data.LongLength) >= Size;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => a.Blobs.Max(b => b.Data.LongLength) <= Size;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}