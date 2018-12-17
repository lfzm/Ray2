using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ray2.Internal
{
    public interface IDataflowBufferWrap<TData>
    {
        TData Data { get; }
        void Canceled();
        void Canceled(CancellationToken cancellationToken);
        void ExceptionHandler(Exception exception);
        void ExceptionHandler(IEnumerable<Exception> exception);
        void CompleteHandler(bool result);
        Task CompleteHandler(Task<bool> result);
        Task<bool> Wall();
    }
}
