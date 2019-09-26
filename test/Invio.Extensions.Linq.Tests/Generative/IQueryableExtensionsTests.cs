using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Invio.Extensions.Linq.Generative {
    public class IQueryableExtensionsTests {
        private const String PersonOne = "one";
        private const String PersonTwo = "two";
        private const String PersonThree = "three";

        private static IQueryable<Email> TestEmails =
            new[] {
                new Email {
                    Id = 0,
                    Sender = PersonOne,
                    Recipient = PersonTwo,
                    Subject = "Oh Hai"
                },
                new Email {
                    Id = 1,
                    Sender = PersonOne,
                    Recipient = PersonThree,
                    Subject = "Reticulating Splines"
                },
                new Email {
                    Id = 2,
                    Sender = PersonTwo,
                    Recipient = PersonThree,
                    Subject = "Oh Bother"
                },
                new Email {
                    Id = 3,
                    Sender = PersonTwo,
                    Recipient = PersonOne,
                    Subject = "K Thnx Bai"
                },
            }.AsQueryable();

        [Theory]
        [InlineData(PersonOne, PersonThree, new[] { "Bai" }, new[] { 0, 1, 2, 3 })]
        [InlineData(PersonOne, null, new[] { "Bother", "Bai" }, new[] { 0, 1, 2, 3 })]
        [InlineData(PersonTwo, PersonTwo, new[] { "no-matches" }, new[] { 0, 2, 3 })]
        [InlineData(PersonThree, null, new[] { "no-matches" }, new Int32[0])]
        [InlineData(null, PersonOne, null, new[] { 3 })]
        [InlineData(null, null, null, new Int32[0])]
        public void TestWhereAny(
            String sender,
            String recipient,
            String[] subjectWords,
            Int32[] results) {

            var filter = new FilterOptions {
                Sender = sender,
                Recipient = recipient,
                SubjectWords = subjectWords
            };

            var queryResults = DynamicFilterExample.Filter(TestEmails, filter).ToList();

            Assert.Equal(
                results.ToImmutableHashSet(),
                queryResults.Select(e => e.Id).ToImmutableHashSet()
            );
        }
    }

    public static class DynamicFilterExample {
        // Returns all Emails matching any of the specified criteria
        public static IQueryable<Email> Filter(
            IQueryable<Email> emails,
            FilterOptions options) {
            var criteria = new List<Expression<Func<Email, Boolean>>>();
            if (options.Sender != null) {
                criteria.Add(e => e.Sender == options.Sender);
            }

            if (options.Recipient != null) {
                criteria.Add(e => e.Recipient == options.Recipient);
            }

            if (options.SubjectWords != null) {
                foreach (var word in options.SubjectWords) {
                    criteria.Add(e => e.Subject.Contains(word));
                }
            }

            return emails.WhereAny(criteria);
        }
    }

    public class FilterOptions {
        public String Sender { get; set; }
        public String Recipient { get; set; }
        public String[] SubjectWords { get; set; }
    }

    public class Email {
        public Int32 Id { get; set; }
        public String Sender { get; set; }
        public String Recipient { get; set; }
        public String Subject { get; set; }
    }
}
