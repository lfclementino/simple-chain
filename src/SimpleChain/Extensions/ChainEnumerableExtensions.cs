using System.Collections.Concurrent;

namespace SimpleChain;

public static class ChainEnumerableExtensions
{

    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        Func<T, CancellationToken, TResult> func, CancellationToken? cancellationToken = null) =>
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;
            return previousResult.Select(item => func(item, cancellationToken ??
                chain.CancellationToken));
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        Func<T, TResult> func, CancellationToken? cancellationToken = null) =>
        AddNode(chain, (obj, _) => func(obj), cancellationToken);

    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        Func<T, CancellationToken, Task<TResult>> func, CancellationToken? cancellationToken = null) =>
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;
            var result = new List<TResult>();

            foreach (var item in previousResult)
            {
                result.Add(await func(item, cancellationToken ?? chain.CancellationToken));
            }
            return result.AsEnumerable();
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        Func<T, Task<TResult>> func, CancellationToken? cancellationToken = null) =>
        AddNode(chain, (obj, _) => func(obj), cancellationToken);

    public static Chain AddNode<T>(this Chain<IEnumerable<T>> chain,
        Action<T, CancellationToken> action, CancellationToken? cancellationToken = null) =>
        chain.AddNodeInternal(async task =>
        {
            var previousResult = await task;

            foreach (var item in previousResult)
            {
                action(item, cancellationToken ?? chain.CancellationToken);
            }
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain AddNode<T>(this Chain<IEnumerable<T>> chain,
     Action<T> action, CancellationToken? cancellationToken = null) =>
        AddNode(chain, (obj, _) => action(obj), cancellationToken);

    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        int maxDegreeOfParallelism, Func<T, CancellationToken, TResult> func,
        CancellationToken? cancellationToken = null) =>
        chain.AddNodeInternal(async task =>
        {
            var result = new ConcurrentBag<TResult>();
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = cancellationToken ?? chain.CancellationToken,
            };

            var previousResult = await task;
            await Parallel.ForEachAsync(previousResult, options, (item, ct) =>
            {
                result.Add(func(item, ct));
                return ValueTask.CompletedTask;
            });

            return result.AsEnumerable();
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        int maxDegreeOfParallelism, Func<T, TResult> func, CancellationToken? cancellationToken = null) =>
        AddNode(chain, maxDegreeOfParallelism, (obj, _) => func(obj), cancellationToken);


    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        int maxDegreeOfParallelism, Func<T, CancellationToken, Task<TResult>> func,
        CancellationToken? cancellationToken = null) =>
        chain.AddNodeInternal(async task =>
        {
            var result = new ConcurrentBag<TResult>();
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = cancellationToken ?? chain.CancellationToken,
            };

            var previousResult = await task;

            await Parallel.ForEachAsync(previousResult, options, async (item, ct) =>
            {
                result.Add(await func(item, ct));
            });

            return result.AsEnumerable();
        }, cancellationToken ?? chain.CancellationToken);

    public static Chain<IEnumerable<TResult>> AddNode<T, TResult>(this Chain<IEnumerable<T>> chain,
        int maxDegreeOfParallelism, Func<T, Task<TResult>> func,
        CancellationToken? cancellationToken = null) =>
        AddNode(chain, maxDegreeOfParallelism, (obj, _) => func(obj), cancellationToken);

    public static Chain AddNode<T>(this Chain<IEnumerable<T>> chain,
      int maxDegreeOfParallelism, Action<T, CancellationToken> action,
      CancellationToken? cancellationToken = null) =>
      chain.AddNodeInternal(async task =>
      {
          var options = new ParallelOptions()
          {
              MaxDegreeOfParallelism = maxDegreeOfParallelism,
              CancellationToken = cancellationToken ?? chain.CancellationToken,
          };

          var previousResult = await task;

          await Parallel.ForEachAsync(previousResult, options, (item, ct) =>
          {
              action(item, ct);
              return ValueTask.CompletedTask;
          });
      }, cancellationToken ?? chain.CancellationToken);

    public static Chain AddNode<T>(this Chain<IEnumerable<T>> chain,
      int maxDegreeOfParallelism, Action<T> action,
      CancellationToken? cancellationToken = null) =>
        AddNode(chain, maxDegreeOfParallelism, (obj, _) => action(obj), cancellationToken);
}
