using BugStore.Data;
using BugStore.Handlers.Products;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Products;

public class CreateProductHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenValidRequest_PersistsAndReturnsResponse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateProductHandler(context);
        var request = new CreateProductRequest
        {
            Title = "Test Product",
            Description = "Test Description",
            Slug = "test-product",
            Price = 99.99m
        };

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Title.Should().Be(request.Title);
        response.Description.Should().Be(request.Description);
        response.Slug.Should().Be(request.Slug);
        response.Price.Should().Be(request.Price);

        var saved = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == response.Id, TestContext.Current.CancellationToken);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be(request.Title);
        saved.Slug.Should().Be(request.Slug);
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateProductHandler(context);
        var request = new CreateProductRequest
        {
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
        var handler = new CreateProductHandler(context);
        var request = new CreateProductRequest
        {
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
        var handler = new CreateProductHandler(context);
        var request = new CreateProductRequest
        {
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
    [InlineData(-10.50)]
    public async Task HandleAsync_WhenPriceIsInvalid_ThrowsArgumentException(decimal price)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateProductHandler(context);
        var request = new CreateProductRequest
        {
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
    public async Task HandleAsync_WhenSlugAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Title = "Existing Product",
            Description = "Existing Description",
            Slug = "existing-slug",
            Price = 50m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateProductHandler(context);
        var request = new CreateProductRequest
        {
            Title = "New Product",
            Description = "New Description",
            Slug = "existing-slug",
            Price = 100m
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Slug already in use");
    }
}
