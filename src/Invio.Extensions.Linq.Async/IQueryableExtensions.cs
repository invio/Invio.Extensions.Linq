using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Invio.Extensions.Reflection;

namespace Invio.Extensions.Linq.Async {
    /// <summary>
    /// Extension methods for Asynchronous operations on <see cref="IQueryable{T}" />.
    /// </summary>
    public static class IQueryableExtensions {
        /// <summary>
        /// Asynchronous version of <see cref="Enumerable.ToList{T}" />.
        /// </summary>
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> source) {
            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var enumerator =
                    await asyncProvider.GetEnumeratorAsync<T>(source.Expression)
                        .ConfigureAwait(continueOnCapturedContext: false);

                var list = new List<T>();

                while(await enumerator.MoveNextAsync()) {
                    list.Add(enumerator.Current);
                }

                return list;
            } else {
                return source.ToList();
            }
        }

        /// <summary>
        /// Asynchronous version of <see cref="Enumerable.ToArray{T}" />.
        /// </summary>
        public static async Task<T[]> ToArrayAsync<T>(this IQueryable<T> source) {
            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var enumerator =
                    await asyncProvider.GetEnumeratorAsync<T>(source.Expression)
                        .ConfigureAwait(continueOnCapturedContext: false);

                var list = new List<T>();

                while(await enumerator.MoveNextAsync()) {
                    list.Add(enumerator.Current);
                }

                return list.ToArray();
            } else {
                return source.ToArray();
            }
        }

// it seems to be impossible to unambiguously reference these ToDictionary overloads.
#pragma warning disable 1574
        /// <summary>
        /// Asynchronous version of
        /// <see cref="Enumerable.ToDictionary{T, TKey}(IQueryable{T}, Func{T, TKey})" />.
        /// </summary>
        public static async Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(
            this IQueryable<T> source,
            Func<T, TKey> keySelector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var enumerator =
                    await asyncProvider.GetEnumeratorAsync<T>(source.Expression)
                        .ConfigureAwait(continueOnCapturedContext: false);

                var dict = new Dictionary<TKey, T>();
                while(await enumerator.MoveNextAsync()) {
                    dict.Add(keySelector(enumerator.Current), enumerator.Current);
                }

                return dict;
            } else {
                return source.ToDictionary(keySelector);
            }
        }

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Enumerable.ToDictionary{T, TKey}(IQueryable{T}, Func{T, TKey}, IEqualityComparer{TKey})" />.
        /// </summary>
        public static async Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(
            this IQueryable<T> source,
            Func<T, TKey> keySelector,
            IEqualityComparer<TKey> comparer) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var enumerator =
                    await asyncProvider.GetEnumeratorAsync<T>(source.Expression)
                        .ConfigureAwait(continueOnCapturedContext: false);

                var dict = new Dictionary<TKey, T>(comparer);
                while(await enumerator.MoveNextAsync()) {
                    dict.Add(keySelector(enumerator.Current), enumerator.Current);
                }

                return dict;
            } else {
                return source.ToDictionary(keySelector, comparer);
            }
        }
