//CREDIT: https://gist.github.com/haacked/febe9e88354fb2f4a4eb11ba88d64c24
//And was written by https://gist.github.com/haacked

/*
Copyright Phil Haack
Licensed under the MIT license - https://github.com/haacked/CodeHaacks/blob/main/LICENSE.
*/
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;

namespace SoftDeletes.Core
{
    public static class ModelBuilderExtensions
    {
        static readonly MethodInfo SetQueryFilterMethod = typeof(ModelBuilderExtensions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilter));

        public static void SetQueryFilterOnAllEntities<TEntityInterface>(
            this ModelBuilder builder,
            Expression<Func<TEntityInterface, bool>> filterExpression)
        {
            foreach (var type in builder.Model.GetEntityTypes()
                .Where(t => t.BaseType == null)
                .Select(t => t.ClrType)
                .Where(t => typeof(TEntityInterface).IsAssignableFrom(t)))
            {
                builder.SetEntityQueryFilter(
                    type,
                    filterExpression);
            }
        }

        static void SetEntityQueryFilter<TEntityInterface>(
            this ModelBuilder builder,
            Type entityType,
            Expression<Func<TEntityInterface, bool>> filterExpression)
        {
            SetQueryFilterMethod
                .MakeGenericMethod(entityType, typeof(TEntityInterface))
               .Invoke(null, new object[] { builder, filterExpression });
        }

        static void SetQueryFilter<TEntity, TEntityInterface>(
            this ModelBuilder builder,
            Expression<Func<TEntityInterface, bool>> filterExpression)
                where TEntityInterface : class
                where TEntity : class, TEntityInterface
        {
            var concreteExpression = filterExpression
                .Convert<TEntityInterface, TEntity>();
            builder.Entity<TEntity>()
                .AddQueryFilter(concreteExpression);
        }

        // CREDIT: The AddQueryFilter and GetInternalEntityTypeBuilder methods come from this comment:
        // https://github.com/aspnet/EntityFrameworkCore/issues/10275#issuecomment-457504348
        // And was written by https://github.com/YZahringer
        static void AddQueryFilter<T>(this EntityTypeBuilder entityTypeBuilder, Expression<Func<T, bool>> expression)
        {
            var parameterType = Expression.Parameter(entityTypeBuilder.Metadata.ClrType);
            var expressionFilter = ReplacingExpressionVisitor.Replace(
                expression.Parameters.Single(), parameterType, expression.Body);

            var internalEntityTypeBuilder = entityTypeBuilder.GetInternalEntityTypeBuilder();
            var queryFilter = internalEntityTypeBuilder?.Metadata.GetQueryFilter();
            if (queryFilter != null)
            {
                var currentExpressionFilter = ReplacingExpressionVisitor.Replace(
                    queryFilter.Parameters.Single(), parameterType, queryFilter.Body);
                expressionFilter = Expression.AndAlso(currentExpressionFilter, expressionFilter);
            }

            var lambdaExpression = Expression.Lambda(expressionFilter, parameterType);
            entityTypeBuilder.HasQueryFilter(lambdaExpression);
        }

        static InternalEntityTypeBuilder? GetInternalEntityTypeBuilder(
            this EntityTypeBuilder entityTypeBuilder)
        {
            var internalEntityTypeBuilder = typeof(EntityTypeBuilder)
                .GetProperty("Builder", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(entityTypeBuilder) as InternalEntityTypeBuilder;

            return internalEntityTypeBuilder;
        }
    }

    public static class ExpressionExtensions
    {
        // This magic is courtesy of this StackOverflow post.
        // https://stackoverflow.com/questions/38316519/replace-parameter-type-in-lambda-expression
        // I made some tweaks to adapt it to our needs - @haacked
        public static Expression<Func<TTarget, bool>> Convert<TSource, TTarget>(
            this Expression<Func<TSource, bool>> root)
        {
            var visitor = new ParameterTypeVisitor<TSource, TTarget>();
            return (Expression<Func<TTarget, bool>>)visitor.Visit(root);
        }

        class ParameterTypeVisitor<TSource, TTarget> : ExpressionVisitor
        {
            private ReadOnlyCollection<ParameterExpression> _parameters;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameters?.FirstOrDefault(p => p.Name == node.Name)
                       ?? (node.Type == typeof(TSource) ? Expression.Parameter(typeof(TTarget), node.Name) : node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                _parameters = VisitAndConvert(node.Parameters, "VisitLambda");
                return Expression.Lambda(Visit(node.Body), _parameters);
            }
        }
    }
}
