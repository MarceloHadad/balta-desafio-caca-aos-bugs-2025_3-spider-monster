using BugStore.Data;
using BugStore.Handlers.Customers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BugStore.Test.Handlers.Customers;

public class CreateCustomerHandlerTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task HandleAsync_WhenValidRequest_PersistsAndReturnsResponse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = "+55 11 99999-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.Phone.Should().Be(request.Phone);
        response.BirthDate.Should().Be(request.BirthDate);

        var saved = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == response.Id, TestContext.Current.CancellationToken);
        saved.Should().NotBeNull();
        saved!.Email.Should().Be(request.Email);
        saved.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Email = "test@example.com",
            Phone = "123",
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
        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Name = "John",
            Phone = "123",
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
        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Name = "John",
            Email = "john@example.com",
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
        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Name = "John",
            Email = "john@example.com",
            Phone = "123",
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
        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Name = "John",
            Email = "john@example.com",
            Phone = "123",
            BirthDate = DateTime.UtcNow.Date.AddDays(1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("BirthDate cannot be in the future");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    public async Task HandleAsync_WhenEmailInvalid_ThrowsArgumentException(string email)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Name = "John",
            Email = email,
            Phone = "123",
            BirthDate = new DateTime(1990, 1, 1)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Existing Customer",
            Email = "existing@example.com",
            Phone = "1234567890",
            BirthDate = new DateTime(1980, 1, 1)
        });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateCustomerHandler(context);
        var request = new CreateCustomerRequest
        {
            Name = "New Customer",
            Email = "existing@example.com",
            Phone = "111",
            BirthDate = new DateTime(1995, 5, 5)
        };

        // Act
        var act = async () => await handler.HandleAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already in use");
    }
}