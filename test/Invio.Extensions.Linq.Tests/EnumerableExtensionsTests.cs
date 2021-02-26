using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invio.Xunit;
using Xunit;

namespace Invio.Extensions.Linq {

    [UnitTest]
    public sealed class EnumerableExtensionsTests {

        [Fact]
        public void FindIndex_NoStartIndex_NullSource() {

            // Arrange

            IEnumerable<string> enumerable = null;
            Predicate<string> match = item => true;

            // Act

            var exception = Record.Exception(
                () => enumerable.FindIndex(match)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void FindIndex_NoStartIndex_NullMatch() {

            // Arrange

            IEnumerable<string> enumerable = new [] { "foo", "bar", "biz" };
            Predicate<string> match = null;

            // Act

            var exception = Record.Exception(
                () => enumerable.FindIndex(match)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void FindIndex_NoStartIndex_Empty() {

            // Arrange

            IEnumerable<string> enumerable = new string[0];
            Predicate<string> match = item => true;

            // Act

            var result = enumerable.FindIndex(match);

            // Assert

            Assert.Equal(-1, result);
        }

        [Theory]
        [InlineData("nope")]
        [InlineData("foo2")]
        [InlineData("bbar")]
        [InlineData(null)]
        [InlineData("but")]
        public void FindIndex_NoStartIndex_NoMatches(string itemMatch) {

            // Arrange

            IEnumerable<string> enumerable = new [] { "foo", "bar", "biz", "buz", "buk" };
            Predicate<string> match = item => item == itemMatch;

            // Act

            var result = enumerable.FindIndex(match);

            // Assert

            Assert.Equal(-1, result);
        }

        [Theory]
        [InlineData("foo", 0)]
        [InlineData("bar", 1)]
        [InlineData("biz", 2)]
        [InlineData("boo", 4)]
        public void FindIndex_NoStartIndex_WithMatches(string itemMatch, int expected) {

            // Arrange

            IEnumerable<string> enumerable = new [] { "foo", "bar", "biz", "bar", "boo" };
            Predicate<string> match = item => item == itemMatch;

            // Act

            var result = enumerable.FindIndex(match);

            // Assert

            Assert.Equal(expected, result);
        }

        [Fact]
        public void FindIndex_WithStartIndex_NullSource() {

            // Arrange

            IEnumerable<string> enumerable = null;
            Predicate<string> match = item => true;

            // Act

            var exception = Record.Exception(
                () => enumerable.FindIndex(0, match)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [InlineData(Int32.MinValue)]
        [InlineData(-1)]
        public void FindIndex_WithStartIndex_InvalidStartIndex(int invalidStartIndex) {

            // Arrange

            IEnumerable<string> enumerable = new [] { "foo", "bar", "biz" };
            Predicate<string> match = item => true;

            // Act

            var exception = Record.Exception(
                () => enumerable.FindIndex(invalidStartIndex, match)
            );

            // Assert

            var typed = Assert.IsType<ArgumentOutOfRangeException>(exception);
            Assert.Equal(invalidStartIndex, typed.ActualValue);
            Assert.Equal("startIndex", typed.ParamName);
        }

        [Fact]
        public void FindIndex_WithStartIndex_NullMatch() {

            // Arrange

            IEnumerable<string> enumerable = new [] { "foo", "bar", "biz" };
            Predicate<string> match = null;

            // Act

            var exception = Record.Exception(
                () => enumerable.FindIndex(0, match)
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void FindIndex_WithStartIndex_Empty() {

            // Arrange

            IEnumerable<string> enumerable = new string[0];
            Predicate<string> match = item => true;

            // Act

            var result = enumerable.FindIndex(0, match);

            // Assert

            Assert.Equal(-1, result);
        }

        [Theory]
        [InlineData(0, "nope")]
        [InlineData(1, "foo")]
        [InlineData(10, "foo")]
        [InlineData(3, "biz")]
        [InlineData(5, "bar")]
        public void FindIndex_WithStartIndex_NoMatches(int startIndex, string itemMatch) {

            // Arrange

            IEnumerable<string> enumerable = new [] { "foo", "bar", "biz", "buz", "bar" };
            Predicate<string> match = item => item == itemMatch;

            // Act

            var result = enumerable.FindIndex(startIndex, match);

            // Assert

            Assert.Equal(-1, result);
        }

        [Theory]
        [InlineData(0, "foo", 0)]
        [InlineData(0, "bar", 1)]
        [InlineData(1, "bar", 1)]
        [InlineData(2, "bar", 4)]
        [InlineData(4, "bar", 4)]
        [InlineData(2, "biz", 2)]
        [InlineData(2, "buz", 3)]
        public void FindIndex_WithStartIndex_WithMatches(
            int startIndex,
            string itemMatch,
            int expected) {

            // Arrange

            IEnumerable<string> enumerable = new [] { "foo", "bar", "biz", "buz", "bar" };
            Predicate<string> match = item => item == itemMatch;

            // Act

            var result = enumerable.FindIndex(startIndex, match);

            // Assert

            Assert.Equal(expected, result);
        }

        [Fact]
        public void UntypedEnumerable_Cycle_NullSource() {

            // Arrange

            IEnumerable enumerable = null;

            // Act

            var exception = Record.Exception(
                () => enumerable.Cycle().Cast<object>().Take(1).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void UntypedEnumerable_Cycle_EmptySource() {

            // Arrange

            IEnumerable enumerable =
                Enumerable
                    .Empty<object>()
                    .Cycle();

            // Act

            var exception = Record.Exception(
                () => enumerable.Cast<object>().Take(1).ToList()
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "The enumerable provided is empty." +
                Environment.NewLine + "Parameter name: source",
                exception.Message
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void UntypedEnumerable_Cycle_EnumeratesEndlessly(int initialCount) {

            // Arrange

            const int numberOfLoops = 3;

            IEnumerable enumerable =
                Enumerable
                    .Range(0, initialCount)
                    .Select(_ => new object())
                    .ToList()
                    .AsEnumerable();

            var infiniteEnumerator =
                enumerable
                    .Cycle()
                    .GetEnumerator();

            // Act

            var results = new List<object>();

            while (infiniteEnumerator.MoveNext()) {
                results.Add(infiniteEnumerator.Current);

                if (results.Count == initialCount * numberOfLoops) {
                    break;
                }
            }

            // Assert

            for (var loop = 0; loop < numberOfLoops; loop++) {
                Assert.Equal(
                    enumerable,
                    results.Skip(loop * initialCount).Take(initialCount)
                );
            }
        }

        [Fact]
        public void TypedEnumerable_Cycle_NullSource() {

            // Arrange

            IEnumerable<object> enumerable = null;

            // Act

            var exception = Record.Exception(
                () => enumerable.Cycle().Take(1).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void TypedEnumerable_Cycle_EmptySource() {

            // Arrange

            var enumerable =
                Enumerable
                    .Empty<object>()
                    .Cycle();

            // Act

            var exception = Record.Exception(
                () => enumerable.Take(1).ToList()
            );

            // Assert

            Assert.IsType<ArgumentException>(exception);

            Assert.Equal(
                "The enumerable provided is empty." +
                Environment.NewLine + "Parameter name: source",
                exception.Message
            );
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void TypedEnumerable_Cycle_EnumeratesEndlessly(int initialCount) {

            // Arrange

            const int numberOfLoops = 3;

            IEnumerable enumerable =
                Enumerable
                    .Range(0, initialCount)
                    .Select(_ => new object())
                    .ToList()
                    .AsEnumerable();

            var infiniteEnumerator =
                enumerable
                    .Cycle()
                    .GetEnumerator();

            // Act

            var results = new List<object>();

            while (infiniteEnumerator.MoveNext()) {
                results.Add(infiniteEnumerator.Current);

                if (results.Count == initialCount * numberOfLoops) {
                    break;
                }
            }

            // Assert

            for (var loop = 0; loop < numberOfLoops; loop++) {
                Assert.Equal(
                    enumerable,
                    results.Skip(loop * initialCount).Take(initialCount)
                );
            }
        }

        [Fact]
        public void ZipToTuple_FirstSequenceNull() {

            // Arrange

            IEnumerable<object> first = null;
            IEnumerable<object> second = new List<object> { new object() };

            // Act

            var exception = Record.Exception(
                () => first.ZipToTuple(second).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ZipToTuple_SecondSequenceNull() {

            // Arrange

            IEnumerable<object> first = new List<object> { new object() };
            IEnumerable<object> second = null;

            // Act

            var exception = Record.Exception(
                () => first.ZipToTuple(second).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ZipToTuple_CombinesVaryingLengths() {

            // Arrange

            IEnumerable<int> first = new List<int> { 1, 2, 3 };
            IEnumerable<string> second = new List<string> { "4", "5", "6", "7" };

            // Act

            var zipped = first.ZipToTuple(second).ToList();

            Assert.Equal(
                new List<Tuple<int, string>> {
                    Tuple.Create(1, "4"),
                    Tuple.Create(2, "5"),
                    Tuple.Create(3, "6")
                },
                zipped
            );
        }

        [Fact]
        public void Subsequences_Null() {

            // Arrange

            IEnumerable<object> source = null;

            // Act

            var exception = Record.Exception(
                () => source.Subsequences().ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        public static IEnumerable<object[]> Subsequences_ValidCases_MemberData {
            get {
                yield return new object[] {
                    new object[0],
                    new object[][] {
                        new object[0]
                    }
                };

                yield return new object[] {
                    new string[] { "foo" },
                    new string[][] {
                        new string[0],
                        new string[] { "foo" },
                    }
                };

                yield return new object[] {
                    new DateTime?[] { DateTime.MinValue, null },
                    new DateTime?[][] {
                        new DateTime?[0],
                        new DateTime?[] { DateTime.MinValue },
                        new DateTime?[] { null },
                        new DateTime?[] { DateTime.MinValue, null }
                    }
                };

                yield return new object[] {
                    new int[] { -2, 0, 5 },
                    new int[][] {
                        new int[0],
                        new int[] { -2 },
                        new int[] { 0 },
                        new int[] { 5 },
                        new int[] { -2, 0 },
                        new int[] { -2, 5 },
                        new int[] { 0, 5 },
                        new int[] { -2, 0, 5 },
                    }
                };

                yield return new object[] {
                    new string[] { "dupe", "dee", "dupe" },
                    new string[][] {
                        new string[0],
                        new string[] { "dupe" },
                        new string[] { "dee" },
                        new string[] { "dupe" },
                        new string[] { "dupe", "dee" },
                        new string[] { "dee", "dupe" },
                        new string[] { "dupe", "dupe" },
                        new string[] { "dupe", "dee", "dupe" }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(Subsequences_ValidCases_MemberData))]
        public void Subsequences_ValidCases<T>(
            IEnumerable<T> source,
            IEnumerable<IEnumerable<T>> expected) {

            // Arrange

            var comparer = new SequenceEqualityComparer<T>();
            var counts = new Dictionary<IEnumerable<T>, int>(comparer);

            foreach (var sequence in expected) {
                if (counts.TryGetValue(sequence, out int count)) {
                    counts[sequence] = count++;
                } else {
                    counts[sequence] = 1;
                }
            }

            // Act

            var actual = source.Subsequences().ToList();

            // Assert

            Assert.NotNull(actual);

            foreach (var sequence in actual) {
                Assert.Contains(sequence, expected, comparer);

                Assert.True(
                    counts.TryGetValue(sequence, out int count),
                    "Each expected subsequence should only be returned once, " +
                    "but the following sequence was returned twice: " +
                    Environment.NewLine +
                    "[ " + String.Join(", ", sequence.Select(elem => elem?.ToString())) + " ]"
                );

                counts[sequence] = count--;
            }

            Assert.Equal(expected.Count(), actual.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void Subsequences_UnusuallyLargeCase(int numberToTake) {

            // Arrange

            var source = Enumerable.Range(0, Int32.MaxValue);

            // Act

            var subsequences = source.Subsequences().Take(numberToTake).ToList();

            // Assert

            Assert.Equal(numberToTake, subsequences.Count());
        }

        [Fact]
        public void Batch_NullSource() {

            // Arrange

            IEnumerable<int> source = null;

            // Act

            var exception = Record.Exception(
                () => source.Batch(size: 5).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Batch_InvalidSizes(int invalidSize) {

            // Arrange

            var source = Enumerable.Range(0, 5);

            // Act

            var exception = Record.Exception(
                () => source.Batch(size: invalidSize).ToList()
            );

            // Assert

            var typed = Assert.IsType<ArgumentOutOfRangeException>(exception);

            Assert.StartsWith(
                "The 'size' must be a positive integer." +
                Environment.NewLine + "Parameter name: size",
                typed.Message
            );

            Assert.Equal(invalidSize, typed.ActualValue);
        }

        [Fact]
        public void Batch_Empty() {

            // Arrange

            var source = Enumerable.Empty<int>();

            // Act

            var batches = source.Batch(size: 1).ToList();

            // Assert

            Assert.Empty(batches);
        }

        [Theory]
        [InlineData(2, 4)]
        [InlineData(18, 10)]
        public void Batch_ImperfectDivision(int numberOfItems, int batchSize) {

            // Arrange

            var source = Enumerable.Range(0, numberOfItems);

            // Act

            var batches = source.Batch(batchSize).ToList();

            // Assert

            Assert.Equal(numberOfItems / batchSize + 1, batches.Count);
            Assert.Equal(numberOfItems % batchSize, batches.Last().Count());
        }

        [Theory]
        [InlineData(4, 4)]
        [InlineData(15, 5)]
        public void Batch_PerfectDivision(int numberOfItems, int batchSize) {

            // Arrange

            var source = Enumerable.Range(0, numberOfItems);

            // Act

            var batches = source.Batch(batchSize).ToList();

            // Assert

            Assert.Equal(numberOfItems / batchSize, batches.Count);
            Assert.Equal(batchSize, batches.Last().Count());
        }

        [Fact]
        public void DistinctBy_NullSource() {

            // Arrange

            IEnumerable<int> source = null;
            Func<int, int> keySelector = item => item;

            // Act

            var exception = Record.Exception(
                () => source.DistinctBy(keySelector).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void DistinctBy_NullKeySelector() {

            // Arrange

            IEnumerable<int> source = new [] { 1, 4, 6, 8 };
            Func<int, string> keySelector = null;

            // Act

            var exception = Record.Exception(
                () => source.DistinctBy(keySelector).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void DistinctBy_DoesNothingOnEmpty() {

            // Arrange

            IEnumerable<string> source = new string[0];

            // Act

            var results = source.DistinctBy(item => item).ToList();

            // Assert

            Assert.Empty(results);
        }

        [Fact]
        public void DistinctBy_ConsidersNullItemsDistinctFromNullKeys() {

            // Arrange

            IEnumerable<string> source = new [] { "biz", null, "foo", "bar" };
            Func<string, string> keySelector = item => item == "foo" ? null : item;

            // Act

            var results = source.DistinctBy(keySelector).ToList();

            // Assert

            Assert.Equal(new [] { "biz", null, "foo", "bar" }, results);
        }

        [Fact]
        public void DistinctBy_ConsidersNullItemsEqualKeys() {

            // Arrange

            IEnumerable<string> source = new [] { "biz", null, null, "bar", null };
            Func<string, string> keySelector = item => item == "foo" ? null : item;

            // Act

            var results = source.DistinctBy(keySelector).ToList();

            // Assert

            Assert.Equal(new [] { "biz", null, "bar" }, results);
        }

        [Fact]
        public void DistinctBy_ConsidersNullKeysEqual() {

            // Arrange

            IEnumerable<string> source = new [] { "biz", "FOO", "foo", "FUN", "bar" };
            Func<string, string> keySelector = item => item[0] == 'F' ? null : item;

            // Act

            var results = source.DistinctBy(keySelector).ToList();

            // Assert

            Assert.Equal(new [] { "biz", "FOO", "foo", "bar" }, results);
        }

        [Fact]
        public void DistinctBy_MaintainsOrderWhenNoMatches() {

            // Arrange

            IEnumerable<string> source = new [] { "FOO", "Foo", "FoO" };

            // Act

            var results = source.DistinctBy(item => item).ToList();

            // Assert

            Assert.Equal(source, results);
        }

        [Fact]
        public void DistinctBy_MaintainsOrderWithMatches() {

            // Arrange

            IEnumerable<string> source = new [] { "cow", "FOO", "Foo", "moon", "FoO", "bar" };

            // Act

            var results = source.DistinctBy(item => item.ToLower()).ToList();

            // Assert

            Assert.Equal(new [] { "cow", "FOO", "moon", "bar" }, results);
        }

        [Fact]
        public void DistinctBy_PreservesOriginalValues() {

            // Arrange

            IEnumerable<string> source = new [] { "FOO", "Foo", "FoO" };

            // Act

            var results = source.DistinctBy(item => item.ToLower()).ToList();

            // Assert

            var result = Assert.Single(results);
            Assert.Contains(result, source);
        }

        [Theory]
        [InlineData(new Int32[0], new Int32[0], new Int32[0])]
        [InlineData(new[] { 2, 4, 6 }, new[] { 2, 4, 6 }, new Int32[0])]
        [InlineData(new[] { 3, 5, 7 }, new Int32[0], new[] { 3, 5, 7 })]
        [InlineData(new[] { 3, 2, 5, 4, 6, 7 }, new[] { 2, 4, 6 }, new[] { 3, 5, 7 })]
        public void ToPartitions(Int32[] input, Int32[] even, Int32[] odd) {
            var (truePart, falsePart) = input.ToPartitions(i => i % 2 == 0);

            Assert.Equal(even, truePart);
            Assert.Equal(odd, falsePart);
        }

        private class SequenceEqualityComparer<T> : IEqualityComparer<IEnumerable<T>> {

            public int GetHashCode(IEnumerable<T> sequence) {
                return sequence.Count();
            }

            public bool Equals(IEnumerable<T> left, IEnumerable<T> right) {
                if (left.Count() != right.Count()) {
                    return false;
                }

                var leftOrdered = left.OrderBy(element => element);
                var rightOrdered = right.OrderBy(element => element);

                return leftOrdered.SequenceEqual(rightOrdered);
            }

        }


    }

}
