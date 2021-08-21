using System;
using System.Linq.Expressions;
using ImageInfrastructure.Web.ViewModel;

namespace ImageInfrastructure.Web.SearchParameters
{
    public record AspectRatioSearchParameter
        (NumberComparator Operator, double AspectRatio, bool Or = false) : SearchParameter(Or)
    {
        public override Expression<Func<SearchViewModel, bool>> Evaluate()
        {
            switch (Operator)
            {
                case NumberComparator.NotEqual:
                    return a => Math.Abs((double)a.Image.Width / a.Image.Height - AspectRatio) > 0.01D;
                case NumberComparator.Equal:
                    return a => Math.Abs((double)a.Image.Width / a.Image.Height - AspectRatio) < 0.01D;
                case NumberComparator.GreaterThan:
                    return a => (double)a.Image.Width / a.Image.Height > AspectRatio;
                case NumberComparator.LessThan:
                    return a => (double)a.Image.Width / a.Image.Height < AspectRatio;
                case NumberComparator.GreaterThan | NumberComparator.Equal:
                    return a => (double)a.Image.Width / a.Image.Height > AspectRatio ||
                                Math.Abs((double)a.Image.Width / a.Image.Height - AspectRatio) < 0.01D;
                case NumberComparator.LessThan | NumberComparator.Equal:
                    return a => (double)a.Image.Width / a.Image.Height < AspectRatio ||
                                Math.Abs((double)a.Image.Width / a.Image.Height - AspectRatio) < 0.01D;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}