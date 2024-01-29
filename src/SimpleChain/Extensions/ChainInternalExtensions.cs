namespace SimpleChain;

internal static class ChainInternalExtensions
{
    internal static Chain<TResult> AddNodeInternal<T, TResult>(this Chain<T> chain,
        Func<Task<T>, Task<TResult>> func, CancellationToken cancellationToken) => 
        new Chain<TResult>
        {
            Task = chain.Task.ContinueWith(func, cancellationToken).Unwrap(),
            CancellationToken = cancellationToken
        };

    internal static Chain AddNodeInternal<T>(this Chain<T> chain,
        Action<Task<T>> action, CancellationToken cancellationToken) => 
        new Chain
        {
            Task = chain.Task.ContinueWith(action, cancellationToken),
            CancellationToken = cancellationToken
        };

    internal static Chain<IEnumerable<TResult>> AddNodeInternal<T, TResult>(this Chain<IEnumerable<T>> chain,
        Func<Task<IEnumerable<T>>, Task<IEnumerable<TResult>>> func, CancellationToken cancellationToken) => 
        new Chain<IEnumerable<TResult>>
        {
            Task = chain.Task.ContinueWith(func, cancellationToken).Unwrap(),
            CancellationToken = cancellationToken
        };

    internal static Chain AddNodeInternal<T>(this Chain<IEnumerable<T>> chain,
        Action<Task<IEnumerable<T>>> action, CancellationToken cancellationToken) => 
        new Chain
        {
            Task = chain.Task.ContinueWith(action, cancellationToken),
            CancellationToken = cancellationToken
        };

    internal static Chain<IAsyncEnumerable<TResult>> AddNodeInternal<T, TResult>(
        this Chain<IAsyncEnumerable<T>> chain, Func<Task<IAsyncEnumerable<T>>, 
            Task<IAsyncEnumerable<TResult>>> func, CancellationToken cancellationToken) =>
        new Chain<IAsyncEnumerable<TResult>>
        {
            Task = chain.Task.ContinueWith(func, cancellationToken).Unwrap(),
            CancellationToken = cancellationToken
        };

    internal static Chain AddNodeInternal<T>(
        this Chain<IAsyncEnumerable<T>> chain, Action<Task<IAsyncEnumerable<T>>> action, 
        CancellationToken cancellationToken) =>
        new Chain
        {
            Task = chain.Task.ContinueWith(action, cancellationToken),
            CancellationToken = cancellationToken
        };
}