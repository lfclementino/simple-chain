namespace SimpleChain;
public class ChainNotHandledExpection : Exception
{   
    public ChainNotHandledExpection() 
    { }

    public ChainNotHandledExpection(string message) : base(message) 
    { }
}

