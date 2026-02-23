namespace Spur.SampleApi.Domain;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}
