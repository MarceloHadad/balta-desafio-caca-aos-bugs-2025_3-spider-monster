using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses.Customers;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Customers;

public class GetByIdCustomerHandler : IHandler<GetByIdCustomerRequest, GetByIdCustomerResponse>
{
    private readonly AppDbContext _context;

    public GetByIdCustomerHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GetByIdCustomerResponse> HandleAsync(GetByIdCustomerRequest request)
    {
        var customer = await _context.Customers.
            AsNoTracking().
            FirstOrDefaultAsync(c => c.Id == request.Id)
                ?? throw new KeyNotFoundException("Customer not found");

        var response = new GetByIdCustomerResponse
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