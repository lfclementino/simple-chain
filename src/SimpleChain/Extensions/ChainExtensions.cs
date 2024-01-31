using System.Runtime.CompilerServices;

namespace SimpleChain;

public static class ChainExtensions
{
    public static Chain<T> ToChain<T>(this Task<T> source, CancellationToken cancellationToken = default)
    {
        return new Chain<T>
        {
            Task = source,
            CancellationToken = cancellationToken
        };
    }

    public static Chain<T> ToChain<T>(this T source, CancellationToken cancellationToken = default) 
        where T : new() =>
        Task.Factory.StartNew(() => source, cancellationToken)
        .ToChain(cancellationToken);

    public static TaskAwaiter<T> GetAwaiter<T>(this Chain<T> chain) => chain.Task.GetAwaiter();

    public static TaskAwaiter GetAwaiter(this Chain chain) => chain.Task.GetAwaiter();

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain, Func<T, CancellationToken, TResult> func,
        CancellationToken? cancellationToken = null) where T : new() =>
        chain.AddNodeInternal(async task =>
            func(await task, cancellationToken ?? chain.CancellationToken),
            cancellationToken ?? chain.CancellationToken);

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain, Func<T, TResult> func,
        CancellationToken? cancellationToken = null) where T : new() =>
        AddNode(chain, (obj, _) => func(obj), cancellationToken);

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain,
        Func<T, CancellationToken, Task<TResult>> func, CancellationToken? cancellationToken = null) where T : new() =>
        chain.AddNodeInternal(async task =>
            await func(await task, cancellationToken ?? chain.CancellationToken),
            cancellationToken ?? chain.CancellationToken);
    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain, Func<T, Task<TResult>> func,
        CancellationToken? cancellationToken = null) where T : new() =>
        AddNode(chain, (obj, _) => func(obj), cancellationToken);

    public static Chain AddNode<T>(this Chain<T> chain, Action<T, CancellationToken> action,
        CancellationToken? cancellationToken = null) where T : new() =>
        chain.AddNodeInternal(async task =>
            action(await task, cancellationToken ?? chain.CancellationToken),
            cancellationToken ?? chain.CancellationToken);

    public static Chain AddNode<T>(this Chain<T> chain, Action<T> action,
        CancellationToken? cancellationToken = null) where T : new() =>
        AddNode(chain, (obj, _) => action(obj), cancellationToken);
}