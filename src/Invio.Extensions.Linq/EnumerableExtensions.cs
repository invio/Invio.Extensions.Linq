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
        public static IEnumerable<T> Cycle<T>(this IEnumerable<T> source) {
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

        /// <summary>
        ///   Similar to <see cref="Enumerable.AsEnumerable" />, this
        ///   method returns an <see cref="IEnumerable" /> that contains
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
        ///   The <see cref="IEnumerable" /> that will be able to be
        ///   enumerated ad infinitum.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Lazily thrown when <paramref name="source" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   Lazily thrown when <paramref name="source" /> is empty.
        /// </exception>
        /// <returns>
        ///   An <see cref="IEnumerable" /> which is able to enumerate
        ///   over all items in <paramref name="source" /> ad infinitum.
        /// </returns>
        public static IEnumerable Cycle(this IEnumerable source) {
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

        /// <summary>
        ///   An overload of <see cref="Enumerable.Zip" />, this method
        ///   combines the corresponding elements of two enumerables into
        ///   a single sequence of tuples.
        /// </summary>
        /// <remarks>
        ///   The method merges each element of the first sequence with
        ///   an element that has the same index in the second sequence.
        ///   If the sequences do not have the same number of elements,
        ///   the method merges sequences until it reaches the end of
        ///   one of them. For example, if one sequence has three elements
        ///   and the other one has four, the result sequence will have
        ///   only three elements.
        /// </remarks>
        /// <typeparam name="TFirst">
        ///   The type of the elements of the first input sequence.
        /// </typeparam>
        /// <typeparam name="TSecond">
        ///   The type of the elements of the second input sequence.
        /// </typeparam>
        /// <param name="first">
        ///   The first enumerable to merge.
        /// </param>
        /// <param name="second">
        ///   The second enumerable to merge.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="first" /> or <paramref name="second" />
        ///   is null.
        /// </exception>
        /// <returns>
        ///   An final enumerable that combines the corresponding elements
        ///   of the two input sequences into a sequence of tuples.
        /// </returns>
        public static IEnumerable<Tuple<TFirst, TSecond>> Zip<TFirst, TSecond>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second) {

            return first.Zip(second, Tuple.Create);
        }

        /// <summary>
        ///   Given an <see cref="IEnumerable{T}" />, this method returns all subsequences
        ///   of <see cref="IEnumerable{T}" /> that contain elements provided via the
        ///   <paramref name="source" /> parameter.
        /// </summary>
        /// <remarks>
        ///   The identity and empty subsequences will be included in the resulting list
        ///   of subsequences for most situations. The lone exception to this is if
        ///   <paramref name="source" /> is empty - then the empty subsequence will
        ///   only be returned once.
        /// </remarks>
        /// <param name="source">
        ///   The original sequence of elements that will be mined for potential subsequences.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="source" /> is null.
        /// </exception>
        /// <returns>
        ///   All potential combinations of subsequences, including the empty sequence and
        ///   the identity sequence, from the original <paramref name="source" />.
        /// </returns>
        public static IEnumerable<IEnumerable<T>> Subsequences<T>(this IEnumerable<T> source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.Any()) {
                yield return Enumerable.Empty<T>();
                yield break;
            }

            var head = source.Take(1);

            foreach (var tail in source.Skip(1).Subsequences()) {
                yield return head.Concat(tail);
                yield return tail;
            }
        }

    }

}
