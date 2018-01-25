using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Invio.Extensions.Linq {

    /// <summary>
    ///   Broad concepts that extend the <see cref="IEnumerable" /> and
    ///   <see cref="IEnumerable{T}" /> functionality that is currently
    ///   available within the System.* namespaces.
    /// </summary>
    public static class EnumerableExtensions {

        /// <summary>
        ///   Similar to <see cref="IEnumerable{T}.GetEnumerator" />, this
        ///   method returns an <see cref="IEnumerator{T}" /> that iterates
        ///   through all items, but instead of stopping at the end, it
        ///   returns once again to the beginning..
        /// </summary>
        /// <remarks>
        ///   The order in which the items will be returned will always
        ///   be the order in which they currently exist within
        ///   <paramref name="source" />.
        /// </remarks>
        /// <param name="source">
        ///   The <see cref="IEnumerable{T}" /> that will be enumerated
        ///   ad infinitum.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Lazily thrown when <paramref name="source" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Lazily thrown when <paramref name="source" /> is empty.
        /// </exception>
        /// <returns>
        ///   An <see cref="IEnumerator{T}" /> which will enumerate over all
        ///   items in <paramref name="source" /> ad infinitum.
        /// </returns>
        public static IEnumerator<T> GetInfiniteEnumerator<T>(
            this IEnumerable<T> source) {

            return source.AsInfiniteEnumerable().GetEnumerator();
        }

        /// <summary>
        ///   Similar to <see cref="Enumerable.AsEnumerable" />, this
        ///   method returns an <see cref="IEnumerable{T}" /> that contains
        ///   all of the items in <paramref name="source" />, but instead of
        ///   stopping at the end of the original enumerable, it returns
        ///   once again to the beginning.
        /// </summary>
        /// <remarks>
        ///   The order in which the items will be returned will always
        ///   be the order in which they currently exist within
        ///   <paramref name="source" />, with the first item immediately
        ///   following the last item.
        /// </remarks>
        /// <param name="source">
        ///   The <see cref="IEnumerable{T}" /> that will be able to be
        ///   enumerated ad infinitum.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Lazily thrown when <paramref name="source" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Lazily thrown when <paramref name="source" /> is empty.
        /// </exception>
        /// <returns>
        ///   An <see cref="IEnumerable{T}" /> which is able to enumerate
        ///   over all items in <paramref name="source" /> ad infinitum.
        /// </returns>
        public static IEnumerable<T> AsInfiniteEnumerable<T>(
            this IEnumerable<T> source) {

            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            var enumerator = source.GetEnumerator();

            while (true) {
                if (!enumerator.MoveNext()) {
                    enumerator = source.GetEnumerator();

                    if (!enumerator.MoveNext()) {
                        throw new ArgumentException(
                            $"The enumerable provided is empty.",
                            nameof(source)
                        );
                    }
                }

                yield return enumerator.Current;
            }
        }

    }

}
