# SimpleChain

[![NuGet version (SimpleChain)](https://img.shields.io/nuget/v/SimpleChain.svg?style=flat-square)](https://www.nuget.org/packages/SimpleChain/) [![GitHub license](https://img.shields.io/github/license/lfclementino/simple-chain.svg)](https://github.com/lfclementino/simple-chain/blob/master/LICENSE)

A set of extensions to implement in a simple way the `Pipeline` or `Chain-of-responsibility` patterns for all types of objects.

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
    public string? ApprovedBy { get; set; }
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
```

### &rarr; `Chain-of-responsability` pattern

You can easily create a chain of responsability like this:

Using the `.AddHandlerNode` node, you should return `true` or `false`. When it returns `true` it will ignore the next nodes.

```c#
var sale = new Sale();

await sale.ToChain()
    .AddNode(sale =>
    {
        sale.Total = 50;
        return sale;
    })
    .AddApprover1()
    .AddApprover2()
    .ThrowIfNotHandledNode();

    /// sale.ApprovedBy == "Approver 1"
```

### &rarr; `Pipeline` pattern

You can wrap functions and write like this with the following wrapping class:

```c#
var sale = new Sale();

await sale
    .Checkout()
    .AddTotal()
    .AddTax()
    .SetStatus(SaleStatus.Closed);
```



#### `Any object`
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
    .AddNode(sales =>
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
```

#### `IAsyncEnumerable`
```c#
var asyncSales = await _repository.GetSalesAsync() // functions returns IAsyncEnumerable<Sale>
    .ToChain()
    .AddNode(async sales =>
    {
        await foreach(var sale in sales)
        {
            sale.Total = sale.Products.Sum(x => x.Price);
            yield return sale;
        }
    })
    .AddNode(async sales =>
    {
        await foreach(var sale in sales)
        {
            sale.Tax = sale.Total * 0.12;
            yield return sale;
        }
    });

await foreach(var sale in asyncSales.WithCancellationToken(ct))
{
    (...)
}
```

##### Or you can use `.AddAsyncNode()` directly:
```c#
var asyncSales = await _repository.GetSalesAsync() // functions returns IAsyncEnumerable<Sale>
    .ToChain()
    .AddAsyncNode(sale =>
    {
        sale.Total = sale.Products.Sum(x => x.Price);
        return sale;
    })
    .AddAsyncNode(sale =>
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

#### Chunk Node for `IAsyncEnumerable`

> **_NOTE:_**  You can specify the chunk size with _**.Chunk()**_ node
```c#
var asyncSales = await _repository.GetSalesAsync() // functions returns IAsyncEnumerable<Sale>
    .ToChain()
    .Chunk(10)
    .AddAsyncNode(chunk =>
    {
        return chunk.Select(sale => 
        {
            sale.Total = sale.Products.Sum(x => x.Price);
            return sale;
        });
    })
    .AddAsyncNode(chunk =>
    {
        return chunk.Select(sale => 
        {
            sale.Tax = sale.Total * 0.12;
            return sale;
        });
    });

await foreach(var chunk in asyncSales.WithCancellationToken(ct))
{
    (...)
}
```

### Wrapping Node functions

You can wrap functions and write like this with the following wrapping class:

```c#
var sale = new Sale();

await sale
    .Checkout()
    .AddTotal()
    .AddTax()
    .SetStatus(SaleStatus.Closed);
```

```c#
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
}
```

```c#
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
```