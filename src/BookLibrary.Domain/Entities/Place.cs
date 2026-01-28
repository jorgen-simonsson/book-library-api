namespace BookLibrary.Domain.Entities;

public class Place
{
    public int Id { get; set; }
    public string Descr { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
