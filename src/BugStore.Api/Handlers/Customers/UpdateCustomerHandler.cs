using System.ComponentModel.DataAnnotations;
using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Customers;
using BugStore.Responses.Customers;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Customers;

public class UpdateCustomerHandler : IHandler<UpdateCustomerRequest, UpdateCustomerResponse>
{
    private readonly AppDbContext _context;

    public UpdateCustomerHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateCustomerResponse> HandleAsync(UpdateCustomerRequest request)
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

        var existingCustomer = await _context.Customers.FindAsync(request.Id)
            ?? throw new KeyNotFoundException("Customer not found");

        var emailInUse = await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Email == request.Email && c.Id != request.Id);
        if (emailInUse)
            throw new InvalidOperationException("Email already in use");

        existingCustomer.Name = request.Name;
        existingCustomer.Email = request.Email;
        existingCustomer.Phone = request.Phone;
        existingCustomer.BirthDate = request.BirthDate;

        await _context.SaveChangesAsync();

        var response = new UpdateCustomerResponse
        {
            Id = existingCustomer.Id,
            Name = existingCustomer.Name,
            Email = existingCustomer.Email,
            Phone = existingCustomer.Phone,
            BirthDate = existingCustomer.BirthDate
        };

        return response;
    }
}