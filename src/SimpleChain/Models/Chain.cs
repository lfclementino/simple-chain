namespace SimpleChain;

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
}

public sealed class Chain : ChainBase
{
    internal Chain(Task task, ChainState state) : base(state)
    {
        Task = task;
    }

    internal Task Task { get; }
}

public abstract class ChainBase
{
    internal ChainBase(ChainState state) 
    {
        State = state;
    }

    public CancellationToken CancellationToken => State.CancellationToken;

    internal ChainState State { get; }
}