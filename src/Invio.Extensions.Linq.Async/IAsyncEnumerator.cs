using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Invio.Extensions.Linq.Async {
    /// <summary>
    /// An extension of the <see cref="IEnumerator{T}" /> interface that supports asynchronously
    /// retrieving items.
    /// </summary>
    /// <typeparam name="T">The type of item being enumerated.</typeparam>
    /// <inheritdoc cref="IEnumerator{T}" />
    public interface IAsyncEnumerator<out T> : IEnumerator<T> {
        /// <summary>
        /// Asynchronously retrieve the next item.
        /// </summary>
        /// <returns>
        /// A Task that asynchronously returns either <c>true</c> if an item exists, or
        /// <c>false</c> if the end of the sequence has been reached.
        /// </returns>
        Task<Boolean> MoveNextAsync();
    }
}
