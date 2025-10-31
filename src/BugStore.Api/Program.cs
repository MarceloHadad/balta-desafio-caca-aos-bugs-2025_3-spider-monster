using BugStore.Data;
using BugStore.Endpoints;
using BugStore.Handlers.Customers;
using BugStore.Handlers.Orders;
using BugStore.Handlers.Products;
using BugStore.Interfaces;
using BugStore.Requests.Customers;
using BugStore.Requests.Orders;
using BugStore.Requests.Products;
using BugStore.Responses.Customers;
using BugStore.Responses.Orders;
using BugStore.Responses.Products;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IHandler<CreateCustomerRequest, CreateCustomerResponse>, CreateCustomerHandler>();
builder.Services.AddScoped<IHandler<DeleteCustomerRequest, DeleteCustomerResponse>, DeleteCustomerHandler>();
builder.Services.AddScoped<IHandler<GetByIdCustomerRequest, GetByIdCustomerResponse>, GetByIdCustomerHandler>();
builder.Services.AddScoped<IHandler<GetCustomersRequest, GetCustomersResponse>, GetCustomersHandler>();
builder.Services.AddScoped<IHandler<UpdateCustomerRequest, UpdateCustomerResponse>, UpdateCustomerHandler>();

builder.Services.AddScoped<IHandler<CreateProductRequest, CreateProductResponse>, CreateProductHandler>();
builder.Services.AddScoped<IHandler<DeleteProductRequest, DeleteProductResponse>, DeleteProductHandler>();
builder.Services.AddScoped<IHandler<GetByIdProductRequest, GetByIdProductResponse>, GetByIdProductHandler>();
builder.Services.AddScoped<IHandler<GetProductsRequest, GetProductsResponse>, GetProductsHandler>();
builder.Services.AddScoped<IHandler<UpdateProductRequest, UpdateProductResponse>, UpdateProductHandler>();

builder.Services.AddScoped<IHandler<CreateOrderRequest, CreateOrderResponse>, CreateOrderHandler>();
builder.Services.AddScoped<IHandler<GetByIdOrderRequest, GetByIdOrderResponse>, GetByIdOrderHandler>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (KeyNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.MapCustomersEndpoints();
app.MapProductsEndpoints();
app.MapOrdersEndpoints();

app.Run();
