namespace SimpleChainTests;

public class ChainEnumerableExtensionsTests
{
    [Fact]
    public void ToChain_Create_Chain_From_Object()
    {
        var sale = FakeData.GetFakesSales();

        var chain = sale.ToChain();

        chain.Should().NotBeNull();
        chain.Should().BeOfType<Chain<Sale[]>>();
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order_With_Parallelism()
    {
        var sales = FakeData.GetFakesSales(100);

        var chain = sales.ToChain();

        sales.Sum(x => x.Total).Should().Be(0);
        sales.Sum(x => x.Tax).Should().Be(0);

        var newSales = await chain.AddNode(sales =>
            {
                sales.AsParallel().ForAll(sale =>
                {
                    sale.Total = sale.Products.Sum(x => x.Price);
                }
                );
                return sales;
            })
            .AddNode(sales =>
            {
                sales.AsParallel().ForAll(sale =>
                {
                    sale.Tax = sale.Total * 0.12;
                }
                );
                return sales;
            });

        newSales.Sum(x => x.Total).Should().Be(sales.Sum(y => y.Products.Sum(z => z.Price)));
        newSales.Sum(x => x.Tax).Should().Be(sales.Sum(y => y.Total * 0.12));
    }

    [Fact]
    public async Task Chain_Should_Run_Correctly_In_Order_Without_Parallelism()
    {
        var sales = FakeData.GetFakesSales(100);

        var chain = sales.ToChain();

        sales.Sum(x => x.Total).Should().Be(0);
        sales.Sum(x => x.Tax).Should().Be(0);

        var newSales = await chain.AddNode(sales =>
            {
                return sales.Select(sale =>
                {
                    sale.Total = sale.Products.Sum(x => x.Price);
                    return sale;
                });
            })
            .AddNode(sales =>
            {
                return sales.Select(sale =>
                {
                    sale.Tax = sale.Total * 0.12;
                    return sale;
                });
            });

        newSales.Sum(x => x.Total).Should().Be(sales.Sum(y => y.Products.Sum(z => z.Price)));
        newSales.Sum(x => x.Tax).Should().Be(sales.Sum(y => y.Total * 0.12));
    }
}
