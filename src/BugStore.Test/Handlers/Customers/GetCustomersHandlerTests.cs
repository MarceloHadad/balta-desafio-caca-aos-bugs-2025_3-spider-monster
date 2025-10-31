using BugStore.Data;
using BugStore.Handlers.Customers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Customers;

public class GetCustomersHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenCustomersExist_ReturnsAllCustomers()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customers = new List<Customer>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Customer 1",
                Email = "customer1@example.com",
                Phone = "111111111",
                BirthDate = new DateTime(1990, 1, 1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Customer 2",
                Email = "customer2@example.com",
                Phone = "222222222",
                BirthDate = new DateTime(1985, 5, 15)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Customer 3",
                Email = "customer3@example.com",
                Phone = "333333333",
                BirthDate = new DateTime(2000, 12, 31)
            }
        };
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetCustomersHandler(context);
        var request = new GetCustomersRequest();

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().HaveCount(3);
        response.Customers.Should().OnlyContain(c =>
            customers.Any(original =>
                original.Id == c.Id &&
                original.Name == c.Name &&
                original.Email == c.Email &&
                original.Phone == c.Phone &&
                original.BirthDate == c.BirthDate));
    }

    [Fact]
    public async Task HandleAsync_WhenNoCustomers_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetCustomersHandler(context);
        var request = new GetCustomersRequest();

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().NotBeNull();
        response.Customers.Should().BeEmpty();
    }
}
