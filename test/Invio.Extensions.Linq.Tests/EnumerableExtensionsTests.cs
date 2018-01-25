using System;
using System.Collections.Generic;
using System.Linq;
using Invio.Xunit;
using Xunit;

namespace Invio.Extensions.Linq {

    [UnitTest]
    public sealed class EnumerableExtensionsTests {

        [Fact]
        public void GetInfiniteEnumerator_NullSource() {

            // Arrange

            IEnumerable<object> enumerable = null;

            // Act

            var exception = Record.Exception(
                () => enumerable.GetInfiniteEnumerator().MoveNext()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void GetInfiniteEnumerator_EmptySource() {

            // Arrange

            var enumerable =
                Enumerable
                    .Empty<object>()
                    .GetInfiniteEnumerator();

            // Act

            var exception = Record.Exception(
                () => enumerable.MoveNext()
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
        public void GetInfiniteEnumerator_EnumeratesEndlessly(int initialCount) {

            // Arrange

            const int numberOfLoops = 3;

            IEnumerable<object> enumerable =
                Enumerable
                    .Range(0, initialCount)
                    .Select(_ => new object())
                    .ToList()
                    .AsEnumerable();

            var infiniteEnumerator = enumerable.GetInfiniteEnumerator();

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
        public void AsInfiniteEnumerable_NullSource() {

            // Arrange

            IEnumerable<object> enumerable = null;

            // Act

            var exception = Record.Exception(
                () => enumerable.AsInfiniteEnumerable().Take(1).ToList()
            );

            // Assert

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void AsInfiniteEnumerable_EmptySource() {

            // Arrange

            var enumerable =
                Enumerable
                    .Empty<object>()
                    .AsInfiniteEnumerable();

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
        public void AsInfiniteEnumerable_EnumeratesEndlessly(int initialCount) {

            // Arrange

            const int numberOfLoops = 3;

            IEnumerable<object> enumerable =
                Enumerable
                    .Range(0, initialCount)
                    .Select(_ => new object())
                    .ToList()
                    .AsEnumerable();

            var infiniteEnumerator =
                enumerable
                    .AsInfiniteEnumerable()
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

    }

}
