namespace SimpleChainTests;

public class ChainAsyncExtensionsTests
{
    [Fact]
    public void ToChain_Create_Chain_From_Object()
    {
        var sales = FakeData.GetFakeSalesAsync();

        var chain = sales.ToChain();

        chain.Should().NotBeNull();
        chain.Should().BeOfType<Chain<IAsyncEnumerable<Sale>>>();
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order()
    {
        var sales = FakeData.GetFakeSalesAsync(10);

        sales = await sales.ToChain()
            .AddNode(sale =>
            {
                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddNode(sale =>
            {
                sale.Tax = sale.Total * 0.12;
                return sale;
            });

        var newSales = sales.ToBlockingEnumerable();

        newSales.Sum(x => x.Total).Should().BeGreaterThan(0);
        newSales.Sum(x => x.Tax).Should().BeGreaterThan(0);
    }
}
