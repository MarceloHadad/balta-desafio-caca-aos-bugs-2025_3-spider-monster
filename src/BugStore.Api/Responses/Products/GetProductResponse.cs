namespace BugStore.Responses.Products;

public class GetProductsResponse
{
    public List<GetByIdProductResponse> Products { get; set; } = [];
}