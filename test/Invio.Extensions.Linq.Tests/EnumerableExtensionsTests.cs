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

    }

}
