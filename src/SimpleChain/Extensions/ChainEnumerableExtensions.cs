using System.Collections.Concurrent;

namespace SimpleChain;

public static class ChainEnumerableExtensions
{
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
