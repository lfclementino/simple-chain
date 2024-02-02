namespace SimpleChainTests.Helpers;

internal static class Wrappers
{
    public static Chain<Sale> Checkout(this Sale sale, CancellationToken cancellationToken = default)
    {
        return sale.ToChain(cancellationToken)
            .SetStatus(SaleStatus.Closing);
    }

    public static Chain<Sale> SetStatus(this Chain<Sale> chain, SaleStatus status)
    {
        return chain.AddNode(sale =>
        {
            sale.Status = status;
            return sale;
        });
    }

    public static Chain<Sale> AddTotal(this Chain<Sale> chain)
    {
        return chain.AddNode(sale =>
        {
            sale.Total = sale.Products.Sum(x => x.Price);
            return sale;
        });
    }

    public static Chain<Sale> AddTax(this Chain<Sale> chain)
    {
        return chain.AddNode(sale =>
        {
            sale.Tax = sale.Total * 0.12;
            return sale;
        });
    }
}
