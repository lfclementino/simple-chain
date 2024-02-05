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

        var controlList = new ConcurrentBag<Sale>();

        var chain = await sales.ToChain()
            .AddAsyncNode(sale =>
            {
                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddAsyncNode(async (sale, ct) =>
            {
                await Task.Delay(1, ct);
                return sale;
            })
            .AddAsyncNode(sale =>
            {
                sale.Tax = sale.Total * 0.12;
                controlList.Add(sale);
                return sale;
            });

        var sumTotal = 0.0;
        var sumTax = 0.0;

        await foreach (var sale in chain)
        {
            sale.Total = sale.Products.Sum(x => x.Price);
            sale.Tax = sale.Total * 0.12;

            sumTotal += sale.Total;
            sumTax += sale.Tax;
        }

        var expectedSumTax = Math.Round(controlList.Sum(y => y.Total * 0.12), 2);
        var expectedSumTotal = controlList.Sum(y => y.Products.Sum(z => z.Price));

        sumTotal.Should().Be(expectedSumTotal);
        Math.Round(sumTax, 2).Should().Be(expectedSumTax);
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order_With_Stop()
    {
        var sales = FakeData.GetFakeSalesAsync(10);

        var result = await sales.ToChain()
            .AddAsyncNode(sale =>
            {
                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddAsyncNode(async (sale, _, state) =>
            {
                await state.CancelAsync();
                return sale;
            })
            .AddAsyncNode(sale =>
            {
                sale.Tax = sale.Total * 0.12;
                return sale;
            });

        var newSales = () => result.ToBlockingEnumerable().ToList();

        newSales.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order_With_Stop_v2()
    {
        var sales = FakeData.GetFakeSalesAsync(10);

        var result = await sales.ToChain()
            .AddAsyncNode((sale, _, state) =>
            {
                state.Cancel();
                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddAsyncNode((sale, ct) =>
            {
                sale.Tax = sale.Total * 0.12;
                return sale;
            });

        var newSales = () => result.ToBlockingEnumerable().ToList();

        newSales.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order_With_Stop_v3()
    {
        var sales = FakeData.GetFakeSalesAsync(10);

        var result = await sales.ToChain()
            .AddAsyncNode(async (sale, ct, state) =>
            {
                await Task.Delay(1, ct);
                state.Cancel();
                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddAsyncNode(sale =>
            {
                sale.Tax = sale.Total * 0.12;
                return sale;
            });

        var newSales = () => result.ToBlockingEnumerable().ToList();

        newSales.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order_With_Stop_v4()
    {
        var sales = FakeData.GetFakeSalesAsync(10);

        var result = await sales.ToChain()
            .AddAsyncNode(async (sale, ct, state) =>
            {
                await Task.Delay(1, ct);

                if (!state.IsCancellationRequested)
                    state.Cancel();

                sale.Total = sale.Products.Sum(x => x.Price);
                return sale;
            })
            .AddAsyncNode((sale, ct) =>
            {
                sale.Tax = sale.Total * 0.12;
                return sales;
            });

        var newSales = () => result.ToBlockingEnumerable().ToList();

        newSales.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_With_Chunk_In_Order()
    {
        var sales = FakeData.GetFakeSalesAsync(10);

        var controlList = new ConcurrentBag<Sale>();

        var chain = await sales.ToChain()
            .Chunk(3)
            .AddAsyncNode(chunk =>
            {
                chunk.Count().Should().BeLessThanOrEqualTo(3);
                foreach (var sale in chunk)
                {
                    sale.Total = sale.Products.Sum(x => x.Price);
                }
                return chunk;
            })
            .AddAsyncNode(chunk =>
            {
                foreach (var sale in chunk)
                {
                    sale.Tax = sale.Total * 0.12;
                    controlList.Add(sale);
                }
                return chunk;
            });

        var sumTotal = 0.0;
        var sumTax = 0.0;

        await foreach (var chunk in chain)
        {
            foreach (var sale in chunk)
            {
                sale.Total = sale.Products.Sum(x => x.Price);
                sale.Tax = sale.Total * 0.12;

                sumTotal += sale.Total;
                sumTax += sale.Tax;
            }
        }

        var expectedSumTax = Math.Round(controlList.Sum(y => y.Total * 0.12), 2);
        var expectedSumTotal = controlList.Sum(y => y.Products.Sum(z => z.Price));

        sumTotal.Should().Be(expectedSumTotal);
        Math.Round(sumTax, 2).Should().Be(expectedSumTax);
    }
}
