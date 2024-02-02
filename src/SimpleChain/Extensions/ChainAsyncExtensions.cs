using System.Runtime.CompilerServices;

namespace SimpleChain;

public static class ChainAsyncExtensions
{
    public static Chain<IAsyncEnumerable<T>> ToChain<T>(this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default) =>
        new Chain<IAsyncEnumerable<T>>
        {
            Task = Task.Run(() => source, cancellationToken),
            CancellationToken = cancellationToken
        };

    public static Chain<IAsyncEnumerable<TResult>> AddNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, TResult> func, CancellationToken? cancellationToken = null) => 
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            return Core(previousResult, cancellationToken ?? chain.CancellationToken);

            async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken ct)
            {
                await foreach (var item in source.WithCancellation(ct))
                {
                    yield return func(item, ct);
                }
            }
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain<IAsyncEnumerable<TResult>> AddNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, TResult> func, CancellationToken? cancellationToken = null) =>
        AddNode(chain, (obj, _) => func(obj), cancellationToken);

    public static Chain AddNode<T>(this Chain<IAsyncEnumerable<T>> chain,
        Action<T, CancellationToken> action, CancellationToken? cancellationToken = null) => 
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            await foreach (var item in previousResult.WithCancellation(cancellationToken ?? chain.CancellationToken))
            {
                action(item, cancellationToken ?? chain.CancellationToken);
            }
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain AddNode<T>(this Chain<IAsyncEnumerable<T>> chain,
        Action<T> action, CancellationToken? cancellationToken = null) =>
        AddNode(chain, (obj, _) => action(obj), cancellationToken);

    public static Chain<IAsyncEnumerable<TResult>> AddNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, Task<TResult>> func, CancellationToken? cancellationToken = null) => 
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            return Core(previousResult, cancellationToken ?? chain.CancellationToken);

            async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken ct)
            {
                await foreach (var item in source.WithCancellation(ct))
                {
                    yield return await func(item, ct);
                }
            }
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain<IAsyncEnumerable<TResult>> AddNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, Task<TResult>> func, CancellationToken? cancellationToken = null) =>
        AddNode(chain, (obj, _) => func(obj), cancellationToken);

    public static Chain<IAsyncEnumerable<TResult>> AddNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        int maxDegreeOfParallelism, Func<T, CancellationToken, TResult> func,
        CancellationToken? cancellationToken = null)
    {
        return chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            return Core(previousResult, cancellationToken ?? chain.CancellationToken);

            async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken ct)
            {
                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = cancellationToken ?? chain.CancellationToken,
                };

                await foreach (var item in source.WithCancellation(ct))
                {
                    yield return func(item, ct);
                }
            }
        }, cancellationToken ?? chain.CancellationToken);
    }

    public static Chain<IAsyncEnumerable<TResult>> AddNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        int maxDegreeOfParallelism, Func<T, TResult> func,
        CancellationToken? cancellationToken = null) =>
        AddNode(chain, maxDegreeOfParallelism, (obj, _) => func(obj), cancellationToken);

    public static Chain<IAsyncEnumerable<IEnumerable<T>>> Chunk<T>(this Chain<IAsyncEnumerable<T>> chain,
        int size, CancellationToken? cancellationToken = null)
    {
        return chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            return Core(previousResult, cancellationToken ?? chain.CancellationToken);

            async IAsyncEnumerable<IEnumerable<T>> Core(IAsyncEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken ct)
            {
                var objList = new List<T>(size);
                await foreach (var obj in source.WithCancellation(ct))
                {
                    objList.Add(obj);
                    if (objList.Count != size) continue;
                    yield return objList.AsEnumerable();
                    objList = new List<T>(size);
                }

                if (objList.Any())
                {
                    yield return objList.AsEnumerable();
                }
            }
        }, cancellationToken ?? chain.CancellationToken);
    }
}
