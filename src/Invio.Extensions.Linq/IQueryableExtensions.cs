using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

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
                    offset + results.Count
                    : source.Count();

            return new PaginatedResult<T>(
                results,
                offset,
                total
            );
        }
    }
}
