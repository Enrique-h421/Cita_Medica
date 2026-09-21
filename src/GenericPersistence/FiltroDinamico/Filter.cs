using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace GenericPersistence.FiltroDinamico
{
    public static class Filter
    {
        public static Expression<Func<TModel, bool>> FromStringExpression<TModel>(string query, string parameter = "x")
        {
            try
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TModel), parameter);
                return (Expression<Func<TModel, bool>>)DynamicExpressionParser.ParseLambda(new ParameterExpression[1] { parameterExpression }, null, query);
            }
            catch
            {
                throw new ArgumentException("filter expression invalid");
            }
        }
    }
}