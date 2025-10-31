using BugStore.Data;
using BugStore.Handlers.Customers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Customers;

public class UpdateCustomerHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenValidRequest_UpdatesAndReturnsResponse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Old Name",
            Email = "old@example.com",
            Phone = "111111111",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "New Name",
            Email = "new@example.com",
            Phone = "999999999",
            BirthDate = new DateTime(1995, 5, 15)
        };

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(customerId);
        response.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.Phone.Should().Be(request.Phone);
        response.BirthDate.Should().Be(request.BirthDate);

        var updated = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, TestContext.Current.CancellationToken);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(request.Name);
        updated.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Test",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email is required");
    }

    [Fact]
    public async Task HandleAsync_WhenPhoneIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Test",
            Email = "test@example.com",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Phone is required");
    }

    [Fact]
    public async Task HandleAsync_WhenBirthDateIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("BirthDate is required");
    }

    [Fact]
    public async Task HandleAsync_WhenBirthDateInFuture_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = DateTime.UtcNow.Date.AddDays(1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("BirthDate cannot be in the future");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailInvalid_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.Add(new Customer
        {
            Id = customerId,
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Test",
            Email = "invalid-email",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email is invalid");
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(Guid.NewGuid())
        {
            Name = "Test",
            Email = "test@example.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer not found");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerId = Guid.NewGuid();
        context.Customers.AddRange(
            new Customer
            {
                Id = customerId,
                Name = "Customer 1",
                Email = "customer1@example.com",
                Phone = "111111111",
                BirthDate = new DateTime(1990, 1, 1)
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Customer 2",
                Email = "existing@example.com",
                Phone = "222222222",
                BirthDate = new DateTime(1995, 5, 15)
            });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateCustomerHandler(context);
        var request = new UpdateCustomerRequest(customerId)
        {
            Name = "Customer 1",
            Email = "existing@example.com",
            Phone = "111111111",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already in use");
    }
}