#pragma warning restore 1574

        private static MethodInfo CountMethod { get; } =
            ReflectionHelper.GetFuncMethod<IQueryable<T1>, Int32>(Queryable.Count)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Count{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<Int32> CountAsync<T>(this IQueryable<T> source) {
            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        CountMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Int32>(expression);
            } else {
                return Task.FromResult(source.Count());
            }
        }

        private static MethodInfo CountFilteredMethod { get; } =
            ReflectionHelper.GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, Int32>(
                    Queryable.Count)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Count{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<Int32> CountAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        CountFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate)
                    );

                return asyncProvider.ExecuteAsync<Int32>(expression);
            } else {
                return Task.FromResult(source.Count(predicate));
            }
        }

        private static MethodInfo LongCountMethod { get; } =
            ReflectionHelper.GetFuncMethod<IQueryable<T1>, Int64>(Queryable.LongCount)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.LongCount{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<Int64> LongCountAsync<T>(this IQueryable<T> source) {
            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        LongCountMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Int64>(expression);
            } else {
                return Task.FromResult(source.LongCount());
            }
        }

        private static MethodInfo LongCountFilteredMethod { get; } =
            ReflectionHelper.GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, Int64>(
                    Queryable.LongCount)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.LongCount{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<Int64> LongCountAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        LongCountFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate)
                    );

                return asyncProvider.ExecuteAsync<Int64>(expression);
            } else {
                return Task.FromResult(source.LongCount(predicate));
            }
        }

        private static MethodInfo AverageInt32Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int32>>, Double>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Int32}})" />.
        /// </summary>
        public static Task<Double> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int32>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageInt32Method.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(selector)
                    );

                return asyncProvider.ExecuteAsync<Double>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageNullableInt32Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int32?>>, Double?>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Int32?}})" />.
        /// </summary>
        public static Task<Double?> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int32?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageNullableInt32Method.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Double?>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageInt64Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int64>>, Double>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Int64}})" />.
        /// </summary>
        public static Task<Double> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int64>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageInt64Method.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Double>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageNullableInt64Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int64?>>, Double?>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Int64?}})" />.
        /// </summary>
        public static Task<Double?> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int64?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageNullableInt64Method.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Double?>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageSingleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Single>>, Single>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Single}})" />.
        /// </summary>
        public static Task<Single> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Single>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageSingleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Single>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageNullableSingleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Single?>>, Single?>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Single?}})" />.
        /// </summary>
        public static Task<Single?> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Single?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageNullableSingleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Single?>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageDoubleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Double>>, Double>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Double}})" />.
        /// </summary>
        public static Task<Double> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Double>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageDoubleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Double>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageNullableDoubleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Double?>>, Double?>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Double?}})" />.
        /// </summary>
        public static Task<Double?> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Double?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageNullableDoubleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Double?>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageDecimalMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Decimal>>, Decimal>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Decimal}})" />.
        /// </summary>
        public static Task<Decimal> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Decimal>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageDecimalMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Decimal>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo AverageNullableDecimalMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Decimal?>>, Decimal?>(
                    Queryable.Average)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Average{T}(IQueryable{T}, Expression{Func{T, Decimal?}})" />.
        /// </summary>
        public static Task<Decimal?> AverageAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Decimal?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AverageNullableDecimalMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Decimal?>(expression);
            } else {
                return Task.FromResult(source.Average(selector));
            }
        }

        private static MethodInfo SumInt32Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int32>>, Int32>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Int32}})" />.
        /// </summary>
        public static Task<Int32> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int32>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumInt32Method.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(selector)
                    );

                return asyncProvider.ExecuteAsync<Int32>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumNullableInt32Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int32?>>, Int32?>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Int32?}})" />.
        /// </summary>
        public static Task<Int32?> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int32?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumNullableInt32Method.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Int32?>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumInt64Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int64>>, Int64>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Int64}})" />.
        /// </summary>
        public static Task<Int64> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int64>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumInt64Method.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Int64>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumNullableInt64Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Int64?>>, Int64?>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Int64?}})" />.
        /// </summary>
        public static Task<Int64?> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Int64?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumNullableInt64Method.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Int64?>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumSingleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Single>>, Single>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Single}})" />.
        /// </summary>
        public static Task<Single> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Single>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumSingleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Single>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumNullableSingleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Single?>>, Single?>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Single?}})" />.
        /// </summary>
        public static Task<Single?> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Single?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumNullableSingleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Single?>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumDoubleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Double>>, Double>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Double}})" />.
        /// </summary>
        public static Task<Double> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Double>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumDoubleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Double>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumNullableDoubleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Double?>>, Double?>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Double?}})" />.
        /// </summary>
        public static Task<Double?> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Double?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumNullableDoubleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Double?>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumDecimalMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Decimal>>, Decimal>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Decimal}})" />.
        /// </summary>
        public static Task<Decimal> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Decimal>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumDecimalMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Decimal>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo SumNullableDecimalMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Decimal?>>, Decimal?>(
                    Queryable.Sum)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Sum{T}(IQueryable{T}, Expression{Func{T, Decimal?}})" />.
        /// </summary>
        public static Task<Decimal?> SumAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Decimal?>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SumNullableDecimalMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<Decimal?>(expression);
            } else {
                return Task.FromResult(source.Sum(selector));
            }
        }

        private static MethodInfo MinMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.Min)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Min{T}" />.
        /// </summary>
        public static Task<T> MinAsync<T>(this IQueryable<T> source) {
            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        MinMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.Min());
            }
        }

        private static MethodInfo MinSelectorMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, T1>>, T1>(Queryable.Min)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Min{T, TResult}" />.
        /// </summary>
        public static Task<TResult> MinAsync<T, TResult>(
            this IQueryable<T> source,
            Expression<Func<T, TResult>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        MinSelectorMethod.MakeGenericMethod(typeof(T), typeof(TResult)),
                        source.Expression,
                        Expression.Quote(selector)
                    );

                return asyncProvider.ExecuteAsync<TResult>(expression);
            } else {
                return Task.FromResult(source.Min(selector));
            }
        }

        private static MethodInfo MaxMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.Max)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Max{T}" />.
        /// </summary>
        public static Task<T> MaxAsync<T>(this IQueryable<T> source) {
            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        MaxMethod.MakeGenericMethod(typeof(T)),
                        source.Expression
                    );

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.Max());
            }
        }

        private static MethodInfo MaxSelectorMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, T1>>, T1>(Queryable.Max)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Max{T, TResult}" />.
        /// </summary>
        public static Task<TResult> MaxAsync<T, TResult>(
            this IQueryable<T> source,
            Expression<Func<T, TResult>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        MaxSelectorMethod.MakeGenericMethod(typeof(T), typeof(TResult)),
                        source.Expression,
                        Expression.Quote(selector)
                    );

                return asyncProvider.ExecuteAsync<TResult>(expression);
            } else {
                return Task.FromResult(source.Max(selector));
            }
        }

        private static MethodInfo AggregateMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, T1, T1>>, T1>(Queryable.Aggregate)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Aggregate{T}" />.
        /// </summary>
        public static Task<T> AggregateAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, T, T>> accumulator) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AggregateMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(accumulator)
                    );

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.Aggregate(accumulator));
            }
        }

        private static MethodInfo Aggregate2Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T2, Expression<Func<T2, T1, T2>>, T2>(
                    Queryable.Aggregate)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Aggregate{T, TAcumulate}" />.
        /// </summary>
        public static Task<TAccumulate> AggregateAsync<T, TAccumulate>(
            this IQueryable<T> source,
            TAccumulate seed,
            Expression<Func<TAccumulate, T, TAccumulate>> accumulator) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        Aggregate2Method.MakeGenericMethod(typeof(T), typeof(TAccumulate)),
                        source.Expression,
                        Expression.Constant(seed),
                        Expression.Quote(accumulator)
                    );

                return asyncProvider.ExecuteAsync<TAccumulate>(expression);
            } else {
                return Task.FromResult(source.Aggregate(seed, accumulator));
            }
        }

        private static MethodInfo Aggregate3Method { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T2, Expression<Func<T2, T1, T2>>, Expression<Func<T2, T3>>, T3>(
                    Queryable.Aggregate)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Aggregate{T, TAccumulate, TResult}" />.
        /// </summary>
        public static Task<TResult> AggregateAsync<T, TAccumulate, TResult>(
            this IQueryable<T> source,
            TAccumulate seed,
            Expression<Func<TAccumulate, T, TAccumulate>> accumulator,
            Expression<Func<TAccumulate, TResult>> selector) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        Aggregate3Method.MakeGenericMethod(typeof(T), typeof(TAccumulate), typeof(TResult)),
                        source.Expression,
                        Expression.Constant(seed),
                        Expression.Quote(accumulator),
                        Expression.Quote(selector));

                return asyncProvider.ExecuteAsync<TResult>(expression);
            } else {
                return Task.FromResult(source.Aggregate(seed, accumulator, selector));
            }
        }

        private static MethodInfo AnyMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Boolean>(Queryable.Any)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Any{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<Boolean> AnyAsync<T>(this IQueryable<T> source) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AnyMethod.MakeGenericMethod(typeof(T)),
                        source.Expression);

                return asyncProvider.ExecuteAsync<Boolean>(expression);
            } else {
                return Task.FromResult(source.Any());
            }
        }

        private static MethodInfo AnyFilteredMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, Boolean>(
                    Queryable.Any)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Any{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<Boolean> AnyAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AnyFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<Boolean>(expression);
            } else {
                return Task.FromResult(source.Any(predicate));
            }
        }

        private static MethodInfo AllMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, Boolean>(
                    Queryable.All)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.All{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<Boolean> AllAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        AllMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<Boolean>(expression);
            } else {
                return Task.FromResult(source.All(predicate));
            }
        }

        private static MethodInfo SingleMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.Single)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Single{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<T> SingleAsync<T>(this IQueryable<T> source) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SingleMethod.MakeGenericMethod(typeof(T)),
                        source.Expression);

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.Single());
            }
        }

        private static MethodInfo SingleFilteredMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, T1>(
                    Queryable.Single)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Single{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<T> SingleAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SingleFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.Single(predicate));
            }
        }

        private static MethodInfo SingleOrDefaultMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.SingleOrDefault)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.SingleOrDefault{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> source) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SingleOrDefaultMethod.MakeGenericMethod(typeof(T)),
                        source.Expression);

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.SingleOrDefault());
            }
        }

        private static MethodInfo SingleOrDefaultFilteredMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, T1>(
                    Queryable.SingleOrDefault)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.SingleOrDefault{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<T> SingleOrDefaultAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SingleOrDefaultFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.SingleOrDefault(predicate));
            }
        }

        private static MethodInfo FirstMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.First)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.First{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<T> FirstAsync<T>(this IQueryable<T> source) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        FirstMethod.MakeGenericMethod(typeof(T)),
                        source.Expression);

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.First());
            }
        }

        private static MethodInfo FirstFilteredMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, T1>(
                    Queryable.First)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.First{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<T> FirstAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        FirstFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.First(predicate));
            }
        }

        private static MethodInfo FirstOrDefaultMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.FirstOrDefault)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.FirstOrDefault{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        FirstOrDefaultMethod.MakeGenericMethod(typeof(T)),
                        source.Expression);

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.FirstOrDefault());
            }
        }

        private static MethodInfo FirstOrDefaultFilteredMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, T1>(
                    Queryable.FirstOrDefault)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.FirstOrDefault{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<T> FirstOrDefaultAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        FirstOrDefaultFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.FirstOrDefault(predicate));
            }
        }

        private static MethodInfo LastMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.Last)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Last{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<T> LastAsync<T>(this IQueryable<T> source) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        LastMethod.MakeGenericMethod(typeof(T)),
                        source.Expression);

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.Last());
            }
        }

        private static MethodInfo LastFilteredMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, T1>(
                    Queryable.Last)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Last{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<T> LastAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        LastFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.Last(predicate));
            }
        }

        private static MethodInfo LastOrDefaultMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1>(Queryable.LastOrDefault)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.LastOrDefault{T}(IQueryable{T})" />.
        /// </summary>
        public static Task<T> LastOrDefaultAsync<T>(this IQueryable<T> source) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        LastOrDefaultMethod.MakeGenericMethod(typeof(T)),
                        source.Expression);

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.LastOrDefault());
            }
        }

        private static MethodInfo LastOrDefaultFilteredMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Expression<Func<T1, Boolean>>, T1>(
                    Queryable.LastOrDefault)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.LastOrDefault{T}(IQueryable{T}, Expression{Func{T, Boolean}})" />.
        /// </summary>
        public static Task<T> LastOrDefaultAsync<T>(
            this IQueryable<T> source,
            Expression<Func<T, Boolean>> predicate) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        LastOrDefaultFilteredMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Quote(predicate));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.LastOrDefault(predicate));
            }
        }

        private static MethodInfo ElementAtMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Int32, T1>(Queryable.ElementAt)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.ElementAt{T}" />.
        /// </summary>
        public static Task<T> ElementAtAsync<T>(this IQueryable<T> source, Int32 index) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        ElementAtMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Constant(index));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.ElementAt(index));
            }
        }

        private static MethodInfo ElementAtOrDefaultMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, Int32, T1>(Queryable.ElementAtOrDefault)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.ElementAtOrDefault{T}" />.
        /// </summary>
        public static Task<T> ElementAtOrDefaultAsync<T>(this IQueryable<T> source, Int32 index) {
            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        ElementAtOrDefaultMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Constant(index));

                return asyncProvider.ExecuteAsync<T>(expression);
            } else {
                return Task.FromResult(source.ElementAtOrDefault(index));
            }
        }

        private static MethodInfo ContainsMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1, Boolean>(
                    Queryable.Contains)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of <see cref="Queryable.Contains{T}(IQueryable{T}, T)" />.
        /// </summary>
        public static Task<Boolean> ContainsAsync<T>(
            this IQueryable<T> source,
            T item) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        ContainsMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Constant(item));

                return asyncProvider.ExecuteAsync<Boolean>(expression);
            } else {
                return Task.FromResult(source.Contains(item));
            }
        }

        private static MethodInfo ContainsComparerMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, T1, IEqualityComparer<T1>, Boolean>(
                    Queryable.Contains)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.Contains{T}(IQueryable{T}, T, IEqualityComparer{T})" />.
        /// </summary>
        public static Task<Boolean> ContainsAsync<T>(
            this IQueryable<T> source,
            T item,
            IEqualityComparer<T> comparer) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        ContainsComparerMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Constant(item),
                        Expression.Constant(comparer));

                return asyncProvider.ExecuteAsync<Boolean>(expression);
            } else {
                return Task.FromResult(source.Contains(item, comparer));
            }
        }

        private static MethodInfo SequenceEqualMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, IEnumerable<T1>, Boolean>(
                    Queryable.SequenceEqual)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.SequenceEqual{T}(IQueryable{T}, IEnumerable{T})" />.
        /// </summary>
        public static Task<Boolean> SequenceEqualAsync<T>(
            this IQueryable<T> source,
            IEnumerable<T> items) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SequenceEqualMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Constant(items));

                return asyncProvider.ExecuteAsync<Boolean>(expression);
            } else {
                return Task.FromResult(source.SequenceEqual(items));
            }
        }

        private static MethodInfo SequenceEqualComparerMethod { get; } =
            ReflectionHelper
                .GetFuncMethod<IQueryable<T1>, IEnumerable<T1>, IEqualityComparer<T1>, Boolean>(
                    Queryable.SequenceEqual)
                .GetGenericMethodDefinition();

        /// <summary>
        /// Asynchronous version of
        /// <see cref="Queryable.SequenceEqual{T}(IQueryable{T}, IEnumerable{T}, IEqualityComparer{T})" />.
        /// </summary>
        public static Task<Boolean> SequenceEqualAsync<T>(
            this IQueryable<T> source,
            IEnumerable<T> items,
            IEqualityComparer<T> comparer) {

            if (source.Provider is IAsyncQueryProvider asyncProvider) {
                var expression =
                    Expression.Call(
                        SequenceEqualComparerMethod.MakeGenericMethod(typeof(T)),
                        source.Expression,
                        Expression.Constant(items),
                        Expression.Constant(comparer));

                return asyncProvider.ExecuteAsync<Boolean>(expression);
            } else {
                return Task.FromResult(source.SequenceEqual(items, comparer));
            }
        }

        // Place holders for arbitrary type arguments
        private class T1 {
        }

        private class T2 {
        }

        private class T3 {
        }
    }
}
