using System.Runtime.CompilerServices;

namespace SimpleChain;

public static class ChainExtensions
{
    /// <summary>
    /// Converts a Task to a Chain.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="source">The Task to convert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Chain representing the Task.</returns>
    public static Chain<T> ToChain<T>(this Task<T> source, CancellationToken cancellationToken = default) =>
        new(source, cancellationToken);

    /// <summary>
    /// Converts a value to a Chain.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The value to convert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Chain representing the value.</returns>
    public static Chain<T> ToChain<T>(this T source, CancellationToken cancellationToken = default) =>
        Task.FromResult(source).ToChain(cancellationToken);

    /// <summary>
    /// Gets the awaiter for the Chain.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="chain">The Chain to get the awaiter for.</param>
    /// <returns>The awaiter for the Chain.</returns>
    public static TaskAwaiter<T> GetAwaiter<T>(this Chain<T> chain) => chain.Task.GetAwaiter();

    /// <summary>
    /// Gets the awaiter for the Chain.
    /// </summary>
    /// <param name="chain">The Chain to get the awaiter for.</param>
    /// <returns>The awaiter for the Chain.</returns>
    public static TaskAwaiter GetAwaiter(this Chain chain) => chain.Task.GetAwaiter();

    /// <summary>
    /// Throws an exception of type <see cref="ChainNotHandledExpection"/> when no node handled the object with a custom message.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="chain">The Chain to check.</param>
    /// <param name="message">The custom message for the exception.</param>
    /// <returns>The Chain for method chaining.</returns>
    /// <exception cref="ChainNotHandledExpection">Thrown when no node handled the object.</exception>
    public static Chain<T> ThrowIfNotHandledNode<T>(this Chain<T> chain,
        string message) =>
        chain.AddHandlerNodeInternal(task =>
            !chain.State.IsHandled ?
                throw new ChainNotHandledExpection(message) :
                Task.FromResult(false)
        , chain.CancellationToken);

    /// <summary>
    /// Throws an exception of type <see cref="ChainNotHandledExpection"/> when no node handled the object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="chain">The Chain to check.</param>
    /// <returns>The Chain for method chaining.</returns>
    /// <exception cref="ChainNotHandledExpection">Thrown when no node handled the object.</exception>
    public static Chain<T> ThrowIfNotHandledNode<T>(this Chain<T> chain) =>
        chain.AddHandlerNodeInternal(task =>
            !chain.State.IsHandled ?
                throw new ChainNotHandledExpection() :
                Task.FromResult(false)
        , chain.CancellationToken);
}