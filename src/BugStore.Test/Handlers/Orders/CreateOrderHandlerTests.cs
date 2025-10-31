using BugStore.Data;
using BugStore.Handlers.Orders;
using BugStore.Models;
using BugStore.Requests.Orders;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Orders;

public class CreateOrderHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenValidRequest_CreatesOrderAndReturnsResponse()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });

        context.Products.AddRange(
            new Product { Id = productId1, Title = "Product 1", Description = "Desc 1", Slug = "product-1", Price = 10m },
            new Product { Id = productId2, Title = "Product 2", Description = "Desc 2", Slug = "product-2", Price = 20m }
        );

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Lines =
            [
                new OrderLineRequest { ProductId = productId1, Quantity = 2 },
                new OrderLineRequest { ProductId = productId2, Quantity = 1 }
            ]
        };

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.CustomerId.Should().Be(customerId);
        response.Lines.Should().HaveCount(2);
        response.Lines[0].Quantity.Should().Be(2);
        response.Lines[0].Total.Should().Be(20m);
        response.Lines[1].Quantity.Should().Be(1);
        response.Lines[1].Total.Should().Be(20m);

        var saved = await context.Orders
            .Include(o => o.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == response.Id, TestContext.Current.CancellationToken);
        saved.Should().NotBeNull();
        saved!.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerIdEmpty_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.Empty,
            Lines =
            [
                new OrderLineRequest { ProductId = Guid.NewGuid(), Quantity = 1 }
            ]
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("CustomerId is required");
    }

    [Fact]
    public async Task HandleAsync_WhenNoLines_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Lines = []
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Order must have at least one line");
    }

    [Fact]
    public async Task HandleAsync_WhenLinesIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Order must have at least one line");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid();

        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Test Product",
            Description = "Test",
            Slug = "test",
            Price = 10m
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Lines =
            [
                new OrderLineRequest { ProductId = productId, Quantity = 1 }
            ]
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer not found");
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();

        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Lines =
            [
                new OrderLineRequest { ProductId = Guid.NewGuid(), Quantity = 1 }
            ]
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("One or more products not found");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task HandleAsync_WhenQuantityInvalid_ThrowsArgumentException(int quantity)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123",
            BirthDate = new DateTime(1990, 1, 1)
        });

        context.Products.Add(new Product
        {
            Id = productId,
            Title = "Test Product",
            Description = "Test",
            Slug = "test",
            Price = 10m
        });

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Lines =
            [
                new OrderLineRequest { ProductId = productId, Quantity = quantity }
            ]
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Quantity must be greater than zero");
    }
}
