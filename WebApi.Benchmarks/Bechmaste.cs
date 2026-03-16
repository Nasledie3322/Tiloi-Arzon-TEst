using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.Entities;
using WebApi.DTOs;
using WebApi.Services;

[MemoryDiagnoser]
public class Bechmaste
{
    private AppDbContext _context = null!;
    private ProductService _service = null!;

    [Params(100, 1000, 10000)]
    public int ProductCount { get; set; }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"tiloiarzon-products-{ProductCount}")
            .EnableSensitiveDataLogging(false)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var category = new Category
        {
            Id = 1,
            Name = "Bench Category"
        };

        var products = new List<Product>(capacity: ProductCount);
        for (var i = 1; i <= ProductCount; i++)
        {
            products.Add(new Product
            {
                Id = i,
                Name = $"Product {i}",
                Price = i % 1000,
                StockQuantity = i % 50,
                Description = "Benchmark product",
                ImageUrl = "/images/default-product.svg",
                CategoryId = category.Id,
                Category = category
            });
        }

        _context.Categories.Add(category);
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        _service = new ProductService(_context);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _context.Dispose();
    }

    [Benchmark]
    public async Task<int> ProductService_GetAllAsync()
    {
        var result = await _service.GetAllAsync(searchTerm: null, categoryId: null);
        return result is ICollection<ProductDto> c ? c.Count : result.Count();
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Bechmaste>();
    }
}
