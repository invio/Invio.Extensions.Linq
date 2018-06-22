using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Invio.Extensions.Reflection;

namespace Invio.Extensions.Linq.Async.Tests {
    /// <summary>
    /// A very haxin QueryProvider.
    /// </summary>
    public class TestAsyncQueryProvider<TRoot> : IAsyncQueryProvider {
        private List<TRoot> source { get; }

        public event EventHandler<AsyncQueryExecutionEventArgs> QueryExecuting;
        public event EventHandler<AsyncQueryExecutionEventArgs> QueryExecuted;

        public TestAsyncQueryProvider(List<TRoot> source) {
            this.source = source;
        }

        public IQueryable<TRoot> CreateQuery() {
            return new TestQuery<TRoot>(this);
        }

        public IQueryable CreateQuery(Expression expression) {
            var elementType = GetElementType(expression.Type);

            if (elementType == null) {
                throw new ArgumentException(
                    $"The specified {nameof(expression)} does not have a type that implements " +
                    $"{typeof(IQueryable<>).Name}.",
                    nameof(expression)
                );
            }

            return (IQueryable)createQueryMethod.MakeGenericMethod(elementType)
                .Invoke(this, new Object[] { expression });
        }

        private Type GetElementType(Type expressionType) {
            var queryableInterface =
                expressionType.GetInterfaces()
                    .Append(expressionType)
                    .SingleOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IQueryable<>));

