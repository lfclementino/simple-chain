namespace SimpleChain;

public abstract class ChainBase : IDisposable
{
    internal ChainBase(ChainState state)
    {
        State = state;
    }

    private bool _disposed = false;

    internal ChainState State { get; }

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the current operation.
    /// </summary>
    public CancellationToken CancellationToken => State.CancellationToken;

    /// <summary>
    /// Releases all resources used by the current instance of the object.
    /// </summary>
    /// <remarks>This method disposes of the underlying resources associated with the object's state.  Once
    /// called, the object should not be used further.</remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">Indicates whether managed resources should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                State.Dispose();
            }
            _disposed = true;
        }
    }
}
