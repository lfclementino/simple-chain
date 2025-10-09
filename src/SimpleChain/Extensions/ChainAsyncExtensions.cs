using System.Runtime.CompilerServices;

namespace SimpleChain;

public static class ChainAsyncExtensions
{
    /// <summary>
    /// Adds an asynchronous processing node to the chain, transforming each element of the input sequence into a result
    /// using the specified function.
    /// </summary>
    /// <remarks>This method adds a node that processes elements of the input sequence asynchronously. The
    /// provided function is invoked for each element, and the resulting values are yielded as an asynchronous sequence.
    /// The chain's cancellation token is observed to handle cancellation requests.</remarks>
    /// <typeparam name="T">The type of elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The type of elements in the output sequence.</typeparam>
    /// <param name="chain">The chain to which the asynchronous processing node will be added.</param>
    /// <param name="func">A function that processes each element of the input sequence. The function takes the input element, a <see
    /// cref="CancellationToken"/> to observe cancellation requests, and the current chain state, and returns the
    /// transformed result.</param>
    /// <returns>A new chain containing the added asynchronous processing node, which produces a sequence of transformed results.</returns>
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

    /// <summary>
    /// Adds an asynchronous processing node to the chain, transforming each element of the input sequence using the
    /// specified synchronous function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The type of elements in the output sequence.</typeparam>
    /// <param name="chain">The chain of asynchronous sequences to which the processing node is added.</param>
    /// <param name="func">A synchronous function that transforms each element of the input sequence.</param>
    /// <returns>A new chain with the added processing node, where each element of the input sequence is transformed by <paramref
    /// name="func"/>.</returns>
    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, TResult> func) =>
        AddAsyncNode(chain, (obj, _, _) => func(obj));

    /// <summary>
    /// Adds an asynchronous processing node to the chain, allowing transformation of elements  in the sequence using
    /// the specified function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The type of elements in the output sequence.</typeparam>
    /// <param name="chain">The chain of asynchronous sequences to which the node will be added.</param>
    /// <param name="func">A function that processes each element of the input sequence asynchronously,  taking a cancellation token and
    /// returning the transformed result.</param>
    /// <returns>A new chain with the added asynchronous processing node.</returns>
    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, TResult> func) =>
        AddAsyncNode(chain, (obj, ct, _) => func(obj, ct));

    /// <summary>
    /// Adds a node to the asynchronous processing chain that performs the specified action on each item in the sequence
    /// produced by the previous node.
    /// </summary>
    /// <remarks>This method extends the asynchronous processing chain by adding a node that iterates over the
    /// items in the sequence produced by the previous node. The specified <paramref name="action"/> is invoked for each
    /// item, allowing custom processing logic to be applied. The operation respects cancellation requests via the
    /// provided <see cref="CancellationToken"/>.</remarks>
    /// <typeparam name="T">The type of the items in the asynchronous sequence.</typeparam>
    /// <param name="chain">The current asynchronous processing chain.</param>
    /// <param name="action">An action to perform on each item in the sequence. The action receives the item, a <see
    /// cref="CancellationToken"/> to monitor for cancellation requests, and the current <see cref="ChainState"/>.</param>
    /// <returns>A new <see cref="Chain"/> instance with the added node that processes each item in the sequence asynchronously.</returns>
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

    /// <summary>
    /// Adds an asynchronous processing node to the chain that performs the specified action on each item in the
    /// sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the asynchronous sequence.</typeparam>
    /// <param name="chain">The chain to which the asynchronous processing node will be added.</param>
    /// <param name="action">The action to perform on each element in the sequence.</param>
    /// <returns>The updated chain with the added asynchronous processing node.</returns>
    public static Chain AddAsyncNode<T>(this Chain<IAsyncEnumerable<T>> chain,
        Action<T> action) =>
        AddAsyncNode(chain, (obj, _, _) => action(obj));

    /// <summary>
    /// Adds an asynchronous processing node to the chain, allowing each item in the asynchronous sequence  to be
    /// processed using the specified action.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the asynchronous sequence.</typeparam>
    /// <param name="chain">The chain to which the asynchronous processing node will be added.</param>
    /// <param name="action">The action to perform on each element of the asynchronous sequence.  The action receives the element and a <see
    /// cref="CancellationToken"/> to handle cancellation.</param>
    /// <returns>The updated chain with the added asynchronous processing node.</returns>
    public static Chain AddAsyncNode<T>(this Chain<IAsyncEnumerable<T>> chain,
        Action<T, CancellationToken> action) =>
        AddAsyncNode(chain, (obj, ct, _) => action(obj, ct));

    /// <summary>
    /// Adds an asynchronous processing node to the chain, transforming each element of the input sequence into an
    /// output sequence using the specified asynchronous function.
    /// </summary>
    /// <remarks>This method allows you to extend the chain with a node that performs asynchronous processing
    /// on each element of the input sequence. The processing function is invoked for each element, and the resulting
    /// values are yielded as an asynchronous sequence.</remarks>
    /// <typeparam name="T">The type of elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The type of elements in the output sequence.</typeparam>
    /// <param name="chain">The chain to which the asynchronous processing node will be added.</param>
    /// <param name="func">A function that processes each element of the input sequence asynchronously. The function takes an input element
    /// of type <typeparamref name="T"/>, a <see cref="CancellationToken"/>, and the current <see cref="ChainState"/>,
    /// and returns a <see cref="Task{TResult}"/> representing the asynchronous operation.</param>
    /// <returns>A new chain containing the added asynchronous processing node. The output sequence of the node contains elements
    /// of type <typeparamref name="TResult"/> produced by the specified function.</returns>
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

    /// <summary>
    /// Adds an asynchronous processing node to the chain, allowing each element in the sequence      to be transformed
    /// asynchronously using the specified function.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the resulting sequence.</typeparam>
    /// <param name="chain">The chain of asynchronous sequences to which the processing node will be added.</param>
    /// <param name="func">A function that asynchronously transforms each element of type <typeparamref name="T"/>      into a result of
    /// type <typeparamref name="TResult"/>.</param>
    /// <returns>A new chain containing the transformed asynchronous sequence.</returns>
    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, Task<TResult>> func) =>
        AddAsyncNode(chain, (obj, _, _) => func(obj));

    /// <summary>
    /// Adds an asynchronous processing node to the chain, allowing transformation of elements in the sequence.
    /// </summary>
    /// <remarks>This method extends the chain by appending a node that applies the specified asynchronous
    /// transformation  function to each element in the sequence. The transformation function is invoked for each
    /// element as it  is processed.</remarks>
    /// <typeparam name="T">The type of elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The type of elements in the output sequence after transformation.</typeparam>
    /// <param name="chain">The chain of asynchronous sequences to which the processing node will be added.</param>
    /// <param name="func">A function that asynchronously transforms each element of type <typeparamref name="T"/>  into a result of type
    /// <typeparamref name="TResult"/>. The function takes the element and a  <see cref="CancellationToken"/> as
    /// parameters.</param>
    /// <returns>A new chain containing the added asynchronous processing node.</returns>
    public static Chain<IAsyncEnumerable<TResult>> AddAsyncNode<T, TResult>(this Chain<IAsyncEnumerable<T>> chain,
        Func<T, CancellationToken, Task<TResult>> func) =>
        AddAsyncNode(chain, (obj, ct, _) => func(obj, ct));

    /// <summary>
    /// Splits the elements of the asynchronous sequence into chunks of the specified size.
    /// </summary>
    /// <remarks>This method processes the asynchronous sequence lazily. Each chunk is yielded as soon as it
    /// is filled, and the operation respects the cancellation token associated with the chain.</remarks>
    /// <typeparam name="T">The type of the elements in the asynchronous sequence.</typeparam>
    /// <param name="chain">The chain containing the asynchronous sequence to be chunked.</param>
    /// <param name="size">The size of each chunk. Must be greater than zero.</param>
    /// <returns>A new chain containing an asynchronous sequence of chunks, where each chunk is a collection of elements from the
    /// original sequence. The last chunk may contain fewer elements if the total number of elements is not evenly
    /// divisible by the chunk size.</returns>
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

                if (objList.Count != 0)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return objList.AsEnumerable();
                }
            }
        }, chain.CancellationToken);
    }
}
