namespace SimpleChain;

internal static class ChainInternalExtensions
{
    internal static Chain<TResult> AddNodeInternal<T, TResult>(this Chain<T> chain,
        Func<Task<T>, Task<TResult>> func, CancellationToken cancellationToken) => 
        new(chain.Task.ContinueWith(task =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return func(task);
                }, cancellationToken, 
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap(),
            chain.State);

    internal static Chain AddNodeInternal<T>(this Chain<T> chain,
        Action<Task<T>> action, CancellationToken cancellationToken) => 
        new(chain.Task.ContinueWith(task =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    action(task);
                }, cancellationToken, 
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default),
            chain.State);
}