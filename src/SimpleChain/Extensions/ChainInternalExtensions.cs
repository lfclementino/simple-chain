namespace SimpleChain;

internal static class ChainInternalExtensions
{
    internal static Chain<TResult> AddNodeInternal<T, TResult>(this Chain<T> chain,
        Func<Task<T>, Task<TResult>> func, CancellationToken cancellationToken) =>
        new(chain.Task.ContinueWith(task =>
                {
                    chain.CancellationToken.ThrowIfCancellationRequested();
                    return func(task);
                }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap(),
            chain.State);

    internal static Chain AddNodeInternal<T>(this Chain<T> chain,
        Action<Task<T>> action, CancellationToken cancellationToken) =>
        new(chain.Task.ContinueWith(task =>
                {
                    chain.CancellationToken.ThrowIfCancellationRequested();
                    action(task);
                }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default),
            chain.State);

    internal static Chain<T> AddHandlerNodeInternal<T>(this Chain<T> chain,
        Func<Task<T>, Task<bool>> func, CancellationToken cancellationToken) =>
        new(chain.Task.ContinueWith(async task =>
        {
            var obj = await task;
            if (!chain.State.IsHandled)
            {
                var result = await func(task);
                if (result) chain.State.SetHandled();
            }
            return obj;
        }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap(),
            chain.State);

    internal static Chain AddHandlerNodeInternal<T>(this Chain<T> chain,
        Action<Task<T>> action, CancellationToken cancellationToken) =>
        new(chain.Task.ContinueWith(task => {
            if (!chain.State.IsHandled)
            {
                action(task);
            }
        }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default),
            chain.State);
}