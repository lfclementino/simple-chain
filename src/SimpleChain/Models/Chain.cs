namespace SimpleChain;

/// <summary>
/// Represents a chain of asynchronous operations that can be composed and executed sequentially.
/// </summary>
/// <remarks>The <see cref="Chain{T}"/> class provides methods to add nodes to the chain, where each node
/// represents an asynchronous or synchronous operation. Nodes can be added with various types of delegates, including
/// functions and actions, and can optionally handle cancellation tokens or chain state. The chain ensures that each
/// node is executed in sequence, and the result of one node can be passed to the next.  This class is designed to
/// facilitate the creation of complex workflows by chaining together multiple operations, while also supporting
/// cancellation and state management.</remarks>
/// <typeparam name="T">The type of the result produced by the current chain node.</typeparam>
public sealed class Chain<T> : ChainBase
{
    internal Chain(Task<T> task, ChainState state) : base(state)
    {
        Task = task;
    }

    internal Chain(Task<T> task, CancellationToken cancellationToken) :
        base(new ChainState(cancellationToken))
    {
        Task = task;
    }

    internal Task<T> Task { get; }

    /// <summary>
    /// Adds a node with a function that receives the current value, cancellation token, and chain state, and produces a result.
    /// </summary>
    /// <typeparam name="TResult">The result type of the new node.</typeparam>
    /// <param name="func">The function to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain<TResult> AddNode<TResult>(
       Func<T, CancellationToken, ChainState, TResult> func) =>
       AddNodeInternal(async task =>
           func(await task, CancellationToken, State),
           CancellationToken);

    /// <summary>
    /// Adds a node with a function that receives the current value and cancellation token, producing a result.
    /// </summary>
    /// <typeparam name="TResult">The result type of the new node.</typeparam>
    /// <param name="func">The function to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain<TResult> AddNode<TResult>(
        Func<T, CancellationToken, TResult> func) =>
        AddNode((obj, ct, _) => func(obj, ct));

    /// <summary>
    /// Adds a node with a function that receives the current value, producing a result.
    /// </summary>
    /// <typeparam name="TResult">The result type of the new node.</typeparam>
    /// <param name="func">The function to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain<TResult> AddNode<TResult>(
        Func<T, TResult> func) =>
        AddNode((obj, _, _) => func(obj));

    /// <summary>
    /// Adds an asynchronous node with a function that receives the current value, cancellation token, and chain state, producing a result task.
    /// </summary>
    /// <typeparam name="TResult">The result type of the new node.</typeparam>
    /// <param name="func">The asynchronous function to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain<TResult> AddNode<TResult>(
        Func<T, CancellationToken, ChainState, Task<TResult>> func) =>
        AddNodeInternal(async task =>
            await func(await task, CancellationToken, State),
            CancellationToken);

    /// <summary>
    /// Adds an asynchronous node with a function that receives the current value and cancellation token, producing a result task.
    /// </summary>
    /// <typeparam name="TResult">The result type of the new node.</typeparam>
    /// <param name="func">The asynchronous function to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain<TResult> AddNode<TResult>(
        Func<T, CancellationToken, Task<TResult>> func) =>
        AddNode((obj, ct, _) => func(obj, ct));

    /// <summary>
    /// Adds an asynchronous node with a function that receives the current value, producing a result task.
    /// </summary>
    /// <typeparam name="TResult">The result type of the new node.</typeparam>
    /// <param name="func">The asynchronous function to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain<TResult> AddNode<TResult>(
        Func<T, Task<TResult>> func) =>
        AddNode((obj, _, _) => func(obj));

    /// <summary>
    /// Adds a node with an action that receives the current value, cancellation token, and chain state.
    /// </summary>
    /// <param name="action">The action to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain AddNode(
        Action<T, CancellationToken, ChainState> action) =>
        AddNodeInternal(async task =>
            action(await task, CancellationToken, State),
            CancellationToken);

    /// <summary>
    /// Adds a node with an action that receives the current value and cancellation token.
    /// </summary>
    /// <param name="action">The action to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain AddNode(
        Action<T, CancellationToken> action) =>
        AddNode((obj, ct, _) => action(obj, ct));

    /// <summary>
    /// Adds a node with an action that receives the current value.
    /// </summary>
    /// <param name="action">The action to add as a node.</param>
    /// <returns>A new chain with the added node.</returns>
    public Chain AddNode(Action<T> action) =>
        AddNode((obj, _, _) => action(obj));

    /// <summary>
    /// Adds a handler node with a function that receives the current value and cancellation token, returning a bool.
    /// </summary>
    /// <param name="func">The function to add as a handler node.</param>
    /// <returns>A new chain with the added handler node.</returns>
    public Chain<T> AddHandlerNode(
        Func<T, CancellationToken, bool> func) =>
        AddHandlerNodeInternal(async task =>
            func(await task, CancellationToken),
            CancellationToken);

    /// <summary>
    /// Adds a handler node with a function that receives the current value, returning a bool.
    /// </summary>
    /// <param name="func">The function to add as a handler node.</param>
    /// <returns>A new chain with the added handler node.</returns>
    public Chain<T> AddHandlerNode(
        Func<T, bool> func) =>
            AddHandlerNode((obj, _) => func(obj));

    /// <summary>
    /// Adds an asynchronous handler node with a function that receives the current value and cancellation token, returning a task of bool.
    /// </summary>
    /// <param name="func">The asynchronous function to add as a handler node.</param>
    /// <returns>A new chain with the added handler node.</returns>
    public Chain<T> AddHandlerNode(
        Func<T, CancellationToken, Task<bool>> func) =>
        AddHandlerNodeInternal(async task =>
            await func(await task, CancellationToken),
            CancellationToken);

    /// <summary>
    /// Adds an asynchronous handler node with a function that receives the current value, returning a task of bool.
    /// </summary>
    /// <param name="func">The asynchronous function to add as a handler node.</param>
    /// <returns>A new chain with the added handler node.</returns>
    public Chain<T> AddHandlerNode(
        Func<T, Task<bool>> func) =>
        AddHandlerNode((obj, _) => func(obj));

    /// <summary>
    /// Adds a handler node with an action that receives the current value and cancellation token.
    /// </summary>
    /// <param name="action">The action to add as a handler node.</param>
    /// <returns>A new chain with the added handler node.</returns>
    public Chain AddHandlerNode(
        Action<T, CancellationToken> action) =>
        AddHandlerNodeInternal(async task =>
            action(await task, CancellationToken),
            CancellationToken);

    /// <summary>
    /// Adds a handler node with an action that receives the current value.
    /// </summary>
    /// <param name="action">The action to add as a handler node.</param>
    /// <returns>A new chain with the added handler node.</returns>
    public Chain AddHandlerNode(
        Action<T> action) =>
        AddHandlerNode((obj, _) => action(obj));

    internal Chain<TResult> AddNodeInternal<TResult>(
        Func<Task<T>, Task<TResult>> func, CancellationToken cancellationToken) =>
        new(Task.ContinueWith(task =>
        {
            CancellationToken.ThrowIfCancellationRequested();
            return func(task);
        }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap(),
            State);

    internal Chain AddNodeInternal(
        Action<Task<T>> action, CancellationToken cancellationToken) =>
        new(Task.ContinueWith(task =>
        {
            CancellationToken.ThrowIfCancellationRequested();
            action(task);
        }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default),
            State);

    internal Chain<T> AddHandlerNodeInternal(
        Func<Task<T>, Task<bool>> func, CancellationToken cancellationToken) =>
        new(Task.ContinueWith(async task =>
        {
            var obj = await task;
            if (!State.IsHandled)
            {
                var result = await func(task);
                if (result) State.SetHandled();
            }
            return obj;
        }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap(),
            State);

    internal Chain AddHandlerNodeInternal(
        Action<Task<T>> action, CancellationToken cancellationToken) =>
        new(Task.ContinueWith(task =>
        {
            if (!State.IsHandled)
            {
                action(task);
            }
        }, cancellationToken,
                TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default),
            State);
}

/// <summary>
/// Represents a chain of asynchronous operations that are executed in sequence.
/// </summary>
/// <remarks>The <see cref="Chain"/> class is used to manage and track the state of a sequence of tasks. It is
/// derived from <see cref="ChainBase"/> and provides additional functionality for handling asynchronous
/// operations.</remarks>
public sealed class Chain : ChainBase
{
    internal Chain(Task task, ChainState state) : base(state)
    {
        Task = task;
    }

    internal Task Task { get; }
}
