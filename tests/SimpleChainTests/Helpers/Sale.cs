namespace SimpleChainTests.Helpers;

internal class Sale
{
    public string? SaleName { get; set; }
    public Product[] Products { get; set; } = Array.Empty<Product>();
    public double Total { get; set; }
    public double Tax { get; set; }
}

internal sealed class Product
{
    public required string Name { get; set; }
    public required double Price { get; set; }
}