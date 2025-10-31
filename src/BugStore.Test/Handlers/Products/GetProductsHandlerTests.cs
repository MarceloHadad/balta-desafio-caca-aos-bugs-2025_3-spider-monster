using BugStore.Data;
using BugStore.Handlers.Products;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Products;

public class GetProductsHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenProductsExist_ReturnsAllProducts()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var products = new List<Product>
        {
            new() {
                Id = Guid.NewGuid(),
                Title = "Product 1",
                Description = "Description 1",
                Slug = "product-1",
                Price = 10m
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Product 2",
                Description = "Description 2",
                Slug = "product-2",
                Price = 20m
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Product 3",
                Description = "Description 3",
                Slug = "product-3",
                Price = 30m
            }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductsHandler(context);
        var request = new GetProductsRequest();

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(3);
        response.Products.Should().OnlyContain(p =>
            products.Any(original =>
                original.Id == p.Id &&
                original.Title == p.Title &&
                original.Description == p.Description &&
                original.Slug == p.Slug &&
                original.Price == p.Price));
    }

    [Fact]
    public async Task HandleAsync_WhenNoProducts_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetProductsHandler(context);
        var request = new GetProductsRequest();

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().NotBeNull();
        response.Products.Should().BeEmpty();
    }
}
