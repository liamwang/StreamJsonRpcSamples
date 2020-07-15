using System.Threading;
using System.Threading.Tasks;

namespace TcpSample.Server
{
    internal static class TaskExtensions
    {
        internal static Task WaitAsync(this CancellationToken cancellationToken)
        {
            var cancelationTaskCompletionSource = new TaskCompletionSource<bool>();
            cancellationToken.Register(CancellationTokenCallback, cancelationTaskCompletionSource);
            return cancellationToken.IsCancellationRequested ? Task.CompletedTask : cancelationTaskCompletionSource.Task;
        }

        private static void CancellationTokenCallback(object taskCompletionSource)
        {
            ((TaskCompletionSource<bool>)taskCompletionSource).SetResult(true);
        }
    }
}
