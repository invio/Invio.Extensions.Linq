using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Invio.Extensions.Linq.Async.Tests {
    public class AsyncQueryableTest : AsyncQueryableTestBase {
        private TestAsyncQueryProvider<TestModel> Provider { get; }
        protected override IQueryable<TestModel> Query { get; }

        public AsyncQueryableTest() {
            this.Provider = new TestAsyncQueryProvider<TestModel>(this.Items);
            this.Query = this.Provider.CreateQuery();
        }

        protected override async Task<TResult> VerifyAsyncExecution<TResult>(
            Func<IQueryable<TestModel>, Task<TResult>> test) {

            using (var executionStartedSemaphore = new SemaphoreSlim(0, 1))
            using (var continueExectionSemaphore = new SemaphoreSlim(0, 1)) {
                EventHandler<AsyncQueryExecutionEventArgs> handler;
                this.Provider.QueryExecuting +=
                    handler = (sender, args) => {
                        executionStartedSemaphore.Release();
                        args.Handler = continueExectionSemaphore.WaitAsync();
                    };

                try {
                    var toListTask = test(this.Query);

                    // Ensure that execution has started asynchronously
                    await executionStartedSemaphore.WaitAsync();
                    // Check if the task completes prematurely, before the event handler Task completes
                    var completedPrematurely = toListTask.IsCompleted;
                    // Allow the event handler Task to complete
                    continueExectionSemaphore.Release();

                    var result = await toListTask;

                    Assert.False(completedPrematurely);
                    return result;
                } finally  {
                    this.Provider.QueryExecuting -= handler;
                }
            }
        }
    }
}
