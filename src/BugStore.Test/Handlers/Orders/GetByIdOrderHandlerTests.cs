using BugStore.Data;
using BugStore.Handlers.Orders;
using BugStore.Models;
using BugStore.Requests.Orders;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Orders;

public class GetByIdOrderHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenOrderExists_ReturnsOrderWithDetails()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };

        var product1 = new Product
        {
            Id = productId1,
            Title = "Product 1",
            Description = "Description 1",
            Slug = "product-1",
            Price = 10m
        };

        var product2 = new Product
        {
            Id = productId2,
            Title = "Product 2",
            Description = "Description 2",
            Slug = "product-2",
            Price = 20m
        };

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Lines =
            [
                new OrderLine
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = productId1,
                    Quantity = 2,
                    Total = 20m
                },
                new OrderLine
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = productId2,
                    Quantity = 1,
                    Total = 20m
                }
            ]
        };

        context.Customers.Add(customer);
        context.Products.AddRange(product1, product2);
        context.Orders.Add(order);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetByIdOrderHandler(context);
        var request = new GetByIdOrderRequest(orderId);

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(orderId);
        response.CustomerId.Should().Be(customerId);
        response.CustomerName.Should().Be(customer.Name);
        response.Lines.Should().HaveCount(2);
        response.TotalAmount.Should().Be(40m);

        var line1 = response.Lines.First(l => l.ProductId == productId1);
        line1.ProductTitle.Should().Be(product1.Title);
        line1.Quantity.Should().Be(2);
        line1.UnitPrice.Should().Be(product1.Price);
        line1.Total.Should().Be(20m);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetByIdOrderHandler(context);
        var request = new GetByIdOrderRequest(Guid.NewGuid());

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Order not found");
    }
}
