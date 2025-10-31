using BugStore.Data;
using BugStore.Handlers.Products;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Products;

public class GetByIdProductHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Title = "Test Product",
            Description = "Test Description",
            Slug = "test-product",
            Price = 99.99m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetByIdProductHandler(context);
        var request = new GetByIdProductRequest(productId);

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(productId);
        response.Title.Should().Be(product.Title);
        response.Description.Should().Be(product.Description);
        response.Slug.Should().Be(product.Slug);
        response.Price.Should().Be(product.Price);
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetByIdProductHandler(context);
        var request = new GetByIdProductRequest(Guid.NewGuid());

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product not found");
    }
}
