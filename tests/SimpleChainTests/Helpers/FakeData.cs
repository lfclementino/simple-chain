namespace SimpleChainTests.Helpers;

internal static class FakeData
{
    public static Sale GetFakeSale(int saleIndex = 0, int totalProducts = 1)
    {
        return new Sale
        {
            SaleName = $"TestSale_{saleIndex}",
            Products = Enumerable.Range(1, totalProducts).Select(i => GenerateFakeProduct(i)).ToArray(),
        };
    }

    public static Sale[] GetFakesSales(int totalSales = 1)
    {
        var random = new Random();
        return Enumerable.Range(1, totalSales).Select(i => GetFakeSale(i, random.Next(1, 10))).ToArray();
    }

    public async static IAsyncEnumerable<Sale> GetFakeSalesAsync(int totalSales = 1)
    {
        var sales = GetFakesSales(totalSales);
        foreach (var sale in sales)
        {
            await Task.Delay(1);
            yield return sale;
        }
    }

    private static Product GenerateFakeProduct(int index)
    {
        var random = new Random();
        return new Product
        {
            Name = $"Test_{index}",
            Price = random.Next(1, 100000)
        };
    }
}
