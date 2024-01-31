namespace SimpleChainTests;

public class ChainExtensionsTests
{
    [Fact]
    public void ToChain_Create_Chain_From_Object()
    {
        var sale = FakeData.GetFakeSale();

        var chain = sale.ToChain();

        chain.Should().NotBeNull();
        chain.Should().BeOfType<Chain<Sale>>();
    }

    [Fact]
    public void ToChain_Create_Chain_From_Task()
    {
        var task = Task.Run(() => FakeData.GetFakeSale());

        var chain = task.ToChain();

        chain.Should().NotBeNull();
        chain.Should().BeOfType<Chain<Sale>>();
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order()
    {
        var sale = FakeData.GetFakeSale();

        var chain = sale.ToChain();

        sale.Total.Should().Be(0);
        sale.Tax.Should().Be(0);

        sale = await chain.AddNode(sale =>
            {
                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddNode(sale =>
            {
                sale.Tax = sale.Total * 0.12;
                return sale;
            });

        sale.Total.Should().Be(sale.Products.Sum(x => x.Price));
        sale.Tax.Should().Be(sale.Total * 0.12);
    }

    [Fact]
    public async Task Chain_With_Async_With_CancellationToken_Method_Should_Run_Correctly_In_Order()
    {
        var sale = FakeData.GetFakeSale();

        await sale.ToChain()
            .AddNode(sale =>
            {
                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddNode(async (sale, ct) =>
            {
                await Task.Delay(1, ct);
                return sale;
            })
            .AddNode(sale =>
            {
                sale.Tax = sale.Total * 0.12;
            });

        sale.Total.Should().Be(sale.Products.Sum(x => x.Price));
        sale.Tax.Should().Be(sale.Total * 0.12);
    }
}