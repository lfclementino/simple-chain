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

    public static Chain<Sale> AddApprover1(this Chain<Sale> chain)
    {
        return chain.AddHandlerNode(sale =>
        {
            if (sale.Total is > 0 and < 100)
            {
                sale.ApprovedBy = "Approver 1";
                return true;
            }
            return false;
        });
    }

    public static Chain<Sale> AddApprover2(this Chain<Sale> chain)
    {
        return chain.AddHandlerNode(sale =>
        {
            if (sale.Total is > 100 and <= 1000)
            {
                sale.ApprovedBy = "Approver 2";
                return true;
            }
            return false;
        });
    }
}
