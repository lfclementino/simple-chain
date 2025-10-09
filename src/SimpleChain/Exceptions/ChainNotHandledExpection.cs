namespace SimpleChain;

/// <summary>
/// Represents an exception that is thrown when a chain of operations fails to handle a specific case or input.
/// </summary>
/// <remarks>This exception is typically used in scenarios where a chain of responsibility pattern is implemented,
/// and no handler in the chain is able to process the given input or request.</remarks>
public class ChainNotHandledExpection : Exception
{
    public ChainNotHandledExpection()
    { }

    public ChainNotHandledExpection(string message) : base(message)
    { }
}

