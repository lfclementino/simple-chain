namespace SimpleChainTests.Helpers;

internal class Sale
{
    public string? SaleName { get; set; }
    public Product[] Products { get; set; } = Array.Empty<Product>();
    public double Total { get; set; }
    public double Tax { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.New;
}

internal sealed class Product
{
    public required string Name { get; set; }
    public required double Price { get; set; }
}

internal enum SaleStatus
{
    New,
    Closing,
    Closed
}