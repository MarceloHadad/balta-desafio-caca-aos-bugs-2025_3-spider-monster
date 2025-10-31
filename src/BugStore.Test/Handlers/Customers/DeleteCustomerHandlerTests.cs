using BugStore.Data;
using BugStore.Handlers.Customers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Customers;

public class DeleteCustomerHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenCustomerExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteCustomerHandler(context);
        var request = new DeleteCustomerRequest { Id = customerId };

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();

        var deleted = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, TestContext.Current.CancellationToken);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new DeleteCustomerHandler(context);
        var request = new DeleteCustomerRequest { Id = Guid.NewGuid() };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer not found");
    }
}
