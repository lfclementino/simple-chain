# SimpleChain

A set of extensions to implement in a simple way the Chain-of-responsibility pattern for all types of objects.

## Stack used

- .NET 8
- xUnit
- No dependencies

## Installation

Depending on your usage, follow one of the guidelines below.

### ASP.NET Core

Install with NuGet:

```
Install-Package SimpleChain
```

or with .NET CLI:

```
dotnet add package SimpleChain
```

## How to Use

You can easily create a `Chain` from every `object`, `Task`, `IEnumerable` or `IAsyncEnumerable`. All those examples are listened in the project tests:

Consider the example class `Sale`
```c#
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
```

### You can start a `Chain` directly like this:

#### `object`
```c#
var sale = new Sale();

var chain = sale.ToChain()
    .AddNode(sale => 
    {
        sale.SaleName = "Sale1";
        sale.Products = new[] 
        {
            new Product
            {
                Name = "Product1",
                Price = "10"
            },
            new Product
            {
                Name = "Product2",
                Price = "15"
            }
        };
        return sale;
    })
    .AddNode(sale => sale.Products.Sum(x => x.Price));

var total = await chain();

// Total: 25
    
```

#### `Task`
```c#
var sale = _repository.GetSale() // returns Task<Sale> type
    .ToChain()
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
```

#### `IEnumerable`
```c#
var sales = new List<Sale>();

var newSales = await sales.ToChain()
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
```

#### `IAsyncEnumerable`
```c#
var asyncSales = await _repository.GetSalesAsync() // functions returns IAsyncEnumerable<Sale>
    .ToChain()
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

await foreach(var sale in asyncSales.WithCancellationToken(ct))
{
    (...)
}
```

### Special Nodes
#### Parallel Node for `IEnumerable`

> **_NOTE:_**  You can specify the total parallelism degree or use **-1** to use total available

```c#
var sales = new List<Sale>();

var newSales = await sales.ToChain()
    .AddNode(4, sale => // telling the node to parallel process with 4 max degree
    {
        sale.Total = sale.Products.Sum(x => x.Price);
        return sale;
    })
    .AddNode(sale =>
    {
        sale.Tax = sale.Total * 0.12;
        return sale;
    });
```

#### Chunk Node for `IAsyncEnumerable`

> **_NOTE:_**  You can specify the total chunk with _**.Chunk()**_ node
```c#
var asyncSales = await _repository.GetSalesAsync() // functions returns IAsyncEnumerable<Sale>
    .ToChain()
    .Chunk(10)
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

await foreach(var chunk in asyncSales.WithCancellationToken(ct))
{
    (...)
}
```