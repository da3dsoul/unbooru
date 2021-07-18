using System;
using System.Linq.Expressions;

namespace ImageInfrastructure.Web
{
    public static class ExpressionHandler
    {
        public class ParameterReplaceVisitor : ExpressionVisitor
        {
            public ParameterExpression Target { get; set; }
            public ParameterExpression Replacement { get; set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == Target ? Replacement : base.VisitParameter(node);
            }
        }

        public static Expression<Func<T, bool>> AndExpression<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var visitor = new ParameterReplaceVisitor()
            {
                Target = right.Parameters[0],
                Replacement = left.Parameters[0],
            };

            var rewrittenRight = visitor.Visit(right.Body);
            var andExpression = Expression.AndAlso(left.Body, rewrittenRight);
            return Expression.Lambda<Func<T, bool>>(andExpression, left.Parameters);
        }

        public static Expression<Func<T, bool>> OrExpression<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var visitor = new ParameterReplaceVisitor()
            {
                Target = right.Parameters[0],
                Replacement = left.Parameters[0],
            };

            var rewrittenRight = visitor.Visit(right.Body);
            var andExpression = Expression.Or(left.Body, rewrittenRight);
            return Expression.Lambda<Func<T, bool>>(andExpression, left.Parameters);
        }
    }
}