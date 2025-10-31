namespace BugStore.Requests.Products;

public class GetByIdProductRequest
{
    public Guid Id { get; set; }

    public GetByIdProductRequest(Guid id)
    {
        Id = id;
    }
}