namespace NERBABO.ApiService.Shared.Models;

public class Entity<T>
{
    public T? Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
