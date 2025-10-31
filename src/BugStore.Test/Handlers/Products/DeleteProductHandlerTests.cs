using BugStore.Data;
using BugStore.Handlers.Products;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Products;

public class DeleteProductHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenProductExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Test Product",
            Description = "Test Description",
            Slug = "test-product",
            Price = 50m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteProductHandler(context);
        var request = new DeleteProductRequest { Id = productId };

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();

        var deleted = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, TestContext.Current.CancellationToken);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new DeleteProductHandler(context);
        var request = new DeleteProductRequest { Id = Guid.NewGuid() };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product not found");
    }
}