            return queryableInterface?.GetGenericArguments()[0];
        }

        private static MethodInfo createQueryMethod { get; } =
            ReflectionHelper
                .GetMethodFromExpression<TestAsyncQueryProvider<Object>>(
                    provider => provider.CreateQuery<Object>(null))
                .GetGenericMethodDefinition();

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
            var isOrerded =
                expression.Type.GetInterfaces()
                    .Append(expression.Type)
                    .Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>));

            return isOrerded ?
                new OrderedTestQuery<TElement>(this, expression) :
                new TestQuery<TElement>(this, expression);
        }

        public object Execute(Expression expression) {
            throw new NotSupportedException();
        }

        public TResult Execute<TResult>(Expression expression) {
            throw new NotSupportedException();
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression) {
            var result = await this.ExecuteAsync(expression).ConfigureAwait(false);
            return (TResult)result;
        }

        public async Task<Object> ExecuteAsync(Expression expression) {
            var executingArgs = new AsyncQueryExecutionEventArgs(this, expression);
            this.OnQueryExecuting(executingArgs);
            if (executingArgs.Handler != null) {
                await executingArgs.Handler.ConfigureAwait(false);
            }

            var result = this.ExecuteImpl(expression);

            var executedArgs = new AsyncQueryExecutionEventArgs(this, expression);
            this.OnQueryExecuted(executedArgs);
            if (executedArgs.Handler != null) {
                await executedArgs.Handler.ConfigureAwait(false);
            }

            return result;
        }

        public async Task<IAsyncEnumerator<TResult>> GetEnumeratorAsync<TResult>(Expression expression) {
            var result = await this.ExecuteAsync(expression);
            return new AsyncEnumerator<TResult>(((IEnumerable<TResult>)result).GetEnumerator());
        }

        private Dictionary<MethodInfo, MethodInfo> EnumerableMethodMapping { get; } =
            new Dictionary<MethodInfo, MethodInfo> {
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Where(null, (Expression<Func<Object, Boolean>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Where(null, (Func<Object, Boolean>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Select(null, (Expression<Func<Object, Object>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Select(null, (Func<Object, Object>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.OrderBy<Object, Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.OrderBy<Object, Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Distinct<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Distinct<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Count<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Count<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Count<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Count<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.LongCount<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.LongCount<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.LongCount<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.LongCount<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Int32>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Int32>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Int32?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Int32?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Int64>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Int64>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Int64?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Int64?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Single>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Single>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Single?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Single?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Double>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Double>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Double?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Double?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Decimal>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Decimal>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Average(null, (Expression<Func<Object, Decimal?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Average(null, (Func<Object, Decimal?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Int32>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Int32>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Int32?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Int32?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Int64>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Int64>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Int64?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Int64?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Single>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Single>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Single?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Single?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Double>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Double>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Double?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Double?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Decimal>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Decimal>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Sum(null, (Expression<Func<Object, Decimal?>>)null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Sum(null, (Func<Object, Decimal?>)null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Min<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Min<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Min<Object, Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Min<Object, Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Max<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Max<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Max<Object, Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Max<Object, Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Aggregate<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Aggregate<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Aggregate<Object, Object>(null, null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Aggregate<Object, Object>(null, null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Aggregate<Object, Object, Object>(null, null, null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Aggregate<Object, Object, Object>(null, null, null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Any<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Any<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Any<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Any<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.All<Object>(null, _ => true)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.All<Object>(null, _ => true)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Single<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Single<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Single<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Single<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.SingleOrDefault<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.SingleOrDefault<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.SingleOrDefault<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.SingleOrDefault<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.First<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.First<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.First<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.First<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.FirstOrDefault<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.FirstOrDefault<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.FirstOrDefault<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.FirstOrDefault<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Last<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Last<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Last<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Last<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.LastOrDefault<Object>(null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.LastOrDefault<Object>(null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.LastOrDefault<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.LastOrDefault<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.ElementAt<Object>(null, 0)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.ElementAt<Object>(null, 0)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.ElementAtOrDefault<Object>(null, 0)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.ElementAtOrDefault<Object>(null, 0)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Contains<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Contains<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.Contains<Object>(null, null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.Contains<Object>(null, null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.SequenceEqual<Object>(null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.SequenceEqual<Object>(null, null)).GetGenericMethodDefinition() },
                { ReflectionHelper.GetMethodFromExpression(() => Queryable.SequenceEqual<Object>(null, null, null)).GetGenericMethodDefinition(),
                    ReflectionHelper.GetMethodFromExpression(() => Enumerable.SequenceEqual<Object>(null, null, null)).GetGenericMethodDefinition() }
            };

        private Object ExecuteImpl(Expression expression) {
            switch (expression) {
                case MethodCallExpression callExpression when callExpression.Object == null:
                    // The first argument should be the source IQueryable
                    var innerResult = this.ExecuteImpl(callExpression.Arguments[0]);

                    if (EnumerableMethodMapping.TryGetValue(callExpression.Method.GetGenericMethodDefinition(), out var method)) {
                        try {
                            return method
                                // Generic type arguments should be the same except for Expression<Func>
                                .MakeGenericMethod(
                                    callExpression.Method
                                        .GetGenericArguments()
                                        .Select(UnwrapExpressionTypes)
                                        .ToArray())
                                .Invoke(
                                    null,
                                    new[] { innerResult }
                                        .Concat(callExpression.Arguments.Skip(1).Select(CompileExpression))
                                        .ToArray()
                            );
                        } catch (TargetInvocationException ex) {
                            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                            throw;
                        }
                    } else {
                        throw new ArgumentException(
                            $"The specified {nameof(Expression)} did not have the expected form. " +
                            $"A method was invoked that does not exist on {nameof(Enumerable)}: " +
                            $"{callExpression.Method.DeclaringType}.{callExpression.Method.Name}",
                            nameof(expression)
                        );
                    }
                case ConstantExpression constExpression
                    when constExpression.Type.IsDerivativeOf(typeof(TestQuery<>)):

                    return this.source;
                default:
                    throw new ArgumentException(
                        $"The specified {nameof(Expression)} did not have the expected form.",
                        nameof(expression)
                    );
            }
        }

        private static Object CompileExpression(Expression expression) {
            Object value;
            switch (expression) {
                case UnaryExpression quote when quote.NodeType == ExpressionType.Quote:
                    value = CompileExpression(quote.Operand);
                    break;
                case LambdaExpression lambda:
                    value = lambda.Compile();
                    break;
                case ConstantExpression constant:
                    value = constant.Value;
                    break;
                default:
                    // Compile the expression and evaluate it to get it's value. This should work
                    // for any simple expression like A && B or local.Foo
                    var lambdaWrapper = Expression.Lambda<Func<Object>>(expression).Compile();
                    value = lambdaWrapper();
                    break;
            }

            if (value is IQueryable queryable) {
                return queryable.Provider.Execute(queryable.Expression);
            } else {
                return value;
            }
        }

        private static Type UnwrapExpressionTypes(Type type) {
            return type.IsDerivativeOf(typeof(Expression<>)) ? type.GetGenericArguments()[0] : type;
        }

        private class TestQuery<TElement> : IQueryable<TElement> {
            public Type ElementType => typeof(TElement);
            public Expression Expression { get; }
            public IQueryProvider Provider { get; }

            public TestQuery(IQueryProvider provider) {
                this.Provider = provider;
                this.Expression = Expression.Constant(this);
            }

            public TestQuery(IQueryProvider provider, Expression expression) {
                this.Provider = provider;
                this.Expression = expression;
            }

            public IEnumerator<TElement> GetEnumerator() {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        private class OrderedTestQuery<TElement> :
            TestQuery<TElement>, IOrderedQueryable<TElement> {
            public OrderedTestQuery(IQueryProvider provider) :
                base(provider) {
            }

            public OrderedTestQuery(IQueryProvider provider, Expression expression) :
                base(provider, expression) {
            }
        }

        protected virtual void OnQueryExecuting(AsyncQueryExecutionEventArgs e) {
            QueryExecuting?.Invoke(this, e);
        }

        protected virtual void OnQueryExecuted(AsyncQueryExecutionEventArgs e) {
            QueryExecuted?.Invoke(this, e);
        }

        private sealed class AsyncEnumerator<T> : IAsyncEnumerator<T> {
            private IEnumerator<T> enumerator { get; }

            public T Current => this.enumerator.Current;

            object IEnumerator.Current => Current;

            public AsyncEnumerator(IEnumerator<T> enumerator) {
                this.enumerator = enumerator;
            }

            public Task<bool> MoveNextAsync() {
                return Task.FromResult(this.enumerator.MoveNext());
            }

            bool IEnumerator.MoveNext() {
                return this.enumerator.MoveNext();
            }

            public void Reset() {
                this.enumerator.Reset();
            }

            public void Dispose() {
                this.enumerator.Dispose();
            }
        }
    }

    public static class TestAsyncQueryProvider {
        public static IQueryable<T> CreateQuery<T>(List<T> source) {
            return new TestAsyncQueryProvider<T>(source).CreateQuery();
        }
    }
}
