using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Invio.Extensions.Linq {

    /// <summary>
    ///   Broad concepts that extend the <see cref="IEnumerable" /> and
    ///   <see cref="IEnumerable{T}" /> functionality that is currently
    ///   available within the System.* namespaces.
    /// </summary>
    public static class EnumerableExtensions {

        /// <summary>
        ///   Searches for an element that matches the conditions defined by the
        ///   specified <see cref="Predicate{T}" />, and returns the zero-based index
        ///   of the first occurrence within the entire <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <param name="source">
        ///   The <see cref="IEnumerable{T}" /> that will be able be searched.
        /// </param>
        /// <param name="match">
        ///   The <see cref="Predicate{T}" /> that defines the conditions
        ///   of the item in the <see cref="IEnumerable{T}" /> to search for.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="source" /> or <paramref name="match" /> is null.
        /// </exception>
        /// <returns>
        ///   The index of the first item first returns <c>true</c> for the
        ///   provided <paramref name="match" /> provided. If no items
        ///   return <c>true</c>, a value of <c>-1</c> is returned.
        /// </returns>
        public static Int32 FindIndex<T>(this IEnumerable<T> source, Predicate<T> match) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            } else if (match == null) {
                throw new ArgumentNullException(nameof(match));
            }

            var result =
                source
                    .Select((item, index) => new { Match = match(item), Index = index })
                    .FirstOrDefault(tuple => tuple.Match);

            return result == null ? -1 : result.Index;
        }

        /// <summary>
        ///   Searches for an element that matches the conditions defined by the
        ///   specified <see cref="Predicate{T}" />, and returns the zero-based index
        ///   of the first occurrence within the entire <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <remarks>
        ///   This implementation breaks from the traditional <see cref="Array" />
        ///   approach in that it does not throw a <see cref="ArgumentOutOfRangeException" />
        ///   if the <paramref name="startIndex" /> is greater than the total size of the
        ///   <see cref="IEnumerable{T}" />. If <paramref name="source" /> does not contain
        ///   the number of elements defined in
        /// </remarks>
        /// <param name="source">
        ///   The <see cref="IEnumerable{T}" /> that will be able be searched.
        /// </param>
        /// <param name="startIndex">
        ///   The zero-based starting index of the search. It can be greater than
        ///   the number of items in the <see cref="IEnumerable{T}" />.
        /// </param>
        /// <param name="match">
        ///   The <see cref="Predicate{T}" /> that defines the conditions
        ///   of the item in the <see cref="IEnumerable{T}" /> to search for.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="source" /> or <paramref name="match" /> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   Thrown when <paramref name="startIndex" /> is less than zero.
        /// </exception>
        /// <returns>
        ///   The index of the first item first returns <c>true</c> for the
        ///   provided <paramref name="match" /> provided after the skipping
        ///   <paramref name="startIndex" /> items in the <paramref name="source" />.
        ///   If no items return <c>true</c>, a value of <c>-1</c> is returned.
        /// </returns>
        public static Int32 FindIndex<T>(this IEnumerable<T> source, int startIndex, Predicate<T> match) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            } else if (startIndex < 0) {
                throw new ArgumentOutOfRangeException(
                    nameof(startIndex),
                    startIndex,
                    $"The '{nameof(startIndex)}' must be a non-negative integer."
                );
            } else if (match == null) {
                throw new ArgumentNullException(nameof(match));
            }

            var result =
                source
                    .Skip(startIndex)
                    .Select((item, index) => new { Match = match(item), Index = startIndex + index })
                    .FirstOrDefault(tuple => tuple.Match);

            return result == null ? -1 : result.Index;
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

            var empty = Enumerable.Empty<T>();

            yield return empty;
            var subsequences = new List<IEnumerable<T>> { empty };

            foreach (var element in source) {
                var additionalSubsequences = new List<IEnumerable<T>>(subsequences.Count);

                foreach (var subsequence in subsequences) {
                    var additionalSubsequence = subsequence.Concat(new T[] { element });

                    yield return additionalSubsequence;
                    additionalSubsequences.Add(additionalSubsequence);
                }

                subsequences.AddRange(additionalSubsequences);
            }
        }

        /// <summary>
        ///   Breaks an <see cref="IEnumerable{T}" /> into batches of <paramref name="size" />.
        /// </summary>
        /// <remarks>
        ///   An empty <see cref="IEnumerable{T}" /> will return no batches. If the number
        ///   of items in the source <see cref="IEnumerable{T}" /> is not perfectly
        ///   divisible by <paramref name="size" />, the last batch will be smaller
        ///   than <paramref name="size" />. The order of items in <paramref name="source" />
        ///   is preserved.
        /// </remarks>
        /// <param name="source">
        ///   The original sequence of elements that will be broken into batches.
        /// </param>
        /// <param name="size">
        ///   The number of items that will be in each set.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="source" /> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   Thrown when <paramref name="size" /> is not a positive integer.
        /// </exception>
        /// <returns>
        ///   Zero ot more batches of <see cref="IEnumerable{T}" /> that will each contain
        ///   up to <paramref name="size" /> items.
        /// </returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            } else if (size < 1) {
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    $"The '{nameof(size)}' must be a positive integer."
                );
            }

            var batch = new T[size];
            var index = 0;

            foreach (var item in source) {
                batch[index++] = item;

                if (index == size) {
                    yield return batch;

                    batch = new T[size];
                    index = 0;
                }
            }

            if (index > 0) {
                Array.Resize(ref batch, index);
                yield return batch;
            }
        }

        /// <summary>
        ///   Returns a distinct enumerable of <see cref="IEnumerable{TSource}" /> by using
        ///   the default comparison on the type <typeparamref name="TKey" />.
        /// </summary>
        /// <typeparam name="TSource">
        ///   The type of the items in <paramref name="source" /> that will be have
        ///   a key extracted in order to determine a new distinct result of items.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///   A value that can be extracted from an instance of <typeparamref name ="TSource" />
        ///   that will determine the distinctness of each item in <paramref name="source" />.
        /// </typeparam>
        /// <param name="source">
        ///   The original sequence of elements that need to be filtered for distinct items.
        /// </param>
        /// <param name="keySelector">
        ///   A delegate that will be invoked on each item with <paramref name="source" />,
        ///   extract a value of type <typeparamref name="TKey" />. If two or more items
        ///   within <paramref name="source" /> have the same key, only one will be returned.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   Thrown when <paramref name="source" /> or <paramref name="keySelector" />
        ///   is null.
        /// </exception>
        /// <returns>
        ///   Each of the results that was determined distinct as determined by the values
        ///   extracted from items in <paramref name="source" /> via key values returned
        ///   from the <paramref name="keySelector" />.
        /// </returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector) {

            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            } else if (keySelector == null) {
                throw new ArgumentNullException(nameof(keySelector));
            }

            return source.Distinct(new ProjectionEqualityComparer<TSource, TKey>(keySelector));
        }

        /// <summary>
        /// Partition an <see cref="IEnumerable{T}" /> source in to a Tuple of
        /// two <see cref="IReadOnlyCollection{T}" /> lists:
        /// <code>(matching, non-matching)</code>.
        /// </summary>
        /// <remarks>
        /// This method enumerates the source only once, but it fully
        /// enumerates it and buffers the items. Not for use on infinite
        /// sequences.
        /// </remarks>
        /// <param name="source">The source of items to partition.</param>
        /// <param name="condition">The predicate on which to partition.</param>
        /// <typeparam name="T">The type of the item being partitioned.</typeparam>
        /// <returns>
        /// A tuple containing the list of items for which the
        /// <paramref name="condition" /> returned true, and the list of items
        /// for which the <paramref name="condition" /> returned false.
        /// </returns>
        public static Tuple<IReadOnlyCollection<T>, IReadOnlyCollection<T>> ToPartitions<T>(
            this IEnumerable<T> source,
            Predicate<T> condition) {

            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }
            if (condition == null) {
                throw new ArgumentNullException(nameof(condition));
            }

            var trueItems = ImmutableList.CreateBuilder<T>();
            var falseItems = ImmutableList.CreateBuilder<T>();

            foreach (var item in source) {
                if (condition(item)) {
                    trueItems.Add(item);
                } else {
                    falseItems.Add(item);
                }
            }

            return Tuple.Create<IReadOnlyCollection<T>, IReadOnlyCollection<T>>(
                trueItems.ToImmutable(),
                falseItems.ToImmutable()
            );
        }

    }

}
