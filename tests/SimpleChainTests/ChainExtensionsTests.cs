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
        var task = Task.FromResult(FakeData.GetFakeSale());

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

    [Fact]
    public async Task Chain_Using_Wrappers_Should_Run_Correctly_In_Order()
    {
        var sale = FakeData.GetFakeSale();

        await sale
            .Checkout()
            .AddTotal()
            .AddTax()
            .SetStatus(SaleStatus.Closed);

        sale.Total.Should().Be(sale.Products.Sum(x => x.Price));
        sale.Tax.Should().Be(sale.Total * 0.12);
        sale.Status.Should().Be(SaleStatus.Closed);
    }

    [Fact]
    public async Task Chain_With_Async_With_CancellationToken_Method_Should_Run_With_Break()
    {
        var sale = FakeData.GetFakeSale();

        var result = async () => await sale.ToChain()
           .AddNode(sale =>
           {
               sale.Total = sale.Products.Sum(x => x.Price);
               return sale;
           })
           .AddNode(async (sale, _, state) =>
           {
               await state.CancelAsync();
               return sale;
           })
           .AddNode((sale, ct) =>
           {
               sale.Tax = sale.Total * 0.12;
           });

        await result.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Chain_With_Async_With_CancellationToken_Method_Should_Run_With_Break_v2()
    {
        var sale = FakeData.GetFakeSale();

        var result = async () => await sale.ToChain()
            .AddNode((sale, _, state) =>
            {
                sale.Total = sale.Products.Sum(x => x.Price);
                state.Cancel();
                return sale;
            })
            .AddNode(sale =>
            {
                sale.Tax = sale.Total * 0.12;
            });

        await result.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ChainNode_Method_Should_Run()
    {
        var sale = FakeData.GetFakeSale();

        await sale.ToChain()
            .AddNode(sale =>
            {
                sale.Total = 50;
                return sale;
            })
            .AddApprover1()
            .AddApprover2()
            .ThrowIfNotHandledNode();

        sale.ApprovedBy.Should().Be("Approver 1");
    }
}