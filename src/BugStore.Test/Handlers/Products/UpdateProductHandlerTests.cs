using BugStore.Data;
using BugStore.Handlers.Products;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Products;

public class UpdateProductHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenValidRequest_UpdatesAndReturnsResponse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Old Title",
            Description = "Old Description",
            Slug = "old-slug",
            Price = 50m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(context);
        var request = new UpdateProductRequest
        {
            Id = productId,
            Title = "New Title",
            Description = "New Description",
            Slug = "new-slug",
            Price = 100m
        };

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(productId);
        response.Title.Should().Be(request.Title);
        response.Description.Should().Be(request.Description);
        response.Slug.Should().Be(request.Slug);
        response.Price.Should().Be(request.Price);

        var updated = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, TestContext.Current.CancellationToken);
        updated.Should().NotBeNull();
        updated!.Title.Should().Be(request.Title);
        updated.Slug.Should().Be(request.Slug);
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Test",
            Description = "Test",
            Slug = "test",
            Price = 10m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(context);
        var request = new UpdateProductRequest
        {
            Id = productId,
            Description = "Test",
            Slug = "test",
            Price = 10m
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public async Task HandleAsync_WhenDescriptionIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Test",
            Description = "Test",
            Slug = "test",
            Price = 10m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(context);
        var request = new UpdateProductRequest
        {
            Id = productId,
            Title = "Test",
            Slug = "test",
            Price = 10m
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Description is required");
    }

    [Fact]
    public async Task HandleAsync_WhenSlugIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Test",
            Description = "Test",
            Slug = "test",
            Price = 10m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(context);
        var request = new UpdateProductRequest
        {
            Id = productId,
            Title = "Test",
            Description = "Test",
            Price = 10m
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Slug is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task HandleAsync_WhenPriceIsInvalid_ThrowsArgumentException(decimal price)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Test",
            Description = "Test",
            Slug = "test",
            Price = 10m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(context);
        var request = new UpdateProductRequest
        {
            Id = productId,
            Title = "Test",
            Description = "Test",
            Slug = "test",
            Price = price
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Price must be greater than zero");
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new UpdateProductHandler(context);
        var request = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            Slug = "test",
            Price = 10m
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task HandleAsync_WhenSlugAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();
        context.Products.AddRange(
            new Product
            {
                Id = productId,
                Title = "Product 1",
                Description = "Description 1",
                Slug = "product-1",
                Price = 10m
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Title = "Product 2",
                Description = "Description 2",
                Slug = "existing-slug",
                Price = 20m
            });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(context);
        var request = new UpdateProductRequest
        {
            Id = productId,
            Title = "Product 1",
            Description = "Description 1",
            Slug = "existing-slug",
            Price = 10m
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Slug already in use");
    }
}
