using BugStore.Interfaces;
using BugStore.Requests.Products;
using BugStore.Responses.Products;

namespace BugStore.Endpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/products")
            .WithTags("Products");

        group.MapGet("/", async (IHandler<GetProductsRequest, GetProductsResponse> handler) =>
        {
            var request = new GetProductsRequest();
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (Guid id, IHandler<GetByIdProductRequest, GetByIdProductResponse> handler) =>
        {
            var request = new GetByIdProductRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async (IHandler<CreateProductRequest, CreateProductResponse> handler, CreateProductRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/products/{response.Id}", response);
        });

        group.MapPut("/{id}", async (Guid id, UpdateProductRequest request, IHandler<UpdateProductRequest, UpdateProductResponse> handler) =>
        {
            request.Id = id;
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapDelete("/{id}", async (Guid id, IHandler<DeleteProductRequest, DeleteProductResponse> handler) =>
        {
            var request = new DeleteProductRequest { Id = id };
            var response = await handler.HandleAsync(request);
            return Results.NoContent();
        });
    }
}