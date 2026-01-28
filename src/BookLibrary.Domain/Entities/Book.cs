using System.Text.Json;

namespace BookLibrary.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string PublishedYear { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public int? PlaceId { get; set; }
    public JsonDocument? ApiInfo { get; set; }
    
    // Navigation property
    public Place? Place { get; set; }
}
