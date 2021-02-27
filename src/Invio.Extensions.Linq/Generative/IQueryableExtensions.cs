using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Invio.Extensions.Linq.Generative {
    /// <summary>
    /// Extensions methods on <see cref="IQueryable{T}" /> meant for dynamically
    /// generating and using query expressions.
    /// </summary>
    public static class IQueryableExtensions {
        /// <summary>
        /// Returns a predicate expression for the type that an
        /// <see cref="IQueryable{T}" /> is querying. This is useful for
        /// constructing predicate expressions for anonymous types.
        /// </summary>
        /// <param name="queryable">
        /// An <see cref="IQueryable{T}" /> instance for which the predicate is
        /// generated. Note: this is only used to establish the type for the
        /// lambda expression. The resulting expression is not explicitly linked
        /// to this <see cref="IQueryable{T}" /> instance.</param>
        /// <param name="predicate">
        /// A lambda expression for the desired predicate.
        /// </param>
        /// <typeparam name="T">The type being filtered.</typeparam>
        /// <returns>A predicate expression.</returns>
        public static Expression<Func<T, Boolean>> MakePredicate<T>(
            this IQueryable<T> queryable,
            Expression<Func<T, Boolean>> predicate) {

            return predicate;
        }

        /// <summary>
        /// Filters an <see cref="IQueryable{T}" /> by a disjunction of a set
        /// of dynamically generated predicates.
        /// </summary>
        /// <remarks>
        /// This is useful when conditionally applying a set of disjunctive
        /// filters based on inputs known and query construction time. This
        /// generates simpler queries when some conditions can be performed
        /// a query generation time, and allows for the generation of
        /// disjunctions with variable number of clauses.
        /// </remarks>
        /// <example>
        /// <code>
        /// public static class DynamicFilterExample {
        ///   // Returns all Emails matching any of the specified criteria
        ///   public static IQueryable&lt;Email> Filter(
        ///     IQueryable&lt;Email> emails,
        ///     FilterOptions options) {
        ///
        ///     var criteria = new List&lt;Expression&lt;Func&lt;Email, Boolean>>>();
        ///     if (options.Sender != null) {
        ///       criteria.Add(e => e.Sender == options.Sender);
        ///     }
        ///     if (options.Recipient != null) {
        ///       criteria.Add(e => e.Recipient == options.Recipient;
        ///     }
        ///     if (options.SubjectWords != null) {
        ///       foreach (var word in options.SubjectWords) {
        ///         criteria.Add(e => e.Subject.Contains(word));
        ///       }
        ///     }
        ///
        ///     return emails.WhereAny(criteria);
        ///   }
        /// }
        ///
        /// public class FilterOptions {
        ///   public String Sender { get; set; }
        ///   public String Recipient { get; set; }
        ///   public String[] SubjectWords { get; set; }
        /// }
        ///
        /// public class Email {
        ///   public String Sender { get; set; }
        ///   public String Recipient { get; set; }
        ///   public String Subject { get; set; }
        ///   public String Body { get; set; }
        /// }
        /// </code>
        /// </example>
        /// <param name="queryable"></param>
        /// <param name="predicates"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IQueryable<T> WhereAny<T>(
            this IQueryable<T> queryable,
            IEnumerable<Expression<Func<T, Boolean>>> predicates) {

            if (queryable == null) {
                throw new ArgumentNullException(nameof(queryable));
            }
            if (predicates == null) {
                throw new ArgumentNullException(nameof(predicates));
            }

            // Combine predicates as a disjunction
            var disjunction = predicates.DefaultIfEmpty().Aggregate(Or);

            return disjunction != null ?
                queryable.Where(disjunction) :
                Enumerable.Empty<T>().AsQueryable();
        }

        private static Expression<Func<T, Boolean>> Or<T>(
            Expression<Func<T, Boolean>> expr1,
            Expression<Func<T, Boolean>> expr2) {

            if (expr1 == null) {
                throw new ArgumentNullException(nameof(expr1));
            }
            if (expr2 == null) {
                throw new ArgumentNullException(nameof(expr2));
            }

            var p1 = expr1.Parameters.Single();
            var p2 = expr2.Parameters.Single();
            var param = Expression.Parameter(typeof(T));

            return Expression.Lambda<Func<T, Boolean>>(
                Expression.OrElse(
                    ParamReplacingVisitor.Replace(expr1.Body, p1, param),
                    ParamReplacingVisitor.Replace(expr2.Body, p2, param)),
                param
            );
        }

        private class ParamReplacingVisitor : ExpressionVisitor {
            private readonly ParameterExpression original;
            private readonly ParameterExpression replacement;

            private ParamReplacingVisitor(
                ParameterExpression original,
                ParameterExpression replacement) {

                this.original = original;
                this.replacement = replacement;
            }

            protected override Expression VisitParameter(ParameterExpression node) {
                return this.original == node ? replacement : base.VisitParameter(node);
            }

            public static Expression Replace(
                Expression expr,
                ParameterExpression original,
                ParameterExpression replacement) {

                return new ParamReplacingVisitor(original, replacement).Visit(expr);
            }
        }
    }
}
