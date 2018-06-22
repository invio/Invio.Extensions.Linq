using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Invio.Extensions.Linq.Async {
    /// <summary>
    /// An extension of <see cref="IQueryProvider" /> interface that supports asynchronously
    /// executing queries.
    /// </summary>
    /// <inheritdoc cref="IQueryProvider" />
    public interface IAsyncQueryProvider : IQueryProvider {
        /// <summary>
        /// Asynchronously executes a query returning the specified type
        /// <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="Expression" /> that represents the query to be executed.
        /// </param>
        /// <typeparam name="TResult">
        /// The type of object returned by the query.
        /// </typeparam>
        /// <returns>A task that asynchronously executes the specified query.</returns>
        Task<TResult> ExecuteAsync<TResult>(Expression expression);

        /// <summary>
        /// Asynchronously executes a query.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="Expression" /> that represents the query to be executed.
        /// </param>
        /// <returns>A task that asynchronously executes the specified query.</returns>
        Task<Object> ExecuteAsync(Expression expression);

        /// <summary>
        /// Asynchronously executes a query returning an enumerator that asynchronously retrieves
        /// items.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="Expression" /> that represents the query to be executed.
        /// </param>
        /// <typeparam name="TResult">
        /// The type of item that the specified query produces a sequence of.
        /// </typeparam>
        /// <returns>A task that asynchronously executes the specified query.</returns>
        Task<IAsyncEnumerator<TResult>> GetEnumeratorAsync<TResult>(Expression expression);
    }
}
