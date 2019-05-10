using System;
using System.Collections.Generic;

namespace Invio.Extensions.Linq {

    /// <summary>
    ///   Converts a lambda into an implementation of <see cref="EqualityComparer{TSource}" />.
    /// </summary>
    internal class ProjectionEqualityComparer<TSource, TKey> : IEqualityComparer<TSource> {

        private Func<TSource, TKey> projection { get; }
        private IEqualityComparer<TKey> comparer { get; }

        public ProjectionEqualityComparer(Func<TSource, TKey> projection) {
            this.projection = projection;
            this.comparer = EqualityComparer<TKey>.Default;
        }

        public int GetHashCode(TSource obj) {
            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            return this.comparer.GetHashCode(projection(obj));
        }

        public bool Equals(TSource x, TSource y) {
            if (x == null || y == null) {
                return x == null && y == null;
            }
            
            return this.comparer.Equals(projection(x), projection(y));
        }

    }

}
