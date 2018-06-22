using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Invio.Extensions.Linq.Async.Tests {
    public abstract class AsyncQueryableTestBase {
        protected List<TestModel> Items { get; }
        protected abstract IQueryable<TestModel> Query { get; }

        protected AsyncQueryableTestBase() {
            this.Items =
                new List<TestModel> {
                    new TestModel {
                        Name = "Foo",
                        InStock = false,
                        ExpirationDate = new DateTime(2017, 9, 27),
                        LotSize = 7,
                        Price = 19.95m
                    },
                    new TestModel {
                        Name = "Bar",
                        InStock = true,
                        ExpirationDate = new DateTime(2025, 1, 1),
                        LotSize = 42,
                        Price = 1.618m
                    },
                    new TestModel {
                        Name = "Baz",
                        InStock = false,
                        ExpirationDate = new DateTime(2045, 7, 12),
                        LotSize = 20,
                        Price = 3.14m
                    }
                };
        }

        [Fact]
        public async Task ToListAsync_VerifyAsyncExecution() {
            var result = await this.VerifyAsyncExecution(query => query.ToListAsync());

            Assert.Equal(
                this.Items,
                result
            );
        }

        [Fact]
        public async Task ToListAsync_All() {
            var result = await this.Query.ToListAsync();

            Assert.Equal(
                this.Items,
                result
            );
        }

        [Fact]
        public async Task ToListAsync_Filtered() {
            var result = await this.Query.Where(m => !m.InStock).ToListAsync();

            Assert.Equal(
                new[] { this.Items[0], this.Items[2] },
                result
            );
        }

        [Fact]
        public async Task ToListAsync_Select() {
            var result = await this.Query.Select(m => m.Name).ToListAsync();

            Assert.Equal(
                new[] { "Foo", "Bar", "Baz" },
                result
            );
        }

        [Fact]
        public async Task ToListAsync_Ordered() {
            var result = await this.Query.OrderBy(m => m.Name).ToListAsync();

            Assert.Equal(
                new[] { this.Items[1], this.Items[2], this.Items[0] },
                result
            );
        }

        [Fact]
        public async Task CountAsync_All() {
            var result = await this.VerifyAsyncExecution(query => query.CountAsync());

            Assert.Equal(
                3,
                result
            );
        }

        [Fact]
        public async Task CountAsync_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.CountAsync(m => m.ExpirationDate > new DateTime(2018, 12, 7))
            );

            Assert.Equal(
                2,
                result
            );
        }

        [Fact]
        public async Task LongCountAsync_All() {
            var result = await this.VerifyAsyncExecution(query => query.LongCountAsync());

            Assert.Equal(
                3L,
                result
            );
        }

        [Fact]
        public async Task LongCountAsync_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.LongCountAsync(m => m.ExpirationDate > new DateTime(2018, 12, 7))
            );

            Assert.Equal(
                2L,
                result
            );
        }

        [Fact]
        public async Task Average() {
            var result = await this.VerifyAsyncExecution(
                query => query.AverageAsync(m => m.LotSize)
            );

            Assert.Equal(
                23d,
                result,
                10
            );
        }

        [Fact]
        public async Task Sum() {
            var result = await this.VerifyAsyncExecution(
                query => query.SumAsync(m => m.LotSize)
            );

            Assert.Equal(
                69,
                result
            );
        }

        [Fact]
        public async Task Min_Selector() {
            var result = await this.VerifyAsyncExecution(
                query => query.MinAsync(m => m.LotSize)
            );

            Assert.Equal(
                7,
                result
            );
        }

        [Fact]
        public async Task Min_Item() {
            var result = await this.VerifyAsyncExecution(
                query => query.Select(m => m.LotSize).MinAsync()
            );

            Assert.Equal(
                7,
                result
            );
        }

        [Fact]
        public async Task Max_Selector() {
            var result = await this.VerifyAsyncExecution(
                query => query.MaxAsync(m => m.LotSize)
            );

            Assert.Equal(
                42,
                result
            );
        }

        [Fact]
        public async Task Max_Item() {
            var result = await this.VerifyAsyncExecution(
                query => query.Select(m => m.LotSize).MaxAsync()
            );

            Assert.Equal(
                42,
                result
            );
        }

        [Fact]
        public async Task Aggregate() {
            var result = await this.VerifyAsyncExecution(
                query => query.Select(m => m.LotSize).AggregateAsync((x, y) => x + y)
            );

            Assert.Equal(
                69,
                result
            );
        }

        [Fact]
        public async Task Aggregate_Seeded() {
            var result = await this.VerifyAsyncExecution(
                query => query.AggregateAsync(0, (sum, model) => sum + model.LotSize)
            );

            Assert.Equal(
                69,
                result
            );
        }

        [Fact]
        public async Task Aggregate_Seeded_ResultSelector() {
            var result = await this.VerifyAsyncExecution(
                query => query.AggregateAsync(0, (sum, model) => sum + model.LotSize, sum => sum / 3)
            );

            Assert.Equal(
                23,
                result
            );
        }

        [Fact]
        public async Task Any() {
            var result = await this.VerifyAsyncExecution(
                query => query.AnyAsync()
            );

            Assert.True(
                result
            );
        }

        [Fact]
        public async Task Any_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.AnyAsync(m => m.InStock)
            );

            Assert.True(
                result
            );
        }

        [Fact]
        public async Task Any_Filtered_False() {
            var result = await this.VerifyAsyncExecution(
                query => query.AnyAsync(m => m.LotSize > 1000)
            );

            Assert.False(
                result
            );
        }

        [Fact]
        public async Task All_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.AllAsync(m => m.Name.Length == 3)
            );

            Assert.True(
                result
            );
        }

        [Fact]
        public async Task All_Filtered_False() {
            var result = await this.VerifyAsyncExecution(
                query => query.AllAsync(m => m.InStock)
            );

            Assert.False(
                result
            );
        }

        [Fact]
        public async Task Single() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name == "Bar").SingleAsync()
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task Single_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(query => query.SingleAsync())
            );
        }

        [Fact]
        public async Task Single_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.SingleAsync(m => m.Name == "Bar")
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task Single_Filtered_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(query => query.SingleAsync(m => !m.InStock))
            );
        }

        [Fact]
        public async Task SingleOrDefault() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name == "Bar").SingleOrDefaultAsync()
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task SingleOrDefault_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(query => query.SingleOrDefaultAsync())
            );
        }

        [Fact]
        public async Task SingleOrDefault_Default() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name == "Fhgwgds").SingleOrDefaultAsync()
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task SingleOrDefault_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.SingleOrDefaultAsync(m => m.Name == "Bar")
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task SingleOrDefault_Filtered_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(query => query.SingleOrDefaultAsync(m => !m.InStock))
            );
        }

        [Fact]
        public async Task SingleOrDefault_Filtered_Default() {
            var result = await this.VerifyAsyncExecution(
                query => query.SingleOrDefaultAsync(m => m.Name == "Fhgwgds")
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task First() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name.StartsWith("B")).FirstAsync()
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task First_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(
                    query => query.Where(m => m.Name == "Fhgwgds").FirstAsync()
                )
            );
        }

        [Fact]
        public async Task First_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.FirstAsync(m => m.Name.StartsWith("B"))
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task First_Filtered_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(
                    query => query.FirstAsync(m => m.Name == "Fhgwgds")
                )
            );
        }

        [Fact]
        public async Task FirstOrDefault() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name.StartsWith("B")).FirstOrDefaultAsync()
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task FirstOrDefault_Default() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name == "Fhgwgds").FirstOrDefaultAsync()
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task FirstOrDefault_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.FirstOrDefaultAsync(m => m.Name.StartsWith("B"))
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task FirstOrDefault_Filtered_Default() {
            var result = await this.VerifyAsyncExecution(
                query => query.FirstOrDefaultAsync(m => m.Name == "Fhgwgds")
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task Last() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name.StartsWith("B")).LastAsync()
            );

            Assert.Equal(
                this.Items[2],
                result
            );
        }

        [Fact]
        public async Task Last_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(
                    query => query.Where(m => m.Name == "Fhgwgds").LastAsync()
                )
            );
        }

        [Fact]
        public async Task Last_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.LastAsync(m => m.Name.StartsWith("B"))
            );

            Assert.Equal(
                this.Items[2],
                result
            );
        }

        [Fact]
        public async Task Last_Filtered_Exception() {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.VerifyAsyncExecution(
                    query => query.LastAsync(m => m.Name == "Fhgwgds")
                )
            );
        }

        [Fact]
        public async Task LastOrDefault() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name.StartsWith("B")).LastOrDefaultAsync()
            );

            Assert.Equal(
                this.Items[2],
                result
            );
        }

        [Fact]
        public async Task LastOrDefault_Default() {
            var result = await this.VerifyAsyncExecution(
                query => query.Where(m => m.Name == "Fhgwgds").LastOrDefaultAsync()
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task LastOrDefault_Filtered() {
            var result = await this.VerifyAsyncExecution(
                query => query.LastOrDefaultAsync(m => m.Name.StartsWith("B"))
            );

            Assert.Equal(
                this.Items[2],
                result
            );
        }

        [Fact]
        public async Task LastOrDefault_Filtered_Default() {
            var result = await this.VerifyAsyncExecution(
                query => query.LastOrDefaultAsync(m => m.Name == "Fhgwgds")
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task ElementAt() {
            var result = await this.VerifyAsyncExecution(
                query => query.ElementAtAsync(1)
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task ElementAt_Exception() {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => this.VerifyAsyncExecution(
                    query => query.ElementAtAsync(3)
                )
            );
        }

        [Fact]
        public async Task ElementAt_Exception_NegativeIndex() {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => this.VerifyAsyncExecution(
                    query => query.ElementAtAsync(-1)
                )
            );
        }

        [Fact]
        public async Task ElementAtOrDefault() {
            var result = await this.VerifyAsyncExecution(
                query => query.ElementAtOrDefaultAsync(1)
            );

            Assert.Equal(
                this.Items[1],
                result
            );
        }

        [Fact]
        public async Task ElementAtOrDefault_Default() {
            var result = await this.VerifyAsyncExecution(
                query => query.ElementAtOrDefaultAsync(3)
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task ElementAtOrDefault_Default_NegativeIndex() {
            var result = await this.VerifyAsyncExecution(
                query => query.ElementAtOrDefaultAsync(-1)
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task Contains() {
            var result = await this.VerifyAsyncExecution(
                query => query.ContainsAsync(this.Items[1])
            );

            Assert.True(result);
        }

        [Fact]
        public async Task Contains_False() {
            var result = await this.VerifyAsyncExecution(
                query => query.ContainsAsync(new TestModel())
            );

            Assert.False(result);
        }

        [Fact]
        public async Task Contains_EqualityComparer() {
            var result = await this.VerifyAsyncExecution(
                query => query.ContainsAsync(
                    new TestModel { Name = "Bar" },
                    TestModelNameEqualityComparer.Instance
                )
            );

            Assert.True(result);
        }

        [Fact]
        public async Task Contains_EqualityComparer_False() {
            var result = await this.VerifyAsyncExecution(
                query => query.ContainsAsync(
                    new TestModel { Name = "Fhgwgds" },
                    TestModelNameEqualityComparer.Instance
                )
            );

            Assert.False(result);
        }

        [Fact]
        public async Task SequenceEqual() {
            var result = await this.VerifyAsyncExecution(
                query => query.SequenceEqualAsync(this.Items)
            );

            Assert.True(result);
        }

        [Fact]
        public async Task SequenceEqual_False() {
            var result = await this.VerifyAsyncExecution(
                query => query.SequenceEqualAsync(new[] { new TestModel() })
            );

            Assert.False(result);
        }

        [Fact]
        public async Task SequenceEqual_EqualityComparer() {
            var result = await this.VerifyAsyncExecution(
                query => query.SequenceEqualAsync(
                    new[] {
                        new TestModel { Name = "Foo" },
                        new TestModel { Name = "Bar" },
                        new TestModel { Name = "Baz" }
                    },
                    TestModelNameEqualityComparer.Instance
                )
            );

            Assert.True(result);
        }

        [Fact]
        public async Task SequenceEqual_EqualityComparer_False() {
            var result = await this.VerifyAsyncExecution(
                query => query.SequenceEqualAsync(
                    new[] { new TestModel { Name = "Fhgwgds" } },
                    TestModelNameEqualityComparer.Instance
                )
            );

            Assert.False(result);
        }

        protected abstract Task<TResult> VerifyAsyncExecution<TResult>(
            Func<IQueryable<TestModel>, Task<TResult>> test);


        protected class TestModel {
            public Guid Id { get; } = Guid.NewGuid();
            public String Name { get; set; }
            public Boolean InStock { get; set; }
            public DateTime ExpirationDate { get; set; }
            public Int32 LotSize { get; set; }
            public Decimal Price { get; set; }

            public override bool Equals(object obj) {
                return !ReferenceEquals(obj, null) &&
                    obj is TestModel other &&
                    other.Id == this.Id &&
                    other.Name == this.Name &&
                    other.InStock == this.InStock &&
                    other.ExpirationDate == this.ExpirationDate &&
                    other.LotSize == this.LotSize &&
                    other.Price == this.Price;
            }

            public override int GetHashCode() {
                return HashCode.Combine(
                    this.Id,
                    this.Name,
                    this.InStock,
                    this.ExpirationDate,
                    this.LotSize,
                    this.Price
                );
            }
        }

        private class TestModelNameEqualityComparer : IEqualityComparer<TestModel> {
            public static TestModelNameEqualityComparer Instance { get; } = new TestModelNameEqualityComparer();

            private TestModelNameEqualityComparer() {
            }

            public bool Equals(TestModel x, TestModel y) {
                if (ReferenceEquals(x, null)) {
                    return ReferenceEquals(y, null);
                } else {
                    return !ReferenceEquals(y, null) &&
                        x.Name == y.Name;
                }
            }

            public int GetHashCode(TestModel model) {
                return HashCode.Combine(model?.Name);
            }
        }

    }
}
