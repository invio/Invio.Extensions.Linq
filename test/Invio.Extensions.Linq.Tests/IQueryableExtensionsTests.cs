using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Invio.Extensions.Linq {
    public class IQueryableExtensionsTests {
        [Fact]
        public void GetPage_NullSource() {
            Assert.Throws<NullReferenceException>(
                () => ((IQueryable<Object>)null).GetPage(1, 1)
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(Int32.MinValue)]
        public void GetPage_PageNumberOutOfRange(Int32 pageNumber) {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Int32[0].AsQueryable().GetPage(pageNumber, 1)
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(Int32.MinValue)]
        public void GetPage_PageSizeOutOfRange(Int32 pageSize) {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Int32[0].AsQueryable().GetPage(1, pageSize)
            );
        }

        [Fact]
        public void GetPage_PageNumberOverflow() {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Int32[0].AsQueryable().GetPage(2, Int32.MaxValue)
            );
        }

        [Fact]
        public void GetPage_MaxPageSize() {
            var source = new[] { 1, 2, 3 };
            var page = source.AsQueryable().GetPage(1, Int32.MaxValue);
            Assert.Equal(3, page.Total);
            Assert.Equal(source, page.Results);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(10, 1)]
        [InlineData(15, 200)] // page number should be ignored
        public void ZeroPageSize_CountOnly(Int32 total, Int32 pageNumber) {
            GetPageAndCheckQueryCounts(
                total,
                pageNumber,
                pageSize: 0,
                expectedQueries: 0,
                expectedCounts: 1);
        }

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(1, 1, 2)]
        [InlineData(15, 2, 10)]
        [InlineData(21, 3, 10)]
        [InlineData(29, 3, 10)]
        public void PartialLastPage_NoCount(Int32 total, Int32 pageNumber, Int32 pageSize) {
            GetPageAndCheckQueryCounts(
                total,
                pageNumber,
                pageSize,
                expectedQueries: 1,
                expectedCounts: 0);
        }

        [Theory]
        [InlineData(1, 1, 1)]
        [InlineData(2, 1, 1)]
        [InlineData(20, 2, 10)]
        [InlineData(21, 2, 10)]
        [InlineData(30, 3, 10)]
        public void FullPage_Count(Int32 total, Int32 pageNumber, Int32 pageSize) {
            GetPageAndCheckQueryCounts(
                total,
                pageNumber,
                pageSize,
                expectedQueries: 1,
                expectedCounts: 1);
        }

        [Theory]
        [InlineData(0, 2, 1)]
        [InlineData(1, 2, 1)]
        [InlineData(11, 3, 10)]
        [InlineData(11, 4, 10)]
        public void EmptyPage_Count(Int32 total, Int32 pageNumber, Int32 pageSize) {
            var list = Enumerable.Range(0, total).ToImmutableList();
            var source = new QueryCountingSource<Int32>(list.AsQueryable());
            var page = source.GetPage(pageNumber, pageSize);

            Assert.Equal(new Int32[0], page.Results);
            Assert.Equal(total, page.Total);
            Assert.Equal(1, source.NumberOfQueries);
            Assert.Equal(1, source.NumberOfCounts);
        }

        [Fact]
        public void LeftOuterJoin() {
            var parents = new[] {
                new Parent { Name = "Foo" },
                new Parent { Name = "Loner" },
                new Parent { Name = "Fizz" }
            };
            var children = new[] {
                new Child { ParentId = parents[0].Id, Name = "Bar" },
                new Child { ParentId = parents[0].Id, Name = "Baz" },
                new Child { ParentId = parents[2].Id, Name = "Buzz" }
            };

            var parentsQuery = parents.AsQueryable();
            var childrenQuery = children.AsQueryable();

            var result =
                parentsQuery
                    .Where(p => p.Name != "Fizz")
                    .LeftOuterJoin(
                        childrenQuery,
                        p => p.Id,
                        c => c.ParentId,
                        row => new { Parent = row.Item1, Child = row.Item2 })
                    .ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(2, result.Count(r => r.Parent.Name == "Foo"));
            Assert.Single(result.Where(r => r.Parent.Name == "Loner"));
            Assert.Null(result.Single(r => r.Parent.Name == "Loner").Child);
            Assert.True(result.Where(r => r.Parent.Name == "Foo").All(r => r.Child != null));
        }

        private static void GetPageAndCheckQueryCounts(
            Int32 total,
            Int32 pageNumber,
            Int32 pageSize,
            Int32 expectedQueries,
            Int32 expectedCounts) {

            var list = Enumerable.Range(0, total).ToImmutableList();
            var source = new QueryCountingSource<Int32>(list.AsQueryable());
            var page = source.GetPage(pageNumber, pageSize);

            Assert.Equal(list.Skip((pageNumber - 1) * pageSize).Take(pageSize), page.Results);
            Assert.Equal(total, page.Total);
            Assert.Equal(expectedQueries, source.NumberOfQueries);
            Assert.Equal(expectedCounts, source.NumberOfCounts);
        }

        private class QueryCountingSource<T> : IQueryable<T> {
            private IQueryable<T> source { get; }

            public Type ElementType { get; } = typeof(T);
            public Expression Expression => source.Expression;
            public IQueryProvider Provider { get; }

            public Int32 NumberOfQueries { get; private set; }
            public Int32 NumberOfCounts { get; private set; }

            public QueryCountingSource(IQueryable<T> source) {
                this.source = source;
                this.Provider = new QueryCountingQueryProvider(this, source.Provider);
            }

            private class QueryCountingQueryProvider : IQueryProvider {
                private QueryCountingSource<T> parent { get; }
                private IQueryProvider innerProvider { get; }

                public QueryCountingQueryProvider(
                    QueryCountingSource<T> parent,
                    IQueryProvider innerProvider) {
                    this.parent = parent;
                    this.innerProvider = innerProvider;
                }

                public IQueryable CreateQuery(Expression expression) {
                    return new QueryCountingQueryable(
                        this,
                        this.innerProvider.CreateQuery(expression)
                    );
                }

                public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
                    return new QueryCountingQueryable<TElement>(
                        this,
                        this.innerProvider.CreateQuery<TElement>(expression)
                    );
                }

                public Object Execute(Expression expression) {
                    if (IsCountExpression(expression)) {
                        this.parent.NumberOfCounts++;
                    } else {
                        this.parent.NumberOfQueries++;
                    }

                    return this.innerProvider.Execute(expression);
                }

                public TResult Execute<TResult>(Expression expression) {
                    if (IsCountExpression(expression)) {
                        this.parent.NumberOfCounts++;
                    } else {
                         this.parent.NumberOfQueries++;
                    }

                    return this.innerProvider.Execute<TResult>(expression);
                }

                private static MethodInfo countMethod { get; } =
                    GetMethod<IQueryable<Object>>(e => e.Count()).GetGenericMethodDefinition();

                private static MethodInfo filteredCountMethod { get; } =
                    GetMethod<IQueryable<Object>>(e => e.Count(_ => false)).GetGenericMethodDefinition();

                private static Boolean IsCountExpression(Expression expression) {
                    var methodCall = expression as MethodCallExpression;

                    return methodCall != null &&
                        (methodCall.Method.GetGenericMethodDefinition().Equals(countMethod) ||
                         methodCall.Method.GetGenericMethodDefinition().Equals(filteredCountMethod));
                }

                private static MethodInfo GetMethod<TClass>(Expression<Action<TClass>> methodCall) {
                    return ((MethodCallExpression)methodCall.Body).Method;
                }

                private class QueryCountingQueryable : IQueryable {
                    private QueryCountingQueryProvider provider { get; }
                    private IQueryable innerQueryable { get; }

                    public QueryCountingQueryable(
                        QueryCountingQueryProvider provider,
                        IQueryable queryable) {

                        this.provider = provider;
                        this.innerQueryable = queryable;
                    }

                    public IEnumerator GetEnumerator() {
                        this.provider.parent.NumberOfQueries++;
                        return this.innerQueryable.GetEnumerator();
                    }

                    public Type ElementType => this.innerQueryable.ElementType;
                    public Expression Expression => this.innerQueryable.Expression;
                    public IQueryProvider Provider => this.provider;
                }

                private class QueryCountingQueryable<TElement> : IQueryable<TElement> {
                    private QueryCountingQueryProvider provider { get; }
                    private IQueryable<TElement> innerQueryable { get; }

                    public QueryCountingQueryable(
                        QueryCountingQueryProvider provider,
                        IQueryable<TElement> queryable) {

                        this.provider = provider;
                        this.innerQueryable = queryable;
                    }

                    public IEnumerator<TElement> GetEnumerator() {
                        this.provider.parent.NumberOfQueries++;
                        return this.innerQueryable.GetEnumerator();
                    }

                    IEnumerator IEnumerable.GetEnumerator() {
                        return GetEnumerator();
                    }

                    public Type ElementType { get; } = typeof(TElement);
                    public Expression Expression => this.innerQueryable.Expression;
                    public IQueryProvider Provider => this.provider;
                }
            }

            public IEnumerator<T> GetEnumerator() {
                this.NumberOfQueries++;
                return this.source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        private class Parent {
            public Guid Id { get; set; } = Guid.NewGuid();
            public String Name { get; set; }
        }

        private class Child {
            public Guid ParentId { get; set; }
            public String Name { get; set; }
        }
    }
}
