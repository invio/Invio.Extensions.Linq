using System;
using System.Collections.Generic;

namespace Invio.Extensions.Linq {
    /// <summary>
    /// A set of results from a paginated result set. Including the offset into
    /// the result set that this page is from, and the total number of results.
    /// </summary>
    /// <typeparam name="T">The type of result items.</typeparam>
    public sealed class PaginatedResult<T> {
        /// <summary>
        /// A list containing the subset of results.
        /// </summary>
        public IList<T> Results { get; }

        /// <summary>
        /// The offset into the source result set where this page is from.
        /// </summary>
        public Int32 Offset { get; }

        /// <summary>
        /// The Total number of results in the source.
        /// </summary>
        public Int32 Total { get; }

        /// <summary>
        /// Create a new PaginatedResult instance.
        /// </summary>
        public PaginatedResult(IList<T> results, Int32 offset, Int32 total) {
            this.Results = results;
            this.Offset = offset;
            this.Total = total;
        }
    }
}