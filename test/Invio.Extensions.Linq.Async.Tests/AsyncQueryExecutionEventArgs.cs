using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Invio.Extensions.Linq.Async.Tests {
    public class AsyncQueryExecutionEventArgs : EventArgs {
        public IQueryProvider Provider { get; }

        public Expression Expression { get; }

        public Task Handler { get; set; }

        public AsyncQueryExecutionEventArgs(IQueryProvider provider, Expression expression) {
            this.Provider = provider;
            this.Expression = expression;
        }
    }
}
