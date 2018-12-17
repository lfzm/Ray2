using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ray2.Internal
{
    public class DataflowBufferWrap<TData> : IDataflowBufferWrap<TData>
    {
        private TaskCompletionSource<bool> TaskSource;

        public DataflowBufferWrap()
        {
            this.TaskSource = new TaskCompletionSource<bool>();
        }
        public DataflowBufferWrap(TData data) : this()
        {
            this.Data = data;
        }
        public TData Data { get; }

        public void Canceled()
        {
            this.TaskSource.SetCanceled();
        }

        public void Canceled(CancellationToken cancellationToken)
        {
            this.TaskSource.TrySetCanceled(cancellationToken);
        }

        public void CompleteHandler(bool result)
        {
            this.TaskSource.SetResult(result);
        }

        public Task CompleteHandler(Task<bool> result)
        {
            return result.ContinueWith((res) =>
             {
                 try
                 {
                     var r = res.GetAwaiter().GetResult();
                     this.CompleteHandler(r);
                 }
                 catch (Exception ex)
                 {
                     this.ExceptionHandler(ex);
                 }

             });
        }

        public void ExceptionHandler(Exception exception)
        {
            this.TaskSource.SetException(exception);
        }

        public void ExceptionHandler(IEnumerable<Exception> exceptions)
        {
            this.TaskSource.SetException(exceptions);
        }

        public Task<bool> Wall()
        {
            return this.TaskSource.Task;
        }
    }
}
