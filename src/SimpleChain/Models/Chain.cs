namespace SimpleChain;

public sealed class Chain<T>
{
    public required CancellationToken CancellationToken { get; init; }
    public required Task<T> Task { get; init; }
}

public sealed class Chain
{
    public required CancellationToken CancellationToken { get; init; }
    public required Task Task { get; init; }
}
