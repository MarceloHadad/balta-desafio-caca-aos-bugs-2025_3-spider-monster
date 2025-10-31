using BugStore.Data;
using BugStore.Handlers.Customers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Customers;

public class GetByIdCustomerHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenCustomerExists_ReturnsCustomer()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1995, 5, 15)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetByIdCustomerHandler(context);
        var request = new GetByIdCustomerRequest(customerId);

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(customerId);
        response.Name.Should().Be(customer.Name);
        response.Email.Should().Be(customer.Email);
        response.Phone.Should().Be(customer.Phone);
        response.BirthDate.Should().Be(customer.BirthDate);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetByIdCustomerHandler(context);
        var request = new GetByIdCustomerRequest(Guid.NewGuid());

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer not found");
    }
}
