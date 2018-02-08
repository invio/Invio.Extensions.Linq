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
        public void Zip_FirstSequenceNull() {

            // Arrange

            IEnumerable<object> first = null;
            IEnumerable<object> second = new List<object> { new object() };

            // Act

            var exception = Record.Exception(
                () => first.Zip(second).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Zip_SecondSequenceNull() {

            // Arrange

            IEnumerable<object> first = new List<object> { new object() };
            IEnumerable<object> second = null;

            // Act

            var exception = Record.Exception(
                () => first.Zip(second).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Zip_CombinesVaryingLengths() {

            // Arrange

            IEnumerable<int> first = new List<int> { 1, 2, 3 };
            IEnumerable<string> second = new List<string> { "4", "5", "6", "7" };

            // Act

            var zipped = first.Zip(second).ToList();

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
