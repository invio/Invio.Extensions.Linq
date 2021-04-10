using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Invio.Extensions.Linq {
    /// <summary>
    /// A set of extension methods on <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class IQueryableExtensions {
        /// <summary>
        /// Retrieve a single page of results from a larger result set,
        /// including the total number of results and the offset of the page.
        /// </summary>
        /// <remarks>
        /// It is up to the caller to ensure that the source has a stable sort
        /// order so that pages returned over multiple calls are consistent.
        /// </remarks>
        /// <param name="source">The source result set.</param>
        /// <param name="pageNumber">
        /// The page number to retrieve starting at 1.
        /// </param>
        /// <param name="pageSize">
        /// The number of results to retrieve per page. (Minimum: 1).
        /// </param>
        /// <typeparam name="T">The type of result items.</typeparam>
        /// <returns>
        /// A PaginatedResult containing the subset of results and the total
        /// number of results.
        /// </returns>
        public static PaginatedResult<T> GetPage<T>(
            this IQueryable<T> source,
            Int32 pageNumber,
            Int32 pageSize) {

            if (source == null) {
                throw new NullReferenceException(nameof(source));
            }

            if (pageNumber < 1) {
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber),
                    pageNumber,
                    "pageNumber must be greater than or equal to 1.");
            }

            if (pageSize < 0) {
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    pageSize,
                    "pageSize must be greater than or equal to 1.");
            }

            if (pageSize == 0) {
                // Short circuit with a simple count when page size is 0
                return new PaginatedResult<T>(
                    ImmutableList<T>.Empty,
                    offset: 0,
                    total: source.Count()
                );
            } else if (pageNumber > Int32.MaxValue / pageSize) {
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber),
                    pageNumber,
                    "pageNumber must not be so large that it would contain an offset larger than the maximum 32 bit integer.");
            }

            var offset = (pageNumber - 1) * pageSize;
            var results =
                source
                    .Skip(offset)
                    .Take(pageSize)
                    .ToImmutableList();

            var isPartialLastPage =
                results.Count < pageSize &&
                (results.Count > 0 || pageNumber == 1);
            var total =
                isPartialLastPage ?
                    // infer the total from the offset + final page size
                    offset + results.Count :
                    source.Count();

            return new PaginatedResult<T>(
                results,
                offset,
                total
            );
        }

        /// <summary>
        /// Performs a Left Join by using a combination of GroupJoin, SelectMany, and DefaultIfEmpty.
        /// </summary>
        /// <param name="source">
        /// The Left query source (a table or subquery expression).
        /// </param>
        /// <param name="related">
        /// The Right query source (a table or subquery expression).
        /// </param>
        /// <param name="sourceKeySelector">
        /// An expression that selects the key to match on from the left source.
        /// </param>
        /// <param name="relatedKeySelector">
        /// An expression that selects the key to match on from the related (right) source.
        /// </param>
        /// <param name="resultSelector">
        /// An expression that creates the result type from the <c>Tuple&lt;TSource, TRelated></c>.
        /// </param>
        /// <typeparam name="TSource">The type contained by the source query.</typeparam>
        /// <typeparam name="TRelated">The type contained by the related query.</typeparam>
        /// <typeparam name="TKey">The type of the key being matched on.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>
        /// A queryable of <typeparamref name="TResult" /> containing entries for each source item
        /// and any related items that matched it. A result for source items with no related items
        /// will also be included.
        /// </returns>
        public static IQueryable<TResult> LeftOuterJoin<TSource, TRelated, TKey, TResult>(
            this IQueryable<TSource> source,
            IQueryable<TRelated> related,
            Expression<Func<TSource, TKey>> sourceKeySelector,
            Expression<Func<TRelated, TKey>> relatedKeySelector,
            Expression<Func<TSource, TRelated, TResult>> resultSelector) {

            var join = source.GroupJoin(
                related,
                sourceKeySelector,
                relatedKeySelector,
                (s, rs) => new { Source = s, RelatedItems = rs }
            );

            // Convert the two parameter (TSource p1, TRelated p2) lambda result selector into a
            // lambda that takes
            // ({TSource Source, IEnumerable<TRelated> RelatedList} p1prime, TRelated p2prime).
            // References to p1 are replaced with references to p1prime.Source, and references to
            // p2 are replaced with p2prime. This is necessary because the maintainers of
            // EntityFrameworkCore are huge knuckleheads.
            var p1Prime = Expression.Parameter(join.ElementType);
            var p2Prime = Expression.Parameter(typeof(TRelated));
            var sourceMember = join.ElementType.GetRuntimeProperty("Source");
            var relatedMember = join.ElementType.GetRuntimeProperty("RelatedItems");
            var sourceReference = Expression.MakeMemberAccess(p1Prime, sourceMember);
            var visitor = new ParameterReplacingExpressionVisitor(new Dictionary<ParameterExpression, Expression> {
                { resultSelector.Parameters[0], sourceReference },
                { resultSelector.Parameters[1], p2Prime }
            });

            var delegateType = typeof(Func<,,>).MakeGenericType(join.ElementType, typeof(TRelated), typeof(TResult));
            var lambda = Expression.Lambda(
                delegateType,
                visitor.Visit(resultSelector.Body) ??
                    throw new InvalidOperationException($"The {nameof(ParameterReplacingExpressionVisitor)} did not return an expression for the body of the result selector"),
                p1Prime,
                p2Prime
            );

            var rowParameter = Expression.Parameter(join.ElementType);
            var collectionSelector = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(join.ElementType, typeof(IEnumerable<TRelated>)),
                Expression.Call(
                    null,
                    DefaultIfEmptyMethod.MakeGenericMethod(typeof(TRelated)),
                    Expression.MakeMemberAccess(rowParameter, relatedMember)
                ),
                rowParameter
            );

            var selectMany = SelectManyMethod.MakeGenericMethod(join.ElementType, typeof(TRelated), typeof(TResult));
            return (IQueryable<TResult>)selectMany.Invoke(null, new Object[] {
                join,
                collectionSelector,
                lambda
            });
        }

        private static readonly MethodInfo SelectManyMethod =
            ((MethodCallExpression)
                new Object[0].Select(_ => new { Foos = new Int32[0] })
                    .AsQueryable()
                    .SelectMany(item => item.Foos, (item, foo) => foo)
                    .Expression).Method.GetGenericMethodDefinition();

        private static readonly MethodInfo DefaultIfEmptyMethod =
            new Func<IEnumerable<Object>, IEnumerable<Object>>(Enumerable.DefaultIfEmpty).GetMethodInfo().GetGenericMethodDefinition();

        private class ParameterReplacingExpressionVisitor : ExpressionVisitor {
            private readonly IDictionary<ParameterExpression, Expression> parameterMapping;

            public ParameterReplacingExpressionVisitor(
                IDictionary<ParameterExpression, Expression> parameterMapping) {

                this.parameterMapping = parameterMapping;
            }

            protected override Expression VisitParameter(ParameterExpression node) {
                if (this.parameterMapping.TryGetValue(node, out var replacement)) {
                    return replacement;
                } else {
                    // Maybe there are nested lambdas in the expression tree.
                    return node;
                }
            }
        }
    }
}
