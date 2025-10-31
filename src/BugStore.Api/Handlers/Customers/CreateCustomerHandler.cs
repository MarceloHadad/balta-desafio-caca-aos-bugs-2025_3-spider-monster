using System.ComponentModel.DataAnnotations;
using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses.Customers;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Customers;

public class CreateCustomerHandler : IHandler<CreateCustomerRequest, CreateCustomerResponse>
{
    private readonly AppDbContext _context;

    public CreateCustomerHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CreateCustomerResponse> HandleAsync(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ArgumentException("Phone is required");
        if (request.BirthDate == default)
            throw new ArgumentException("BirthDate is required");
        if (request.BirthDate > DateTime.UtcNow.Date)
            throw new ArgumentException("BirthDate cannot be in the future");

        var emailAttr = new EmailAddressAttribute();
        if (!emailAttr.IsValid(request.Email))
            throw new ArgumentException("Email is invalid");

        var emailInUse = await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Email == request.Email);
        if (emailInUse)
            throw new InvalidOperationException("Email already in use");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            BirthDate = request.BirthDate
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var response = new CreateCustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            BirthDate = customer.BirthDate
        };

        return response;
    }
}