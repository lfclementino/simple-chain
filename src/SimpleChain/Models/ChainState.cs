namespace SimpleChain;

public sealed class ChainState : IDisposable
{
    internal ChainState()
    {
        TokenSource = new CancellationTokenSource();
    }

    internal ChainState(CancellationToken cancellationToken)
    {
        TokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }

    private CancellationTokenSource TokenSource { get; set; }

    internal CancellationToken CancellationToken => TokenSource.Token;

    /// <summary>
    /// Communicates a request for cancelling the Chain. Invokes cancellation of the <see cref="ChainState"/>
    /// </summary>
    /// <exception cref="OperationCanceledException">This Chain has been cancelled.</exception>
    public void Cancel() => TokenSource.Cancel();

    /// <summary>
    /// Communicates a request for cancelling asynchronously the Chain. Invokes asynchronous cancellation of the <see cref="CancellationToken"/>
    /// </summary>
    /// <exception cref="OperationCanceledException">This Chain has been cancelled.</exception>
    public Task CancelAsync() => TokenSource.CancelAsync();

    /// <summary>Releases the resources used by this <see cref="ChainState" />.</summary>
    /// <remarks>This method is not thread-safe for any other concurrent calls.</remarks>
    public void Dispose() => TokenSource.Dispose();

    /// <summary>Gets whether cancellation has been requested for this <see cref="ChainState" />.</summary>
    public bool IsCancellationRequested => TokenSource.IsCancellationRequested;
}