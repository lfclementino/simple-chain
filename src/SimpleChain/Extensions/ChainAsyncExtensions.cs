using System.Runtime.CompilerServices;

namespace SimpleChain;

public static class ChainAsyncExtensions
{
    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, ChainState, TResult> func) => 
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            return Core(previousResult, chain.CancellationToken);

            async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken ct)
            {
                await foreach (var item in source.WithCancellation(ct))
                {
                    ct.ThrowIfCancellationRequested();
                    yield return func(item, ct, chain.State);
                }
            }
        }, chain.CancellationToken);

    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, TResult> func) =>
        AddAsyncNode(chain, (obj, _, _) => func(obj));

    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, TResult> func) =>
        AddAsyncNode(chain, (obj, ct, _) => func(obj, ct));

    public static Chain AddAsyncNode<T>(this Chain<IAsyncEnumerable<T>> chain,
        Action<T, CancellationToken, ChainState> action) => 
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            await foreach (var item in previousResult.WithCancellation(chain.CancellationToken))
            {
                chain.CancellationToken.ThrowIfCancellationRequested();
                action(item, chain.CancellationToken, chain.State);
            }
        }, chain.CancellationToken);

    public static Chain AddAsyncNode<T>(this Chain<IAsyncEnumerable<T>> chain,
        Action<T> action) =>
        AddAsyncNode(chain, (obj, _, _) => action(obj));

    public static Chain AddAsyncNode<T>(this Chain<IAsyncEnumerable<T>> chain,
        Action<T, CancellationToken> action) =>
        AddAsyncNode(chain, (obj, ct, _) => action(obj, ct));

    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, ChainState, Task<TResult>> func) => 
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            return Core(previousResult, chain.CancellationToken);

            async IAsyncEnumerable<TResult> Core(IAsyncEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken ct)
            {
                await foreach (var item in source.WithCancellation(ct))
                {
                    ct.ThrowIfCancellationRequested();  
                    yield return await func(item, ct, chain.State);
                }
            }
        }, chain.CancellationToken);

    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, Task<TResult>> func) =>
        AddAsyncNode(chain, (obj, _, _) => func(obj));

    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, Task<TResult>> func) =>
        AddAsyncNode(chain, (obj, ct, _) => func(obj, ct));

    public static Chain<IAsyncEnumerable<IEnumerable<T>>> Chunk<T>(this Chain<IAsyncEnumerable<T>> chain,
        int size)
    {
        return chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            return Core(previousResult, chain.CancellationToken);

            async IAsyncEnumerable<IEnumerable<T>> Core(IAsyncEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken ct)
            {
                var objList = new List<T>(size);
                await foreach (var obj in source.WithCancellation(ct))
                {
                    ct.ThrowIfCancellationRequested(); 
                    objList.Add(obj);
                    if (objList.Count != size) continue;
                    yield return objList.AsEnumerable();
                    objList = new List<T>(size);
                }

                if (objList.Any())
                {
                    ct.ThrowIfCancellationRequested();
                    yield return objList.AsEnumerable();
                }
            }
        }, chain.CancellationToken);
    }
}
